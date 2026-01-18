using System.Collections.Generic;
using System.Net.Sockets;
using Data;
using GameLogic;
using Network;
using Unit;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace GameClient
{
    /// <summary>
    /// Main script for the client-side of the game, should be in game scene only
    /// Will connect to server, then connect to the game on that server (with uid) and then will send game settings
    /// During the game, will send all actions performed by the player and receive game refreshes
    /// </summary>
    public class Gameclient:MonoBehaviour
    {
        //--- These settings are set in the menu scene and when the game start will be sent to server
        public static GameSetting gameSetting = GameSetting.Default;
        public static PlayerSettings playerSettings = PlayerSettings.Default;
        public static PlayerSettings aiSettings = PlayerSettings.DefaultAI;
        //Which user should it observe, null if not an obs
        public static string observerUser = null;
        
        public UnityAction onConnectServer;
        public UnityAction onConnectGame;
        public UnityAction<int> onPlayerReady;

        public UnityAction onGameStart;
        public UnityAction<int> onGameEnd;              //winner player_id
        public UnityAction<int> onNewTurn;              //current player_id

        public UnityAction<Card, Slot> onCardPlayed;
        public UnityAction<Card, Slot> onCardMoved;
        public UnityAction<Slot> onCardSummoned;
        public UnityAction<Card> onCardTransformed;
        public UnityAction<Card> onCardDiscarded;
        public UnityAction<int> onCardDraw;
        public UnityAction<int> onValueRolled;

        public UnityAction<AbilityData, Card> onAbilityStart;
        public UnityAction<AbilityData, Card, Card> onAbilityTargetCard;      //Ability, Caster, Target
        public UnityAction<AbilityData, Card, Player> onAbilityTargetPlayer;
        public UnityAction<AbilityData, Card, Slot> onAbilityTargetSlot;
        public UnityAction<AbilityData, Card> onAbilityEnd;
        public UnityAction<Card, Card> onSecretTrigger;    //Secret, Triggerer
        public UnityAction<Card, Card> onSecretResolve;    //Secret, Triggerer

        public UnityAction<Card, Card> onAttackStart;   //Attacker, Defender
        public UnityAction<Card, Card> onAttackEnd;     //Attacker, Defender
        public UnityAction<Card, Player> onAttackPlayerStart;
        public UnityAction<Card, Player> onAttackPlayerEnd;
        
        public UnityAction<Card,bool> onCardEvolved;
        public UnityAction<Card,bool> onCardSuperEvolved;
        
        public UnityAction<AbilityData, Card> onAbilityTrigger;
        
        public UnityAction<Card> onPlayerEvolveCard;
        public UnityAction<Card> onPlayerSuperEvolveCard;
        
        public UnityAction<Card> onCardAttacked;

        public UnityAction<Card, int> onCardDamaged;
        public UnityAction<Player, int> onPlayerDamaged;
        public UnityAction<Card, int> onCardHealed;
        public UnityAction<Player, int> onPlayerHealed;

        public UnityAction<Player, int> onAddExtraMana;

        public UnityAction<int, string> onChatMsg;  //player_id, msg
        public UnityAction< string> onServerMsg;  //msg
        public UnityAction onRefreshAll;

        private int playerId = 0;//Player playing on this device;
        private Game gameData;
        
        private bool observerMode = false;
        private int observerPlayerId = 0;
        private float timer = 0;

        private Dictionary<ushort, RefreshEvent> registeredCommands = new();
        private static Gameclient instance;

        private void Awake()
        {
            instance = this;
            Application.targetFrameRate = 120;
        }

        protected virtual void Start()
        {
            RegisterRefresh(GameAction.Connected, OnConnectedToGame);
            RegisterRefresh(GameAction.PlayerReady, OnPlayerReady);
            RegisterRefresh(GameAction.GameStart, OnGameStart);
            RegisterRefresh(GameAction.GameEnd, OnGameEnd);
            RegisterRefresh(GameAction.NewTurn, OnNewTurn);
            RegisterRefresh(GameAction.CardPlayed, OnCardPlayed);
            RegisterRefresh(GameAction.CardMoved, OnCardMoved);
            RegisterRefresh(GameAction.CardSummoned, OnCardSummoned);
            RegisterRefresh(GameAction.CardTransformed, OnCardTransformed);
            RegisterRefresh(GameAction.CardDiscarded, OnCardDiscarded);
            RegisterRefresh(GameAction.CardDrawn, OnCardDraw);
            RegisterRefresh(GameAction.ValueRolled, OnValueRolled);

            RegisterRefresh(GameAction.AttackStart, OnAttackStart);
            RegisterRefresh(GameAction.AttackEnd, OnAttackEnd);
            RegisterRefresh(GameAction.AttackPlayerStart, OnAttackPlayerStart);
            RegisterRefresh(GameAction.AttackPlayerEnd, OnAttackPlayerEnd);
            RegisterRefresh(GameAction.CardDamaged, OnCardDamaged);
            RegisterRefresh(GameAction.PlayerDamaged, OnPlayerDamaged);
            RegisterRefresh(GameAction.CardHealed, OnCardHealed);
            RegisterRefresh(GameAction.PlayerHealed, OnPlayerHealed);

            RegisterRefresh(GameAction.AbilityTrigger, OnAbilityTrigger);
            RegisterRefresh(GameAction.AbilityTargetCard, OnAbilityTargetCard);
            RegisterRefresh(GameAction.AbilityTargetPlayer, OnAbilityTargetPlayer);
            RegisterRefresh(GameAction.AbilityTargetSlot, OnAbilityTargetSlot);
            RegisterRefresh(GameAction.AbilityEnd, OnAbilityAfter);

            RegisterRefresh(GameAction.SecretTriggered, OnSecretTrigger);
            RegisterRefresh(GameAction.SecretResolved, OnSecretResolve);
            
            RegisterRefresh(GameAction.CardEvolved, OnCardEvolved);
            RegisterRefresh(GameAction.CardSuperEvolved, OnCardSuperEvolved);
            RegisterRefresh(GameAction.PlayerEvolveCard, OnPlayerEvolveCard);
            RegisterRefresh(GameAction.PlayerSuperEvolveCard, OnPlayerSuperEvolveCard);
            

            RegisterRefresh(GameAction.ChatMessage, OnChat);
            RegisterRefresh(GameAction.ServerMessage, OnServerMsg);
            RegisterRefresh(GameAction.RefreshAll, OnRefreshAll);

            TcgNetwork.Get().onConnect += OnConnectedServer;
            TcgNetwork.Get().Messaging.ListenMsg("refresh", OnReceiveRefresh);

            ConnectToAPI();
            ConnectToServer();
        }

        protected virtual void OnDestroy()
        {
            TcgNetwork.Get().onConnect -= OnConnectedServer;
            TcgNetwork.Get().Messaging.UnListenMsg("refresh");
        }

        protected virtual void Update()
        {
            bool isStarting = gameData==null||gameData.state==GameState.Connecting;
            bool isClient = !gameSetting.IsHost();
            bool isConnecting = TcgNetwork.Get().IsConnecting();
            bool isConnected = TcgNetwork.Get().IsConnected();
            
            //Exit game scene if cannot connect after a while
            if (isStarting && isClient)
            {
                timer += Time.deltaTime;
                if (timer > 10f)
                {
                    SceneNav.GoTo("Menu");
                }
            }
            
            //Reconnect to server
            if (!isStarting && !isConnecting && !isConnected && !isClient)
            {
                timer += Time.deltaTime;
                if (timer > 10f)
                {
                    timer = 0;
                    ConnectToServer();
                }
            }
        }
        
        public virtual void ConnectToAPI()
        {
            //Should already be logged in from the menu
            //If not connected, start in test mode (this means game scene was launched directly from Unity)
            if (!Authenticator.Get().IsSignedIn())
            {
                Authenticator.Get().LoginTest("Player");
                if(!playerSettings.HasDeck())
                    playerSettings.deck = new UserDeckData(GamePlayData.Get().testDeck);
                if (!aiSettings.HasDeck())
                {
                    aiSettings.deck = new UserDeckData(GamePlayData.Get().testDeck);
                    aiSettings.aiLevel = GamePlayData.Get().aiLevel;
                }
            }
            
            //Set avatar, cardback based on your api data
            UserData udata = Authenticator.Get().UserData;
            if (udata != null)
            {
                playerSettings.avatar = udata.GetAvatar();
                playerSettings.cardback = udata.GetCardback();
            }
            
        }

        public virtual async void ConnectToServer()
        {
            await TimeTool.Delay(100);
            
            if (TcgNetwork.Get().IsActive())
                return; // Already connected
            
            Debug.Log($"NetworkData is : {NetworkData.Get()==null}");
            if (gameSetting.IsHost() && NetworkData.Get().soloType == SoloType.Offline)
            {
                TcgNetwork.Get().StartHostOffline();    //WebGL dont support hosting a game, must join a dedicated server, in solo it starts a offline mode that doesn't use netcode at all
            }else if (gameSetting.IsHost())
            {
                TcgNetwork.Get().StartHost(NetworkData.Get().port);       //Host a game, either solo or for P2P, still using netcode in solo to have consistant behavior when testing solo vs multi
            }
            else
            {
                TcgNetwork.Get().StartClient(gameSetting.GetUrl(), NetworkData.Get().port);       //Join server
            }
        }

        public virtual async void ConnectToGame(string uid)
        {
            await TimeTool.Delay(100); //Wait for initialization to finish
            
            if (!TcgNetwork.Get().IsActive())
                return; //Not connected to server
            
            Debug.Log("Connect to Game: " + uid);
            MsgPlayerConnect nplayer = new MsgPlayerConnect
            {
                userId = Authenticator.Get().UserID,
                username = Authenticator.Get().Username,
                gameUID = uid,
                nbPlayers = gameSetting.nbPlayers,
                observer = gameSetting.gameType==GameType.Observer
            };

            Messaging.SendObject("connect", ServerID, nplayer, NetworkDelivery.Reliable);
        }

        public virtual void SendGameSettings()
        {
            if (gameSetting.IsOffline())
            {
                //Solo mode, send both your settings and AI settings
                SendGameplaySettings(gameSetting);
                SendPlayerSettingsAI(aiSettings);
                SendPlayerSettings(playerSettings);
            }else
            {
                //Online mode, only send your own settings
                SendGameplaySettings(gameSetting);
                SendPlayerSettings(playerSettings);
            }
        }

        public virtual void Disconnect()
        {
            TcgNetwork.Get().Disconnect();
        }

        private void RegisterRefresh(ushort tag, UnityAction<SerializedData> callback)
        {
            RefreshEvent refreshEvent = new RefreshEvent()
            {
                tag = tag,
                callback = callback
            };
            registeredCommands.Add(tag, refreshEvent);
        }

        public void OnReceiveRefresh(ulong clientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ushort type);
            bool found = registeredCommands.TryGetValue(type, out RefreshEvent refreshEvent);
            if (found)
            {
                refreshEvent.callback?.Invoke(new SerializedData(reader));
            }
        }
        
        //--------------------------
        public void SendPlayerSettings(PlayerSettings psettings)
        {
            SendAction(GameAction.PlayerSettings, psettings, NetworkDelivery.ReliableFragmentedSequenced);

        }
        
        public void SendPlayerSettingsAI(PlayerSettings psettings)
        {
            SendAction(GameAction.PlayerSettingsAI, psettings, NetworkDelivery.ReliableFragmentedSequenced);
        }
        
        public void SendGameplaySettings(GameSetting settings)
        {
            SendAction(GameAction.GameSettings, settings, NetworkDelivery.ReliableFragmentedSequenced);
        }
        
        public void PlayCard(Card card, Slot slot)
        {
            MsgPlayCard mdata = new MsgPlayCard
            {
                cardUID = card.uid,
                slot = slot
            };
            SendAction(GameAction.PlayCard, mdata);
        }

        public void AttackTarget(Card card, Card target)
        {
            MsgAttack mdata = new MsgAttack
            {
                attackerUID = card.uid,
                targetUID = target.uid
            };
            SendAction(GameAction.Attack, mdata);
        }

        public void AttackPlayer(Card card, Player target)
        {
            Debug.Log($"Card is {card==null}");
            Debug.Log($"Target is {target==null}");
            MsgAttackPlayer mdata = new MsgAttackPlayer
            {
                attackerUID = card.uid,
                targetID = target.id
            };
            SendAction(GameAction.AttackPlayer, mdata);
        }
        
        public void Move(Card card, Slot slot)
        {
            MsgPlayCard mdata = new MsgPlayCard
            {
                cardUID = card.uid,
                slot = slot
            };
            SendAction(GameAction.Move, mdata);
        }
        
        public void CastAbility(Card card, AbilityData ability)
        {
            MsgCastAbility mdata = new MsgCastAbility
            {
                casterUID = card.uid,
                abilityID = ability.id,
                targetUID = ""
            };
            SendAction(GameAction.CastAbility, mdata);
        }
        
        public void SelectCard(Card card)
        {
            MsgCard mdata = new MsgCard
            {
                cardUID = card.uid
            };
            SendAction(GameAction.SelectCard, mdata);
        }
        
        public void SelectPlayer(Player player)
        {
            MsgPlayer mdata = new MsgPlayer
            {
                playerID = player.id
            };
            SendAction(GameAction.SelectPlayer, mdata);
        }

        public void EvolveCard(Card card,bool isPlayer)
        {
            MsgEvolveCard mdata = new MsgEvolveCard()
            {
                cardUID = card.uid,
                isPlayer = isPlayer
            };
            SendAction(GameAction.EvolveCard, mdata);
        }

        public void SuperEvolveCard(Card card,bool isPlayer)
        {
            Debug.Log("Super Evolve card");
            MsgEvolveCard mdata = new MsgEvolveCard()
            {
                cardUID = card.uid,
                isPlayer = isPlayer
            };
            SendAction(GameAction.SuperEvolveCard, mdata);
            Debug.Log("Super Evolve card");
        }
        
        public void SendAction<T>(ushort type, T data, NetworkDelivery delivery = NetworkDelivery.Reliable) where T : INetworkSerializable
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(type);
            writer.WriteNetworkSerializable(data);
            Messaging.Send("action", ServerID, writer, delivery);
            writer.Dispose();
        }
        
        public void SendAction(ushort type, int data)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(type);
            writer.WriteValueSafe(data);
            Messaging.Send("action", ServerID, writer, NetworkDelivery.Reliable);
            writer.Dispose();
        }
        
        public void SendAction(ushort type)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(type);
            Messaging.Send("action", ServerID, writer, NetworkDelivery.Reliable);
            writer.Dispose();
        }
        
        public void SelectSlot(Slot slot)
        {
            SendAction(GameAction.SelectSlot, slot);
        }
        
        public void SelectChoice(int c)
        {
            MsgInt choice = new MsgInt
            {
                value = c
            };
            SendAction(GameAction.SelectChoice, choice);
        }
        
        public void SelectCost(int c)
        {
            MsgInt choice = new MsgInt
            {
                value = c
            };
            SendAction(GameAction.SelectCost, choice);
        }
        
        public void Mulligan(string[] cards)
        {
            MsgMulligan mdata = new MsgMulligan
            {
                cards = cards
            };
            SendAction(GameAction.SelectMulligan, mdata);
        }
        
        public void CancelSelection()
        {
            SendAction(GameAction.CancelSelect);
        }
        
        public void SendChatMsg(string msg)
        {
            MsgChat chat = new MsgChat
            {
                msg = msg,
                player_id = playerId
            };
            SendAction(GameAction.ChatMessage, chat);
        }
        
        public void EndTurn()
        {
            SendAction(GameAction.EndTurn);
        }
        
        public void Resign()
        {
            SendAction(GameAction.Resign);
        }
        
        public void SetObserverMode(int playerID)
        {
            observerMode = true;
            observerPlayerId = playerID;
        }
        
        public void SetObserverMode(string username)
        {
            observerPlayerId = 0; //Default value of observe_user not found

            Game data = GetGameData();
            foreach (Player player in data.players)
            {
                if (player.username == username)
                {
                    observerPlayerId = player.id;
                }
            }
        }
        
        //--- Receive Refresh ----------------------
        protected virtual void OnConnectedServer()
        {
            ConnectToGame(gameSetting.gameUid);
            onConnectServer?.Invoke();
        }

        protected virtual void OnConnectedToGame(SerializedData sdata)
        {
            MsgAfterConnected msg = sdata.Get<MsgAfterConnected>();
            playerId = msg.playerId;
            gameData = msg.gameData;
            observerMode = playerId < 0; //Will usually return -1 if its an observer

            if (observerMode)
                SetObserverMode(observerUser);

            onConnectGame?.Invoke();

            SendGameSettings();
        }
        
        protected virtual void OnPlayerReady(SerializedData sdata)
        {
            MsgInt msg = sdata.Get<MsgInt>();
            int pid = msg.value;

            onPlayerReady?.Invoke(pid);
        }
        
        private void OnGameStart(SerializedData sdata)
        {
            onGameStart?.Invoke();
        }
        
        private void OnGameEnd(SerializedData sdata)
        {
            MsgPlayer msg = sdata.Get<MsgPlayer>();
            onGameEnd?.Invoke(msg.playerID);
        }

        
        private void OnNewTurn(SerializedData sdata)
        {
            MsgPlayer msg = sdata.Get<MsgPlayer>();
            onNewTurn?.Invoke(msg.playerID);
        }
        
        private void OnCardPlayed(SerializedData sdata)
        {
            MsgPlayCard msg = sdata.Get<MsgPlayCard>();
            Card card = gameData.GetCard(msg.cardUID);
            onCardPlayed?.Invoke(card, msg.slot);
        }
        
        private void OnCardSummoned(SerializedData sdata)
        {
            MsgPlayCard msg = sdata.Get<MsgPlayCard>();
            onCardSummoned?.Invoke(msg.slot);
        }
        
        private void OnCardMoved(SerializedData sdata)
        {
            MsgPlayCard msg = sdata.Get<MsgPlayCard>();
            Card card = gameData.GetCard(msg.cardUID);
            onCardMoved?.Invoke(card, msg.slot);
        }
        
        private void OnCardTransformed(SerializedData sdata)
        {
            MsgCard msg = sdata.Get<MsgCard>();
            Card card = gameData.GetCard(msg.cardUID);
            onCardTransformed?.Invoke(card);
        }
        
        private void OnCardDiscarded(SerializedData sdata)
        {
            MsgCard msg = sdata.Get<MsgCard>();
            Card card = gameData.GetCard(msg.cardUID);
            onCardDiscarded?.Invoke(card);
        }
        
        private void OnCardDraw(SerializedData sdata)
        {
            MsgInt msg = sdata.Get<MsgInt>();
            onCardDraw?.Invoke(msg.value);
        }
        
        private void OnValueRolled(SerializedData sdata)
        {
            MsgInt msg = sdata.Get<MsgInt>();
            onValueRolled?.Invoke(msg.value);
        }
        
        private void OnAttackStart(SerializedData sdata)
        {
            MsgAttack msg = sdata.Get<MsgAttack>();
            Card attacker = gameData.GetCard(msg.attackerUID);
            Card target = gameData.GetCard(msg.targetUID);
            onAttackStart?.Invoke(attacker, target);
        }
        
        private void OnAttackEnd(SerializedData sdata)
        {
            MsgAttack msg = sdata.Get<MsgAttack>();
            Card attacker = gameData.GetCard(msg.attackerUID);
            Card target = gameData.GetCard(msg.targetUID);
            onAttackEnd?.Invoke(attacker, target);
        }

        private void OnAttackPlayerStart(SerializedData sdata)
        {
            MsgAttackPlayer msg = sdata.Get<MsgAttackPlayer>();
            Card attacker = gameData.GetCard(msg.attackerUID);
            Player target = gameData.GetPlayer(msg.targetID);
            onAttackPlayerStart?.Invoke(attacker, target);
        }

        private void OnAttackPlayerEnd(SerializedData sdata)
        {
            MsgAttackPlayer msg = sdata.Get<MsgAttackPlayer>();
            Card attacker = gameData.GetCard(msg.attackerUID);
            Player target = gameData.GetPlayer(msg.targetID);
            onAttackPlayerEnd?.Invoke(attacker, target);
        }

        private void OnCardDamaged(SerializedData sdata)
        {
            MsgCardValue msg = sdata.Get<MsgCardValue>();
            Card card = gameData.GetCard(msg.cardUID);
            onCardDamaged?.Invoke(card, msg.value);
        }

        private void OnPlayerDamaged(SerializedData sdata)
        {
            MsgPlayerValue msg = sdata.Get<MsgPlayerValue>();
            Player player = gameData.GetPlayer(msg.playerID);
            onPlayerDamaged?.Invoke(player, msg.value);
        }

        private void OnCardHealed(SerializedData sdata)
        {
            MsgCardValue msg = sdata.Get<MsgCardValue>();
            Card card = gameData.GetCard(msg.cardUID);
            onCardHealed?.Invoke(card, msg.value);
        }

        private void OnPlayerHealed(SerializedData sdata)
        {
            MsgPlayerValue msg = sdata.Get<MsgPlayerValue>();
            Player player = gameData.GetPlayer(msg.playerID);
            onPlayerHealed?.Invoke(player, msg.value);
        }

        private void OnCardEvolved(SerializedData sdata)
        {
            MsgEvolveCard msg = sdata.Get<MsgEvolveCard>();
            bool isPlayer = msg.isPlayer;
            Card card = gameData.GetCard(msg.cardUID);
            onCardEvolved?.Invoke(card, isPlayer);
            Debug.Log("Card Evolved: " + card.cardId);
        }

        private void OnCardSuperEvolved(SerializedData sdata)
        {
            MsgEvolveCard msg = sdata.Get<MsgEvolveCard>();
            Card card = gameData.GetCard(msg.cardUID);
            bool isPlayer = msg.isPlayer;
            onCardSuperEvolved?.Invoke(card, isPlayer);
            Debug.Log("Card Super Evolved: " + card.cardId);
        }

        private void OnPlayerEvolveCard(SerializedData sdata)
        {
            MsgCard msg = sdata.Get<MsgCard>();
            Card card = gameData.GetCard(msg.cardUID);
            onPlayerEvolveCard?.Invoke(card);
            Debug.Log("Player Evolved Card: " + card.cardId);
        }
        
        private void OnPlayerSuperEvolveCard(SerializedData sdata)
        {
            MsgCard msg = sdata.Get<MsgCard>();
            Card card = gameData.GetCard(msg.cardUID);
            onPlayerSuperEvolveCard?.Invoke(card);
            Debug.Log("Player Super Evolved Card: " + card.cardId);
        }

        private void OnAbilityTrigger(SerializedData sdata)
        {
            MsgCastAbility msg = sdata.Get<MsgCastAbility>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = gameData.GetCard(msg.casterUID);
            onAbilityStart?.Invoke(ability, caster);
        }

        private void OnAbilityTargetCard(SerializedData sdata)
        {
            MsgCastAbility msg = sdata.Get<MsgCastAbility>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = gameData.GetCard(msg.casterUID);
            Card target = gameData.GetCard(msg.targetUID);
            onAbilityTargetCard?.Invoke(ability, caster, target);
        }

        private void OnAbilityTargetPlayer(SerializedData sdata)
        {
            MsgCastAbilityPlayer msg = sdata.Get<MsgCastAbilityPlayer>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = gameData.GetCard(msg.casterUID);
            Player target = gameData.GetPlayer(msg.targetID);
            onAbilityTargetPlayer?.Invoke(ability, caster, target);
        }

        private void OnAbilityTargetSlot(SerializedData sdata)
        {
            MsgCastAbilitySlot msg = sdata.Get<MsgCastAbilitySlot>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = gameData.GetCard(msg.casterUID);
            onAbilityTargetSlot?.Invoke(ability, caster, msg.slot);
        }

        private void OnAbilityAfter(SerializedData sdata)
        {
            MsgCastAbility msg = sdata.Get<MsgCastAbility>();
            AbilityData ability = AbilityData.Get(msg.abilityID);
            Card caster = gameData.GetCard(msg.casterUID);
            onAbilityEnd?.Invoke(ability, caster);
        }

        private void OnSecretTrigger(SerializedData sdata)
        {
            MsgSecret msg = sdata.Get<MsgSecret>();
            Card secret = gameData.GetCard(msg.secretUID);
            Card triggerer = gameData.GetCard(msg.triggerUID);
            onSecretTrigger?.Invoke(secret, triggerer);
        }

        private void OnSecretResolve(SerializedData sdata)
        {
            MsgSecret msg = sdata.Get<MsgSecret>();
            Card secret = gameData.GetCard(msg.secretUID);
            Card triggerer = gameData.GetCard(msg.triggerUID);
            onSecretResolve?.Invoke(secret, triggerer);
        }

        private void OnChat(SerializedData sdata)
        {
            MsgChat msg = sdata.Get<MsgChat>();
            onChatMsg?.Invoke(msg.player_id, msg.msg);
        }

        private void OnServerMsg(SerializedData sdata)
        {
            string msg = sdata.GetString();
            onServerMsg?.Invoke(msg);
        }

        private void OnRefreshAll(SerializedData sdata)
        {
            MsgRefreshAll msg = sdata.Get<MsgRefreshAll>();
            gameData= msg.gameData;
            onRefreshAll?.Invoke();
        }
        
        //--------------------------
        public virtual bool IsReady()
        {
            return gameData != null && TcgNetwork.Get().IsConnected();
        }
        
        public Player GetPlayer()
        {
            Game gdata = GetGameData();
            return gdata.GetPlayer(GetPlayerID());
        }
        
        public Player GetOpponentPlayer()
        {
            Game gdata = GetGameData();
            return gdata.GetPlayer(GetOpponentPlayerID());
        }
        
        public int GetPlayerID()
        {
            if (observerMode)
                return observerPlayerId;
            return playerId;
        }
        
        public int GetOpponentPlayerID()
        {
            return GetPlayerID() == 0 ? 1 : 0;
        }
        
        public bool IsYourTurn()
        {
            Game game_data = GetGameData();
            Player player = GetPlayer();
            return IsReady() && game_data.IsPlayerTurn(player);
        }
        
        public bool IsObserveMode()
        {
            return observerMode;
        }
        
        public Game GetGameData()
        {
            return gameData;
        }
        
        public bool HasEnded()
        {
            return gameData!=null&&gameData.HasEnded();
        }
        private void OnApplicationQuit()
        {
            Resign(); //Auto Resign before closing the app. NOTE: doesn't seem to work since the msg dont have time to be sent before it closes
        }
        
        public static Gameclient Get()
        {
            return instance;
        }
        

       
        

        

        
        
        
        
        public bool IsHost => TcgNetwork.Get().IsHost;
        public ulong ServerID => TcgNetwork.Get().ServerID;
        public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;
    }
    
    public class RefreshEvent
    {
        public ushort tag;
        public UnityAction<SerializedData> callback;
    }
}