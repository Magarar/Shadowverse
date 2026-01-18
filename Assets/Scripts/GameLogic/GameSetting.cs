using System;
using Data;
using Network;
using Unity.Netcode;

namespace GameLogic
{
    ///<总结>
    ///保持所有客户端的游戏设置，如游戏模式、游戏uid和要加载的场景
    ///将在比赛开始时发送到服务器
    ///</摘要>
    ///
    [Serializable]
    public class GameSetting:INetworkSerializable
    {
        public string serverUrl;
        public string gameUid;
        public string scene;
        public int nbPlayers;
        
        public GameType gameType = GameType.Solo;
        public GameMode gameMode = GameMode.Casual;
        public string level;

        public virtual bool IsHost()
        {
            return gameType is GameType.Solo or GameType.Adventure or GameType.HostP2P;
        }

        public virtual bool IsOffline()
        {
            return gameType is GameType.Solo or GameType.Adventure;
        }

        public virtual bool IsOnline()
        {
            return gameType is GameType.Multiplayer or GameType.HostP2P or GameType.Observer;
        }

        public virtual bool IsOnlinePlayer()
        {
            return gameType is GameType.Multiplayer or GameType.HostP2P;
        }

        public virtual bool IsRanked()
        {
            return gameMode==GameMode.Ranked;
        }

        public virtual string GetUrl()
        {
            if(!string.IsNullOrEmpty(serverUrl))
                return serverUrl;
            return NetworkData.Get().url;
        }
        
        public virtual string GetScene()
        {
            if (!string.IsNullOrEmpty(scene))
                return scene;
            return GamePlayData.Get().GetRandomArena();
        }

        public virtual string GetGameModeId()
        {
            if(gameMode==GameMode.Ranked)
                return "ranked";
            if(gameMode==GameMode.Casual)
                return "casual";
            return "";
        }

        public virtual LevelData GetLevel()
        {
            if(gameType==GameType.Adventure)
                return LevelData.Get(level);
            return null;
        }

        public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref serverUrl);
            serializer.SerializeValue(ref gameUid);
            serializer.SerializeValue(ref scene);
            serializer.SerializeValue(ref gameType);
            serializer.SerializeValue(ref gameMode);
            serializer.SerializeValue(ref level);
            serializer.SerializeValue(ref nbPlayers);
        }

        public static string GetRankModeString(GameMode mode)
        {
            if(mode==GameMode.Ranked)
                return "ranked";
            if(mode==GameMode.Casual)
                return "casual";
            return "";
        }
        
        public static GameMode GetRankMode(string rankID)
        {
            if (rankID == "ranked")
                return GameMode.Ranked;
            if (rankID == "casual")
                return GameMode.Casual;
            return GameMode.Casual;
        }

        public static GameSetting Default
        {
            get
            {
                GameSetting setting = new GameSetting
                {
                    serverUrl = "",
                    gameUid = "test",
                    gameType = GameType.Solo,
                    gameMode = GameMode.Casual,
                    nbPlayers = 2,
                    level = "",
                    scene = "Game"
                };
                return setting;
            }
        }

    }
    
    /// <summary>
    /// Hold all client's player settings, like avatar, cardback, and deck being used
    /// will be sent to server when a match start
    /// </summary>
    [Serializable]
    public class PlayerSettings:INetworkSerializable
    {
        public string username;
        public string avatar;
        public string cardback;
        public int aiLevel;
        public UserDeckData deck = UserDeckData.Default;
        
        public bool HasDeck()
        {
            return deck != null && !string.IsNullOrEmpty(deck.tid);
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref avatar);
            serializer.SerializeValue(ref cardback);
            serializer.SerializeValue(ref aiLevel);
            serializer.SerializeValue(ref deck);
        }
        
        public static PlayerSettings Default
        {
            get
            {
                PlayerSettings settings = new PlayerSettings
                {
                    username = "Player",
                    avatar = "",
                    cardback = "",
                    deck = UserDeckData.Default,
                    aiLevel = 1
                };
                return settings;
            }
        }
        
        public static PlayerSettings DefaultAI
        {
            get
            {
                PlayerSettings settings = new PlayerSettings
                {
                    username = "AI",
                    avatar = "",
                    cardback = "",
                    deck = UserDeckData.Default,
                    aiLevel = 10
                };
                return settings;
            }
        }
    }

    [Serializable]
    public enum GameType
    {
        Solo = 0,
        Adventure = 10,
        Multiplayer = 20,
        HostP2P = 30,
        Observer = 40,
    }

    [Serializable]
    public enum GameMode
    {
        Casual = 0,
        Ranked = 10,
    }
}