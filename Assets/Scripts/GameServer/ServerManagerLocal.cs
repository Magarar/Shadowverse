using System.Collections.Generic;
using GameClient;
using GameLogic;
using Network;
using Unity.Netcode;
using UnityEngine;

namespace GameServer
{
    /// <summary>
    /// Local server running on the client to play in solo mode against AI
    /// Contains only one GameServer
    /// </summary>
    public class ServerManagerLocal : MonoBehaviour
    {
        private Gameserver server;
        
        private Dictionary<ulong, ClientData> clientList = new Dictionary<ulong, ClientData>();  //List of clients

        protected virtual void Start()
        {
            if (Gameclient.gameSetting.IsHost())
            {
                StartServer(); //Start local server if not playing online
            }
        }

        protected virtual void StartServer()
        {
            TcgNetwork network = TcgNetwork.Get();
            network.onClientJoin += OnClientJoin;
            network.onClientQuit += OnClientQuit;
            network.Messaging.ListenMsg("connect", ReceiveConnectPlayer);
            network.Messaging.ListenMsg("action", ReceiveGameAction);
            
            clientList[network.ServerID] = new ClientData(network.ServerID); //Add yourself
            server = new Gameserver(Gameclient.gameSetting.gameUid, Gameclient.gameSetting.nbPlayers, false);
        }

        protected virtual void OnDestroy()
        {
            TcgNetwork network = TcgNetwork.Get();
            if (network != null)
            {
                network.onClientJoin -= OnClientJoin;
                network.onClientQuit -= OnClientQuit;
                network.Messaging.UnListenMsg("connect");
                network.Messaging.UnListenMsg("action");
            }
        }

        protected virtual void OnClientJoin(ulong clientID)
        {
            clientList[clientID] = new ClientData(clientID);
        }

        protected virtual void OnClientQuit(ulong clientID)
        {
            ClientData client = GetClient(clientID);
            server?.RemoveClient(client);
            clientList.Remove(clientID);
        }
        
        protected virtual void Update()
        {
            server?.Update();
        }

        protected virtual void ReceiveConnectPlayer(ulong clientID, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MsgPlayerConnect msg);

            if ( msg != null)
            {
                if (string.IsNullOrWhiteSpace(msg.username))
                    return;

                if (string.IsNullOrWhiteSpace(msg.gameUID))
                    return;
                
                ClientData client = GetClient(clientID);
                if (client == null)
                    return;
                Debug.Log("Client " + clientID + " connecting to game: " + msg.gameUID);
                
                bool canConnect = server.IsPlayer(msg.userId) || server.CountPlayers() < server.nbPlayers;

                if (canConnect)
                {
                    client.userID = msg.userId;
                    client.username = msg.username;
                    client.gameUID = msg.gameUID;
                    server.AddClient(client);
                    
                    int playerID = server.AddPlayer(client);
                    //Send back result
                    MsgAfterConnected msgData = new MsgAfterConnected()
                    {
                        success = true,
                        playerId = playerID,
                        gameData = server.GetGameData()
                    };
                    SendToClient(clientID, GameAction.Connected, msgData, NetworkDelivery.ReliableFragmentedSequenced);

                }

            }

        }

        protected virtual void ReceiveGameAction(ulong clientID, FastBufferReader reader)
        {
            ClientData client = GetClient(clientID);
            if (client != null)
            {
                if (server.IsConnectedPlayer(client.userID))
                    server.ReceiveAction(clientID, reader);
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
        
        public ClientData GetClient(ulong clientID)
        {
            if (clientList.ContainsKey(clientID))
                return clientList[clientID];
            return null;
        }
        
        public ulong ServerID => TcgNetwork.Get().ServerID;
        public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;
        

        
    }
}