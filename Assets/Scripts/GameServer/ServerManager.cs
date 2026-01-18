using System.Collections.Generic;
using GameLogic;
using Network;
using Unit;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace GameServer
{
    ///<总结>
    ///顶级服务器脚本，用于管理新连接，并将玩家分配到正确的比赛中
    ///还将接收游戏动作并将其发送到相应的游戏
    ///可以同时包含多个游戏（GameServer）
    ///</摘要>
    public class ServerManager:MonoBehaviour
    {
        [Header("API")]
        public string apiUsername;
        public string apiPassword;
        
        private Dictionary<ulong, ClientData> clientList = new Dictionary<ulong, ClientData>();  //List of clients
        private Dictionary<string, Gameserver> gameList = new Dictionary<string, Gameserver>(); //List of games
        private List<string> gameRemoveList = new List<string>();
        
        private float loginTimer = 0f;

        protected virtual void Awake()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 200; //Limit server frame rate to prevent using 100% cpu
        }

        protected virtual void Start()
        {
            TcgNetwork network = TcgNetwork.Get();
            network.onClientJoin += OnClientConnected;
            network.onClientQuit += OnClientDisconnected;
            Messaging.ListenMsg("connect", ReceiveConnectPlayer);
            Messaging.ListenMsg("action", ReceiveGameAction);
            
            if (!network.IsActive())
            {
                network.StartServer(NetworkData.Get().port);
            }

            Login();
        }

        protected virtual void Update()
        {
            //Update games and Destroy games with no players
            foreach (var pair in gameList)
            {
                Gameserver gserver = pair.Value;
                gserver.Update();
                
                if (gserver.IsGameExpired())
                    gameRemoveList.Add(pair.Key);
            }

            foreach (string key in gameRemoveList)
            {
                gameList.Remove(key);
                if (ServerMatchmaker.Get())
                    ServerMatchmaker.Get().EndMatch(key);
            }
            gameRemoveList.Clear();
            
            //Re login
            loginTimer += Time.deltaTime;
            if (loginTimer > 15f && !Authenticator.Get().IsConnected())
            {
                loginTimer = 0f;
                Login();
            }
        }

        protected virtual async void Login()
        {
            await Authenticator.Get().Login(apiUsername, apiPassword);
            
            bool success = Authenticator.Get().IsConnected();
            int permission = Authenticator.Get().GetPermission();
            string api = Authenticator.Get().IsApi() ? "API" : "Local";
            
            Debug.Log(api + " authentication: " + success + " (" + permission + ")");
            
            //If login fail, login again
            if (!success)
            {
                TimeTool.WaitFor(5f, () =>
                {
                    if (!Authenticator.Get().IsConnected())
                        Login();
                });
            }
        }

        protected virtual void OnClientConnected(ulong clientID)
        {
             ClientData iclient = new ClientData(clientID);
             clientList[clientID] = iclient;
        }

        protected virtual void OnClientDisconnected(ulong clientID)
        {
            ClientData iclient = GetClient(clientID);
            clientList.Remove(clientID);
            ReceiveDisconnectPlayer(iclient);
        }

        protected virtual void ReceiveConnectPlayer(ulong clientID, FastBufferReader reader)
        {
            ClientData iclient = GetClient(clientID);
            reader.ReadNetworkSerializable(out MsgPlayerConnect msg);

            if (iclient != null && msg != null)
            {
                if (string.IsNullOrWhiteSpace(msg.username))
                    return;

                if (string.IsNullOrWhiteSpace(msg.gameUID))
                    return;
                
                Debug.Log("Client " + clientID + " connecting to game: " + msg.gameUID);
                
                //Connect to game as player or observer
                if (msg.observer)
                    ConnectObserverToGame(iclient, msg.userId, msg.username, msg.gameUID);
                else
                    ConnectPlayerToGame(iclient, msg.userId, msg.username, msg.gameUID, msg.nbPlayers);
                
                Gameserver gserver = GetGame(msg.gameUID);
                if(gserver != null)
                    gserver.RefreshAll();

            }
            
        }

        protected virtual void ReceiveDisconnectPlayer(ClientData iclient)
        {
            if (iclient == null)
                return;
            
            Gameserver gserver = GetGame(iclient.gameUID);
            if (gserver != null)
            {
                gserver.RemoveClient(iclient);
            }
        }
        
        protected virtual void ReceiveGameAction(ulong clientID, FastBufferReader reader)
        {
            ClientData client = GetClient(clientID);
            if (client != null)
            {
                Gameserver gserver = GetGame(client.gameUID);
                if (gserver != null && gserver.IsConnectedPlayer(client.userID))
                    gserver.ReceiveAction(clientID, reader);
            }
        }
        
        //Player wants to connect to gameUID
        protected virtual void ConnectPlayerToGame(ClientData client, string userID, string username, string gameUID,
            int nbPlayers)
        {
            //Create game
            Gameserver gserver = GetGame(gameUID);
            if (gserver == null)
                gserver = CreateGame(gameUID, nbPlayers);
            
            bool canConnect = gserver.IsPlayer(userID) || gserver.CountPlayers() < gserver.nbPlayers;
            if (gserver != null && canConnect)
            {
                //Add player to game
                client.gameUID = gameUID;
                client.userID = userID;
                client.username = username;
                gserver.AddClient(client);
                
                int playerID = gserver.AddPlayer(client);
                
                //Send back result
                MsgAfterConnected msgData = new MsgAfterConnected();
                msgData.success = true;
                msgData.playerId = playerID;
                msgData.gameData = gserver.GetGameData();
                SendToClient(client.clientID, GameAction.Connected, msgData, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }
        
        //Player wants to connect to gameUID as observer
        protected virtual void ConnectObserverToGame(ClientData client, string userID, string username,
            string gameUID)
        {
            Gameserver gserver = GetGame(gameUID);
            if (gserver != null && client != null)
            {
                //Set client data
                client.gameUID = gameUID;
                client.userID = userID;
                client.username = username;
                gserver.AddClient(client);

                //Return request
                MsgAfterConnected msg_data = new MsgAfterConnected();
                msg_data.success = true;
                msg_data.playerId = -1;
                msg_data.gameData = gserver.GetGameData();
                SendToClient(client.clientID, GameAction.Connected, msg_data, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        public void SendToClient(ulong clientID, ushort tag, INetworkSerializable data, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(tag);
            writer.WriteNetworkSerializable(data);
            Messaging.Send("refresh", clientID, writer, delivery);
            writer.Dispose();
        }

        public void SendMsgToClient(ushort clientID, string msg)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(GameAction.ServerMessage);
            writer.WriteValueSafe(msg);
            Messaging.Send("refresh", clientID, writer, NetworkDelivery.Reliable);
            writer.Dispose();
        }
        
        public Gameserver CreateGame(string uid, int nbPlayers)
        {
            Gameserver game = new Gameserver(uid, nbPlayers, true);
            gameList[game.gameUID] = game;
            return game;
        }
        
        public void RemoveGame(string gameID)
        {
            gameList.Remove(gameID);
        }
        
        public Gameserver GetGame(string gameUID)
        {
            if (string.IsNullOrEmpty(gameUID))
                return null;
            if (gameList.ContainsKey(gameUID))
                return gameList[gameUID];
            return null;
        }
        
        public ClientData GetClient(ulong clientID)
        {
            if (clientList.ContainsKey(clientID))
                return clientList[clientID];
            return null;
        }
        
        public ClientData GetClientByUser(string username)
        {
            foreach (KeyValuePair<ulong, ClientData> pair in clientList)
            {
                if (pair.Value.username == username)
                    return pair.Value;
            }
            return null;
        }
        
        



        public ulong ServerID => TcgNetwork.Get().ServerID;
        public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;
    }
    
    public class ClientData
    {
        public ulong clientID; //index of the connection
        public string userID; //Player userID, in auth system
        public string username; //Player username
        public string gameUID; //Unique id for the game

        public ClientData(ulong id) { clientID = id; }
    }

    public class CommandEvent
    {
        public ushort tag;
        public UnityAction<ClientData, SerializedData> callback;
    }
}