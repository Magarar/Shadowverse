using Unit;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Network
{
    /// <summary>
    /// 只是UnityTransport的包装器，如果计划为WebGL构建，可以更容易地用WebSocketTransport替换
    /// </summary>
    public class TcgTransport : MonoBehaviour
    {
        //用于 SSL/TLS 加密连接配置
        // [Header("Client")] 
        // [TextArea] public string chain;
        //
        // [Header("Server")] 
        // [TextArea] public string cert;
        // [TextArea] public string key; //Set this on server only

        private UnityTransport transport;
        
        private const string listenAddress = "0.0.0.0";

        public virtual void Init()
        {
            transport = GetComponent<UnityTransport>();
        }

        public virtual void SetServer(ushort port)
        {
            transport.ConnectionData.ServerListenAddress = listenAddress;
            transport.SetConnectionData(listenAddress, port);
            //transport.SetServerSecrets(cert, key);
            
        }

        public virtual void SetClient(string address, ushort port)
        {
            string ip = NetworkTool.HostToIP(address);
            transport.SetConnectionData(ip, port);
            //transport.SetClientSecrets(address, chain);
        }
        
        public virtual string GetAddress() { return transport.ConnectionData.Address; }
        public virtual ushort GetPort() { return transport.ConnectionData.Port; }
    }
}
