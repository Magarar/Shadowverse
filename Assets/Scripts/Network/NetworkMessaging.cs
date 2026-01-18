using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace Network
{
    ///<总结>
    ///发送和接收网络消息的基类
    ///</摘要>
    public class NetworkMessaging
    {
        private TcgNetwork network;
        
        private Dictionary<string,Action<ulong,FastBufferReader>> msgDict = new Dictionary<string,Action<ulong,FastBufferReader>>();
        
        public NetworkMessaging(TcgNetwork tcgNetwork)
        {
            network = tcgNetwork;
            network.onConnect += OnConnect;
        }

        private void OnConnect()
        {
            foreach (var pair in msgDict)
            {
                RegisterNetMsg(pair.Key, pair.Value);
            }
        }

        public void ListenMsg(string type, Action<ulong, FastBufferReader> callback)
        {
            msgDict[type] = callback;
            RegisterNetMsg(type, callback);
        }

        public void UnListenMsg(string type)
        {
            msgDict.Remove(type);
            if (network.NetworkManager.CustomMessagingManager != null)
            {
                network.NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler(type);
            }
        }

        private void RegisterNetMsg(string type, Action<ulong, FastBufferReader> callback)
        {
            if (IsOnline)
            {
                network.NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(type,
                    (ulong clientID, FastBufferReader reader) =>
                    {
                        ReceiveNetMessage(type, clientID, reader);
                    });
            }
        }

        private void ReceiveNetMessage(string type, ulong clientID, FastBufferReader reader)
        {
            bool isValid = msgDict.TryGetValue(type, out var callback);
            if (isValid && IsOnline)
            {
                callback(clientID, reader);
            }
        }
        
        //Send Signal
        
        public void SendEmpty(string type, ulong target, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(0, Allocator.Temp);
            Send(type, target, writer, delivery);
            writer.Dispose();
        }

        public void SendBytes(string type, ulong target, byte[] msg, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(msg.Length, Allocator.Temp);
            writer.WriteBytesSafe(msg, msg.Length);
            Send(type, target, writer, delivery);
            writer.Dispose();
        }
        
        public void SendString(string type, ulong target, string msg, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(msg.Length, Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(msg);
            Send(type, target, writer, delivery);
            writer.Dispose();
        }
        
        public void SendInt(string type, ulong target, int data, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(4, Allocator.Temp);
            writer.WriteValueSafe(data);
            Send(type, target, writer, delivery);
            writer.Dispose();
        }

        public void SendUInt64(string type, ulong target, ulong data, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(8, Allocator.Temp);
            writer.WriteValueSafe(data);
            Send(type, target, writer, delivery);
            writer.Dispose();
        }

        public void SendFloat(string type, ulong target, float data, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(4, Allocator.Temp);
            writer.WriteValueSafe(data);
            Send(type, target, writer, delivery);
            writer.Dispose();
        }
        
        public void SendObject<T>(string type, ulong target, T data, NetworkDelivery delivery) where T : INetworkSerializable
        {
            FastBufferWriter writer = new FastBufferWriter(256, Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteNetworkSerializable(data);
            Send(type, target, writer, delivery);
            writer.Dispose();
        }
        
        //--------- Send Multi ----------

        public void SendEmpty(string type, IReadOnlyList<ulong> targets, NetworkDelivery delivery)
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(0, Allocator.Temp);
                Send(type, targets, writer, delivery);
                writer.Dispose();
            }
        }

        public void SendBytes(string type, IReadOnlyList<ulong> targets, byte[] msg, NetworkDelivery delivery)
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(msg.Length, Allocator.Temp);
                writer.WriteBytesSafe(msg, msg.Length);
                Send(type, targets, writer, delivery);
                writer.Dispose();
            }
        }

        public void SendString(string type, IReadOnlyList<ulong> targets, string msg, NetworkDelivery delivery)
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(msg.Length, Allocator.Temp, TcgNetwork.MsgSizeMax);
                writer.WriteValueSafe(msg);
                Send(type, targets, writer, delivery);
                writer.Dispose();
            }
        }

        public void SendInt(string type, IReadOnlyList<ulong> targets, int data, NetworkDelivery delivery)
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(4, Allocator.Temp);
                writer.WriteValueSafe(data);
                Send(type, targets, writer, delivery);
                writer.Dispose();
            }
        }

        public void SendUInt64(string type, IReadOnlyList<ulong> targets, ulong data, NetworkDelivery delivery)
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(8, Allocator.Temp);
                writer.WriteValueSafe(data);
                Send(type, targets, writer, delivery);
                writer.Dispose();
            }
        }

        public void SendFloat(string type, IReadOnlyList<ulong> targets, float data, NetworkDelivery delivery)
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(4, Allocator.Temp);
                writer.WriteValueSafe(data);
                Send(type, targets, writer, delivery);
                writer.Dispose();
            }
        }
        
        public void SendObject<T>(string type, IReadOnlyList<ulong> targets, T data, NetworkDelivery delivery) where T : INetworkSerializable
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(256, Allocator.Temp, TcgNetwork.MsgSizeMax);
                writer.WriteNetworkSerializable(data);
                Send(type, targets, writer, delivery);
                writer.Dispose();
            }
        }
        
        //--------- Send All ----------

        public void SendEmptyAll(string type, NetworkDelivery delivery)
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(0, Allocator.Temp);
                SendAll(type, writer, delivery);
                writer.Dispose();
            }
        }

        public void SendStringAll(string type, string msg, NetworkDelivery delivery)
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(msg.Length, Allocator.Temp, TcgNetwork.MsgSizeMax);
                writer.WriteValueSafe(msg);
                SendAll(type, writer, delivery);
                writer.Dispose();
            }
        }

        public void SendIntAll(string type, int data, NetworkDelivery delivery)
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(4, Allocator.Temp);
                writer.WriteValueSafe(data);
                SendAll(type, writer, delivery);
                writer.Dispose();
            }
        }

        public void SendUInt64All(string type, ulong data, NetworkDelivery delivery)
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(8, Allocator.Temp);
                writer.WriteValueSafe(data);
                SendAll(type, writer, delivery);
                writer.Dispose();
            }
        }

        public void SendFloatAll(string type, float data, NetworkDelivery delivery)
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(4, Allocator.Temp);
                writer.WriteValueSafe(data);
                SendAll(type, writer, delivery);
                writer.Dispose();
            }
        }

        public void SendBytesAll(string type, byte[] msg, NetworkDelivery delivery)
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(msg.Length, Allocator.Temp);
                writer.WriteBytesSafe(msg, msg.Length);
                SendAll(type, writer, delivery);
                writer.Dispose();
            }
        }

        public void SendObjectAll<T>(string type, T data, NetworkDelivery delivery) where T : INetworkSerializable
        {
            if (IsServer)
            {
                FastBufferWriter writer = new FastBufferWriter(256, Allocator.Temp, TcgNetwork.MsgSizeMax);
                writer.WriteNetworkSerializable(data);
                SendAll(type, writer, delivery);
                writer.Dispose();
            }
        }
        
        
        //-------- Generic Send ----------
        public void Send(string type, ulong target, FastBufferWriter writer, NetworkDelivery delivery)
        {
            if (IsOnline)
                SendOnline(type, target, writer, delivery);
            else if(target == ClientID)
                SendOffline(type, writer);
        }
        
        public void Send(string type, IReadOnlyList<ulong> targets, FastBufferWriter writer, NetworkDelivery delivery)
        {
            if (IsOnline)
                SendOnline(type, targets, writer, delivery);
            else if (Contains(targets, ClientID))
                SendOffline(type, writer);
        }

        public void SendAll(string type, FastBufferWriter writer, NetworkDelivery delivery)
        {
            Send(type, ClientList, writer, delivery);
        }
        
        private void SendOnline(string type, ulong target, FastBufferWriter writer, NetworkDelivery delivery)
        {
            network.NetworkManager.CustomMessagingManager.SendNamedMessage(type, target, writer, delivery);
        }

        private void SendOnline(string type, IReadOnlyList<ulong> targets, FastBufferWriter writer, NetworkDelivery delivery)
        {
            network.NetworkManager.CustomMessagingManager.SendNamedMessage(type, targets, writer, delivery);
        }

        //只需将消息从编写者复制到reader，并立即调用回调
        private void SendOffline(string type, FastBufferWriter writer)
        {
            bool found = msgDict.TryGetValue(type, out System.Action<ulong, FastBufferReader> callback);
            if (found)
            {
                FastBufferReader reader = new FastBufferReader(writer, Allocator.Temp);
                callback?.Invoke(ClientID, reader);
                reader.Dispose();
            }
        }
        
        //--------- Forward msgs ----------
        
        //将客户端消息转发给一个客户端
        //在转发之前，请确保reader读完
        public void Forward(string type, ulong target, FastBufferReader reader, NetworkDelivery delivery)
        {
            if (IsServer && IsOnline)
            {
                reader.Seek(0); //Reset reader
                reader.ReadValueSafe(out ulong header); //Ignore header
                byte[] bytes = new byte[reader.Length - reader.Position];
                reader.ReadBytesSafe(ref bytes, reader.Length - reader.Position);
                FastBufferWriter writer = new FastBufferWriter(bytes.Length, Allocator.Temp);
                writer.WriteBytesSafe(bytes, bytes.Length);
                network.NetworkManager.CustomMessagingManager.SendNamedMessage(type, target, writer, delivery);
                writer.Dispose();
            }
        }
        
        //Forward a client message to all target clients
        //Make sure you finished reading the reader before forwarding
        public void Forward(string type, IReadOnlyList<ulong> targets, FastBufferReader reader, NetworkDelivery delivery)
        {
            if (IsServer && IsOnline)
            {
                reader.Seek(0); //Reset reader
                reader.ReadValueSafe(out ulong header); //Ignore header
                byte[] bytes = new byte[reader.Length - reader.Position];
                reader.ReadBytesSafe(ref bytes, reader.Length - reader.Position);
                FastBufferWriter writer = new FastBufferWriter(bytes.Length, Allocator.Temp);
                writer.WriteBytesSafe(bytes, bytes.Length);
                network.NetworkManager.CustomMessagingManager.SendNamedMessage(type, targets, writer, delivery);
                writer.Dispose();
            }
        }
        
        //将客户端消息转发给所有其他客户端（源除外）
        //在转发之前，请确保您已阅读完阅读器
        public void ForwardAll(string type, ulong sourceClient, FastBufferReader reader, NetworkDelivery delivery)
        {
            if (IsServer && IsOnline)
            {
                reader.Seek(0); //Reset reader
                reader.ReadValueSafe(out ulong header); //Ignore header
                byte[] bytes = new byte[reader.Length - reader.Position];
                reader.ReadBytesSafe(ref bytes, reader.Length - reader.Position);
                FastBufferWriter writer = new FastBufferWriter(bytes.Length, Allocator.Temp);
                writer.WriteBytesSafe(bytes, bytes.Length);

                foreach (ulong client in ClientList)
                {
                    if(client != sourceClient && client != ClientID)
                        network.NetworkManager.CustomMessagingManager.SendNamedMessage(type, client, writer, delivery);
                }
                writer.Dispose();
            }
        }

        private bool Contains(IReadOnlyList<ulong> list, ulong client)
        {
            foreach (var cid in list)
            {
                if(cid == client)
                    return true;
            }
            return false;
        }
        
        public IReadOnlyList<ulong> ClientList => network.GetClientsIds();
        
        public bool IsOnline => network.IsOnline;
        public bool IsServer => network.IsServer;
        public ulong ServerID => network.ServerID;
        public ulong ClientID => network.ClientID;


        public static NetworkMessaging Get()
        {
            return TcgNetwork.Get().Messaging;
        }
    }
}