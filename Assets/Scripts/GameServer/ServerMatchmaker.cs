using System.Collections.Generic;
using System.Linq;
using Network;
using Unit;
using Unity.Netcode;
using UnityEngine;

namespace GameServer
{
    /// <summary>
    /// Main server script for the matchmaker
    /// will receive player request and then match players together and send them the game uid and game url to connect to
    /// </summary>
    public class ServerMatchmaker:MonoBehaviour
    {
        [Header("Matchmaker")]
        public string[] servers;
        
        private Dictionary<ulong, ClientData> clientList = new Dictionary<ulong, ClientData>();  //List of clients
        private Dictionary<string, MatchPlayerData> matchmakingPlayers = new Dictionary<string, MatchPlayerData>(); //Get deleted every 20 sec
        private Dictionary<string, MatchData> matchedPlayers = new Dictionary<string, MatchData>(); //userID -> match
        private List<MatchPlayerData> validUsers = new List<MatchPlayerData>(); //temporary array
        private float matchmakeTimer = 0f;
        
        private static ServerMatchmaker instance;
        
        protected virtual void Awake()
        {
            instance = this;
            Application.runInBackground = true;
        }
        
        protected virtual void Start()
        {
            TcgNetwork network = TcgNetwork.Get();
            network.onClientJoin += OnClientConnected;
            network.onClientQuit += OnClientDisconnected;

            Messaging.ListenMsg("matchmaking", ReceiveMatchmaking);
            Messaging.ListenMsg("matchmaking_list", ReceiveMatchmakingList);
            Messaging.ListenMsg("match_list", ReceiveMatchList);

            if (!network.IsActive())
            {
                network.StartServer(NetworkData.Get().port);
            }
        }
        
        protected virtual void Update()
        {
            //Matchmaking
            matchmakeTimer += Time.deltaTime;
            if (matchmakeTimer > 20f)
            {
                matchmakeTimer = 0f;
                matchmakingPlayers.Clear(); //Delete and restart, to make sure you only keep recent players
            }
        }
        
        protected virtual void OnClientConnected(ulong clientID)
        {
            ClientData iclient = new ClientData(clientID);
            clientList[clientID] = iclient;
        }
        
        protected virtual void OnClientDisconnected(ulong clientID)
        {
            if (clientList.ContainsKey(clientID))
            {
                ClientData iclient = clientList[clientID];
                if(iclient.username != null)
                    matchmakingPlayers.Remove(iclient.userID);
                clientList.Remove(clientID);
            }
        }

        protected virtual void ReceiveMatchmaking(ulong clientID, FastBufferReader reader)
        {
            ClientData iclient = GetClient(clientID);
            reader.ReadNetworkSerializable(out MsgMatchmaking msg);
            
            if (iclient == null || string.IsNullOrWhiteSpace(msg.userId) || string.IsNullOrWhiteSpace(msg.username))
                return;
            
            string userID = msg.userId;
            bool isRefresh = msg.refresh;

            iclient.userID = msg.userId;
            iclient.username = msg.username;

            if (!isRefresh)
                matchmakingPlayers.Remove(userID);
            
            //Check if already matched
            if (matchedPlayers.ContainsKey(userID))
            {
                MatchData match = matchedPlayers[userID];
                if (!match.ended)
                {
                    SendMatchmakingResponse(iclient, match, msg.group, match.players.Length); //Was already matched, return saved result!
                    return;
                }
            }
            
            MatchPlayerData pdata =  new MatchPlayerData()
            {
                userID = userID,
                username = msg.username,
                group = msg.group,
                eloRank = msg.elo,
                nbPlayers = msg.players
            };
            
            //Add to matchking players
            if (!matchmakingPlayers.ContainsKey(userID))
                matchmakingPlayers.Add(userID, pdata);
            
            //Start searching for other valid players
            float waitMax = 20f;
            int varianceMax = 2000;
            
            bool friendly = msg.group.StartsWith("u_");
            float waitTimer = msg.time;
            float waitValue = Mathf.Clamp01(waitTimer / waitMax);
            int eloVariance = Mathf.RoundToInt(waitValue * varianceMax);
            
            validUsers.Clear();
            validUsers.Add(pdata); //Add self

            foreach (KeyValuePair<string, MatchPlayerData> opair in matchmakingPlayers)
            {
                string auserID = opair.Key;
                MatchPlayerData adata = opair.Value;
                int diff = Mathf.Abs(adata.eloRank - msg.elo);
                bool sameGroup = adata.group == msg.group;
                bool samePlayers = adata.nbPlayers == msg.players;
                bool validElo = friendly || diff < eloVariance;
                if (auserID != userID && validElo && sameGroup && samePlayers)
                {
                    validUsers.Add(adata);
                }
            }
            
            //Not enough players found, send current count
            if (validUsers.Count < msg.players)
            {
                SendMatchmakingResponse(iclient, null, msg.group, validUsers.Count);
                return; //Not enough valid users
            }
            
            //Match success, send result
            string prefix = msg.group.Length >= 2 ? msg.group.Substring(0, 2) : "";
            string gameCode = prefix + GameTool.GenerateRandomID(12, 15);
            string gameURL = ""; //Empty url means it will use the default NetworkData url set on the client
            if (servers.Length > 0)
                gameURL = servers[Random.Range(0, servers.Length)];

            int pindex = 0;
            MatchData nmatch = new MatchData(msg.group, gameCode, gameURL, msg.players);
            foreach (MatchPlayerData vuser in validUsers)
            {
                if (pindex < nmatch.players.Length)
                {
                    matchmakingPlayers.Remove(vuser.userID);
                    matchedPlayers[vuser.userID] = nmatch;
                    nmatch.players[pindex] = vuser.username;
                    pindex++;
                }
            }

            //Send response to current request
            if (matchedPlayers.ContainsKey(userID))
            {
                SendMatchmakingResponse(iclient, nmatch, nmatch.group, nmatch.players.Length); //Just matched to new player!
            }
            
        }

