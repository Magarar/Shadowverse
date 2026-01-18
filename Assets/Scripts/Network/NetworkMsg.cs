using GameLogic;
using Unit;
using Unity.Netcode;
using UnityEngine.Serialization;

namespace Network
{
    public class MsgPlayerConnect : INetworkSerializable
    {
        public string userId;
        public string username;
        public string gameUID;
        public int nbPlayers;
        public bool observer; //join as observer

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref userId);
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref gameUID);
            serializer.SerializeValue(ref nbPlayers);
            serializer.SerializeValue(ref observer);
        }
    }
    
    public class MsgAfterConnected : INetworkSerializable
    {
        public bool success;
        public int playerId;
        public Game gameData;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref success);
            serializer.SerializeValue(ref playerId);

            if (serializer.IsReader)
            {
                int size = 0;
                serializer.SerializeValue(ref size);
                if (size > 0)
                {
                    byte[] bytes = new byte[size];
                    serializer.SerializeValue(ref bytes);
                    gameData = NetworkTool.Deserialize<Game>(bytes);
                }
            }

            if (serializer.IsWriter)
            {
                byte[] bytes = NetworkTool.Serialize(gameData);
                int size = bytes.Length;
                serializer.SerializeValue(ref size);
                if(size > 0)
                    serializer.SerializeValue(ref bytes);
            }
        }
    }
    
    //-------- Matchmaking --------
    public class MsgMatchmaking : INetworkSerializable
    {
        public string userId;
        public string username;
        public string group;
        public int players;
        public int elo;
        public bool refresh;
        public float time;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref userId);
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref group);
            serializer.SerializeValue(ref players);
            serializer.SerializeValue(ref elo);
            serializer.SerializeValue(ref refresh);
            serializer.SerializeValue(ref time);
        }
    }
    
    public class MatchmakingResult : INetworkSerializable
    {
        public bool success;
        public int players;
        public string group;
        public string serverURL;
        public string gameUID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref success);
            serializer.SerializeValue(ref players);
            serializer.SerializeValue(ref group);
            serializer.SerializeValue(ref serverURL);
            serializer.SerializeValue(ref gameUID);
        }
    }
    
    public class MsgMatchmakingList : INetworkSerializable
    {
        public string username;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref username);
        }
    }
    
    [System.Serializable]
    public struct MatchmakingListItem : INetworkSerializable
    {
        public string group;
        public string userID;
        public string username;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref group);
            serializer.SerializeValue(ref userID);
            serializer.SerializeValue(ref username);
        }
    }
    
    public class MatchmakingList : INetworkSerializable
    {
        public MatchmakingListItem[] items;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            NetworkTool.NetSerializeArray(serializer, ref items);
        }
    }
    
    [System.Serializable]
    public class MatchListItem : INetworkSerializable
    {
        public string group;
        public string username;
        public string gameUID;
        public string gameURL;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref group);
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref gameUID);
            serializer.SerializeValue(ref gameURL);
        }
    }
    
    public class MatchList : INetworkSerializable
    {
        public MatchListItem[] items;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            NetworkTool.NetSerializeArray(serializer, ref items);
        }
    }
    
    //-------- In Game --------
    public class MsgPlayCard : INetworkSerializable
    {
        public string cardUID;
        public Slot slot;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref cardUID);
            serializer.SerializeNetworkSerializable(ref slot);
        }
    }
    
    public class MsgCard : INetworkSerializable
    {
        public string cardUID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref cardUID);
        }
    }
    
    public class MsgEvolveCard : INetworkSerializable
    {
        public string cardUID;
        public bool isPlayer;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref cardUID);
            serializer.SerializeValue(ref isPlayer);
        }
    }
    
    public class MsgCardValue : INetworkSerializable
    {
        public string cardUID;
        public int value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref cardUID);
            serializer.SerializeValue(ref value);
        }
    }
    
    public class MsgPlayer : INetworkSerializable
    {
        public int playerID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerID);
        }
    }
    
    public class MsgPlayerValue : INetworkSerializable
    {
        public int playerID;
        public int value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerID);
            serializer.SerializeValue(ref value);
        }
    }
    
    public class MsgAttack : INetworkSerializable
    {
        public string attackerUID;
        public string targetUID;
        public int damage;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref attackerUID);
            serializer.SerializeValue(ref targetUID);
            serializer.SerializeValue(ref damage);
        }
    }
    
    public class MsgAttackPlayer : INetworkSerializable
    {
        public string attackerUID;
        public int targetID;
        public int damage;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref attackerUID);
            serializer.SerializeValue(ref targetID);
            serializer.SerializeValue(ref damage);
        }
    }
    
    public class MsgCastAbility : INetworkSerializable
    {
        public string abilityID;
        public string casterUID;
        public string targetUID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref abilityID);
            serializer.SerializeValue(ref casterUID);
            serializer.SerializeValue(ref targetUID);
        }
    }
    
    public class MsgCastAbilityPlayer : INetworkSerializable
    {
        public string abilityID;
        public string casterUID;
        public int targetID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref abilityID);
            serializer.SerializeValue(ref casterUID);
            serializer.SerializeValue(ref targetID);
        }
    }
    
    public class MsgCastAbilitySlot : INetworkSerializable
    {
        public string abilityID;
        public string casterUID;
        public Slot slot;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref abilityID);
            serializer.SerializeValue(ref casterUID);
            serializer.SerializeNetworkSerializable(ref slot);
        }
    }
    
    public class MsgSecret : INetworkSerializable
    {
        public string secretUID;
        public string triggerUID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref secretUID);
            serializer.SerializeValue(ref triggerUID);
        }
    }
    
    public class MsgMulligan : INetworkSerializable
    {
        public string[] cards;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            NetworkTool.NetSerializeArray(serializer, ref cards);
        }
    }
    
    public class MsgInt : INetworkSerializable
    {
        public int value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref value);
        }
    }
    
    public class MsgChat : INetworkSerializable
    {
        public int player_id;
        public string msg;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref player_id);
            serializer.SerializeValue(ref msg);
        }
    }
    
    public class MsgRefreshAll : INetworkSerializable
    {
        public Game gameData;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                int size = 0;
                serializer.SerializeValue(ref size);
                if (size > 0)
                {
                    byte[] bytes = new byte[size];
                    serializer.SerializeValue(ref bytes);
                    gameData = NetworkTool.Deserialize<Game>(bytes);
                }
            }
            
            if (serializer.IsWriter)
            {
                byte[] bytes = NetworkTool.Serialize(gameData);
                int size = bytes.Length;
                serializer.SerializeValue(ref size);
                if (size > 0)
                    serializer.SerializeValue(ref bytes);
            }
        }
    }
    
    public class NetworkMsg
    {
        
    }
}