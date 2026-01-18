using System;
using Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace GameClient
{
    /// <summary>
    /// Main client script for the matchmaker
    /// Will send requests to server and receive a response when a matchmaking succeed or fail
    /// </summary>
    public class GameClientMatchmaker:MonoBehaviour
    {
        public UnityAction<MatchmakingResult> onMatchmaking;
        public UnityAction<MatchmakingList> onMatchmakingList;
        public UnityAction<MatchList> onMatchList;
        
        private bool matchMaking = false;
        private float timer = 0f;
        private float matchTimer = 0f;
        private string matchMakingGroup;
        private int matchMakingPlayers;
        private UnityAction<bool> connectCallback;
        
        private static GameClientMatchmaker instance;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            TcgNetwork.Get().onConnect += OnConnect;
            TcgNetwork.Get().onDisconnect += OnDisconnect;
            Messaging.ListenMsg("matchmaking", ReceiveMatchmaking);
            Messaging.ListenMsg("matchmaking_list", ReceiveMatchmakingList);
            Messaging.ListenMsg("match_list", ReceiveMatchList);
        }

        private void OnDestroy()
        {
            Disconnect(); //Disconnect when switching scene

            if (TcgNetwork.Get() != null)
            {
                TcgNetwork.Get().onConnect -= OnConnect;
                TcgNetwork.Get().onDisconnect -= OnDisconnect;
                Messaging.UnListenMsg("matchmaking");
                Messaging.UnListenMsg("matchmaking_list");
                Messaging.UnListenMsg("match_list");
            }

        }

        private void Update()
        {
            if (matchMaking)
            {
                timer += Time.deltaTime;
                matchTimer+= Time.deltaTime;
                
                //发送定期请求
                if (IsConnected() && timer > 2f)
                {
                    timer = 0f;
                    SendMatchRequest(true, matchMakingGroup, matchMakingPlayers);
                }
                
                //Disconnected, stop
                if (!IsConnected() && !IsConnecting() && timer > 5f)
                {
                    StopMatchmaking();
                }
            }
        }

        public void StartMatchmaking(string group, int nbPlayers)
        {
            if(matchMaking)
                StopMatchmaking();
            
            Debug.Log("Start MatchMaking");
            matchMakingGroup = group;
            matchMakingPlayers = nbPlayers;
            matchMaking = true;
            matchTimer = 0f;
            timer = 0f;
            
            Connect(NetworkData.Get().url, NetworkData.Get().port, (bool success) =>
            {
                if (success)
                {
                    SendMatchRequest(false, group, nbPlayers);
                }
                else
                {
                    StopMatchmaking();
                }
            });

        }

        public void StopMatchmaking()
        {
            if (matchMaking)
            {
                Debug.Log("Stop MatchMaking");
                onMatchmaking?.Invoke(null);
                matchMaking = false;
                matchMakingGroup = "";
                matchMakingPlayers = 0;
            }
        }

        public void RefreshMatchmakingList()
        {
            Connect(NetworkData.Get().url, NetworkData.Get().port, (bool success) =>
            {
                if(success)
                    SendMatchmakingListRequest();
            });
        }

        public void RefreshMatchList(string username)
        {
            Connect(NetworkData.Get().url, NetworkData.Get().port, (bool success) =>
            {
                if (success)
                    SendMatchListRequest(username);
            });
        }

        public void Connect(string url, ushort port, UnityAction<bool> callback = null)
        {
            //Must be logged in to API to connect
            if (!Authenticator.Get().IsSignedIn())
            {
                callback?.Invoke(false);
                return;
            }
            
            //Check if already connected
            if (IsConnected() || IsConnecting())
            {
                callback?.Invoke(IsConnected());
                return;
            }
            
            connectCallback = callback;
            TcgNetwork.Get().StartClient(url, port);
        }

        public void Disconnect()
        {
            TcgNetwork.Get()?.Disconnect();
        }

        private void OnConnect()
        {
            Debug.Log("Connected to server");
            connectCallback?.Invoke(true);
            connectCallback = null;
        }

        private void OnDisconnect()
        {
            Debug.Log("Disconnected from server");
            connectCallback?.Invoke(false);
            connectCallback = null;
            matchMaking = false;
        }

        private void SendMatchRequest(bool refresh, string group, int nbPlayers)
        {
            MsgMatchmaking msgMatchmaking = new MsgMatchmaking()
            {
                userId = Authenticator.Get().GetUserId(),
                username = Authenticator.Get().GetUsername(),
                group = group,
                players = nbPlayers,
                refresh = refresh,
                time = matchTimer,
                elo = Authenticator.Get().GetUserData().elo,
            };
            Messaging.SendObject("matchmaking", ServerID, msgMatchmaking, NetworkDelivery.Reliable);
        }

        private void SendMatchmakingListRequest()
        {
            MsgMatchmakingList msgMatch = new MsgMatchmakingList()
            {
                username = ""
            };//Return all users
            Messaging.SendObject("matchmaking_list", ServerID, msgMatch, NetworkDelivery.Reliable);

        }

        private void SendMatchListRequest(string username)
        {
            MsgMatchmakingList msgMatch = new MsgMatchmakingList()
            {
                username = username
            };
            Messaging.SendObject("match_list", ServerID, msgMatch, NetworkDelivery.Reliable);
        }

        private void ReceiveMatchmaking(ulong clientId, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MatchmakingResult msg);
            if (IsConnected() && matchMaking && matchMakingGroup == msg.group)
            {
                matchMaking = !msg.success; //Stop matchmaking if success
                onMatchmaking?.Invoke(msg);
            }
        }

        private void ReceiveMatchmakingList(ulong clientID, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MatchmakingList list);
            onMatchmakingList?.Invoke(list);
        }

        private void ReceiveMatchList(ulong clientID, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MatchList list);
            onMatchList?.Invoke(list);
        }
        
        public bool IsMatchmaking()
        {
            return matchMaking;
        }
        
        public string GetGroup()
        {
            return matchMakingGroup;
        }

        public int GetNbPlayers()
        {
            return matchMakingPlayers;
        }
        
        public float GetTimer()
        {
            return matchTimer;
        }
        
        private bool IsConnecting()
        {
            return TcgNetwork.Get().IsConnecting();
        }
        
        

        public bool IsConnected()
        {
            return TcgNetwork.Get().IsConnected();
        }
        
        

        public ulong ServerID => TcgNetwork.Get().ServerID;
        public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;

        public static GameClientMatchmaker Get()
        {
            return instance;
        }
    }
}