        protected virtual void SendMatchmakingResponse(ClientData iclient, MatchData match, string group, int players)
        {
            MatchmakingResult msgMatch = new MatchmakingResult
            {
                success = match != null,
                players = players,
                group = group,
                gameUID = match != null ? match.gameUID : "",
                serverURL = match != null ? match.serverURL : ""
            };

            Messaging.SendObject("matchmaking", iclient.clientID, msgMatch, NetworkDelivery.Reliable);
        }

        protected virtual void ReceiveMatchmakingList(ulong clientID, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MsgMatchmakingList msg);

            List<MatchmakingListItem> items = new List<MatchmakingListItem>();

            foreach (KeyValuePair<string, MatchPlayerData> pair in matchmakingPlayers)
            {
                if (string.IsNullOrEmpty(msg.username) || pair.Key == msg.username)
                {
                    MatchPlayerData pdata = pair.Value;
                    MatchmakingListItem item = new MatchmakingListItem();
                    item.group = pdata.group;
                    item.userID = pdata.userID;
                    item.username = pdata.username;
                    items.Add(item);
                }
            }

            MatchmakingList msg_list = new MatchmakingList();
            msg_list.items = items.ToArray();
            Messaging.SendObject("matchmaking_list", clientID, msg_list, NetworkDelivery.Reliable);
        }
        
        protected virtual void ReceiveMatchList(ulong client_id, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MsgMatchmakingList msg);

            List<MatchListItem> items = new List<MatchListItem>();

            foreach (KeyValuePair<string, MatchData> pair in matchedPlayers)
            {
                if (!pair.Value.ended)
                {
                    if (string.IsNullOrEmpty(msg.username) || Contains(pair.Value.players, msg.username))
                    {
                        MatchData pdata = pair.Value;
                        MatchListItem item = new MatchListItem();
                        item.group = pair.Value.group;
                        item.username = msg.username;
                        item.gameUID = pdata.gameUID;
                        item.gameURL = pdata.serverURL;
                        items.Add(item);
                    }
                }
            }

            MatchList msg_list = new MatchList
            {
                items = items.ToArray()
            };

            Messaging.SendObject("match_list", client_id, msg_list, NetworkDelivery.Reliable);
        }
        
        private bool Contains(string[] users, string user)
        {
            return users.Any(auser => auser == user);
        }
        
        public void EndMatch(string uid)
        {
            foreach (KeyValuePair<string, MatchData> pair in matchedPlayers)
            {
                if (pair.Value.gameUID == uid)
                    pair.Value.ended = true;
            }
        }
        
        public ClientData GetClient(ulong client_id)
        {
            if (clientList.ContainsKey(client_id))
                return clientList[client_id];
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

        public static ServerMatchmaker Get()
        {
            return instance;
        }
    }
    
    public class MatchPlayerData
    {
        public string userID;
        public string username;
        public string group;
        public int eloRank;
        public int nbPlayers;
    }

    public class MatchData
    {
        public string group;
        public string gameUID;
        public string serverURL;
        public bool ended = false;
        public string[] players;

        public MatchData(string grp, string uid, string url, int players) { group = grp; gameUID = uid; serverURL = url; this.players = new string[players]; }
    }
}