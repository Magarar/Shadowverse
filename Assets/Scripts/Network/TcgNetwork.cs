using System;
using System.Collections.Generic;
using Unit;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Network
{
    /// <summary>
    /// 处理服务器和客户端之间网络连接的主脚本
    /// 需要位于DontDestroyOnLoad
    /// </summary>
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(NetworkManager))]
    [RequireComponent(typeof(TcgTransport))]
    public class TcgNetwork : MonoBehaviour
    {
        public NetworkData data;
        
        //Server & Client events
        public UnityAction onTick; //Every network tick
        public UnityAction onConnect;  //Event when self connect, happens before onReady, before sending any data
        public UnityAction onDisconnect; //Event when self disconnect
        
        //Server only events
        public UnityAction<ulong> onClientJoin;
        public UnityAction<ulong> onClientQuit;
        public UnityAction<ulong> onClientReady;
        
        //客户端连接时的额外批准验证
        public delegate bool ApprovalEvent(ulong client_id, ConnectionData connect_data);
        public event ApprovalEvent onApproval;
        
        //-----------------------------------------------------------------------------------------------------------------

        private NetworkManager network;
        private TcgTransport transport;
        private Authenticator auth;
        private ConnectionData connection;
        private NetworkMessaging messaging;
        
        [System.NonSerialized]
        private static bool inited = false;
        private static TcgNetwork instance;
        
        private const int MsgSize = 1024 * 1024;
        private bool offlineMode = false;
        private bool connected = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return; //Manager already exists, destroy this one
            }
            Init();
            DontDestroyOnLoad(gameObject);
        }
        
        private void Init()
        {
            if (!inited || transport == null)
            {
                instance = this;
                inited = true;
                network = GetComponent<NetworkManager>();
                transport = GetComponent<TcgTransport>();
                messaging = new NetworkMessaging(this);
                connection = new ConnectionData();
                transport.Init();
                
                network.ConnectionApprovalCallback += ApprovalCheck;
                network.OnClientConnectedCallback += OnClientConnect;
                network.OnClientDisconnectCallback += OnClientDisconnect;
                
                InitAuth();
            }
        }

        private void Update()
        {
            
        }
        
        //Start a host (client + server)
        public void StartHost(ushort port)
        {
            Debug.Log("Host Server Port " + port);
            transport.SetServer(port);
            connection.userId = auth.UserID;
            connection.userName = auth.Username;
            network.NetworkConfig.ConnectionData = NetworkTool.NetSerialize(connection);
            offlineMode = false;
            network.StartHost();
            AfterConnected();
            
        }
        
        //启动专用服务器
        public void StartServer(ushort port)
        {
            Debug.Log("Start Server Port " + port);
            transport.SetServer(port);
            connection.userId = "";
            connection.userName = "";
            network.NetworkConfig.ConnectionData = NetworkTool.NetSerialize(connection);
            offlineMode = false;
            network.StartServer();
            AfterConnected();
        }
        
        //如果is_host设置为true，则表示该玩家在专用服务器上创建了游戏
        //所以它仍然是一个客户端（不是服务器），而是选择游戏设置的人
        public void StartClient(string serverURL, ushort port)
        {
            Debug.Log("Join Server: " + serverURL + " " + port);
            transport.SetClient(serverURL, port);
            connection.userId = auth.UserID;
            connection.userName = auth.Username;
            network.NetworkConfig.ConnectionData = NetworkTool.NetSerialize(connection);
            offlineMode = false;
            network.StartClient();
        }
        
        //在关闭所有网络的情况下启动模拟主机
        public void StartHostOffline()
        {
            Debug.Log("Host Offline");
            Disconnect();
            offlineMode = true;
            AfterConnected();
        }

        public void Disconnect()
        {
            if(!IsClient&&!IsServer)
                return;
            Debug.Log("Disconnect");
            network.Shutdown();
            AfterDisconnected();
        }
        
        public void SetConnectionExtraData(byte[] bytes)
        {
            connection.extra = bytes;
        }

        public void SetConnectionExtraData(string data)
        {
            connection.extra = NetworkTool.SerializeString(data);
        }

        public void SetConnectionExtraData<T>(T data) where T : INetworkSerializable, new()
        {
            connection.extra = NetworkTool.NetSerialize(data);
        }
        
        private async void InitAuth()
        {
            auth = Authenticator.Create(data.authenticatorType);
            await auth.Initialize();
        }
        
        private void AfterConnected()
        {
            if(connected)
                return;
            if (network.NetworkTickSystem != null)
                network.NetworkTickSystem.Tick += OnTick;
            connected = true;
            onConnect?.Invoke();
        }
        
        private void AfterDisconnected()
        {
            if (!connected)
                return;

            if (network.NetworkTickSystem != null)
                network.NetworkTickSystem.Tick -= OnTick;
            offlineMode = false;
            connected = false;
            onDisconnect?.Invoke();
        }
        
        private void OnClientConnect(ulong clientId)
        {
            if (IsServer && clientId != ServerID)
            {
                Debug.Log("Client Connected: " + clientId);
                onClientJoin?.Invoke(clientId);
            }
            if (!IsServer)
                AfterConnected(); //AfterConnected wasn't called yet for client
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (IsServer && clientId != ServerID)
            {
                Debug.Log("Client Disconnected: " + clientId);
                onClientQuit?.Invoke(clientId);
            }
            if (ClientID == clientId || clientId == ServerID)
                AfterDisconnected();
        }

        private void OnTick() =>onTick?.Invoke();


        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest req, NetworkManager.ConnectionApprovalResponse res)
        {
            ConnectionData connect = NetworkTool.NetDeserialize<ConnectionData>(req.Payload);
            bool approved = ApproveClient(req.ClientNetworkId, connect);
            res.Approved = approved;
        }

        private bool ApproveClient(ulong clientId, ConnectionData connect)
        {
            if (clientId == ServerID)
                return true; //Server always approve itself

            if (offlineMode)
                return false;

            if (connect == null)
                return false; //Invalid data

            if (string.IsNullOrEmpty(connect.userName) || string.IsNullOrEmpty(connect.userId))
                return false; //Invalid username

            if (onApproval != null && !onApproval.Invoke(clientId, connect))
                return false; //自定义审批回调

            return true; //通过所有验证后批准客户端连接
        }

        public IReadOnlyList<ulong> GetClientsIds()
        {
            return network.ConnectedClientsIds;
        }

        public int CountClients()
        {
            if(offlineMode)
                return 1;
            if (IsServer && IsConnected())
                return network.ConnectedClientsIds.Count;
            return 0;
        }
        
        public bool IsConnecting()
        {
            return IsActive() && !IsConnected(); //Trying to connect but not yet
        }

        public bool IsConnected()
        {
            return offlineMode || network.IsServer || network.IsConnectedClient;
        }
        
        public bool IsActive()
        {
            return offlineMode || network.IsServer || network.IsClient;
        }
        
        public string Address => transport.GetAddress();
        public ushort Port => transport.GetPort();

        public ulong ClientID => offlineMode?ServerID : network.LocalClientId;//此客户端的ID（如果是主机，将与ServerID相同），每次重新连接时都会更改，由Netcode分配
        public ulong ServerID => NetworkManager.ServerClientId;//服务器ID

        public bool IsServer => offlineMode || network.IsServer;
        public bool IsClient => offlineMode || network.IsClient;
        public bool IsHost => IsServer && IsClient;
        public bool IsOnline => !offlineMode && IsActive();
        public NetworkTime LocalTime => network.LocalTime;
        public NetworkTime ServerTime => network.ServerTime;
        public float DeltaTick => 1f / network.NetworkTickSystem.TickRate;
        
        public NetworkManager NetworkManager => network;
        public TcgTransport Transport => transport;
        public NetworkMessaging Messaging => messaging;
        public Authenticator Auth => auth;


        public static int MsgSizeMax => MsgSize;
        
        public static TcgNetwork Get()
        {
            if (instance == null)
            {
                TcgNetwork net = FindObjectOfType<TcgNetwork>();
                net?.Init();
            }
            return instance;
        }
        
    }

    [Serializable]
    public class ConnectionData : INetworkSerializable
    {
        public string userId = "";
        public string userName = "";
        
        public byte[] extra = Array.Empty<byte>();
        //如果添加额外数据，请确保ConnectionData的总大小不超过Netcode最大未分段消息（1400字节）
        //连接数据不可能有碎片化的消息，因为连接是在单个请求中完成的

        public string GetExtraString()
        {
            return NetworkTool.DeserializeString(extra);
        }

        public T GetExtraData<T>() where T : INetworkSerializable, new()
        {
            return NetworkTool.NetDeserialize<T>(extra);
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref userId);
            serializer.SerializeValue(ref userName);
            serializer.SerializeValue(ref extra);
        }

        
    }
    
    public class SerializedData
    {
        private FastBufferReader reader;
        private INetworkSerializable data;
        private byte[] bytes;

        public SerializedData(FastBufferReader r)
        {
            reader = r;
            data = null;
        }

        public SerializedData(INetworkSerializable d)
        {
            data = d;
        }

        public string GetString()
        {
            reader.ReadValueSafe(out string msg);
            return msg;
        }

        public T Get<T>() where T : INetworkSerializable, new()
        {
            if (data != null)
            {
                return (T)data;
            }
            else if (bytes != null)
            {
                data = NetworkTool.NetDeserialize<T>(bytes);
                return (T)data;
            }
            else
            {
                reader.ReadNetworkSerializable(out T obj);
                data = obj;
                return obj;
            }
        }
        
        //PreRead不知道对象类型，因为FastBufferReader将被netcode处理
        public void PreRead()
        {
            int size = reader.Length - reader.Position;
            bytes = new byte[size];
            reader.ReadBytesSafe(ref bytes, size);
        }
    }
    
}
