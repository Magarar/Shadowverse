using System.Collections.Generic;
using System.Linq;
using Ai;
using Api;
using Data;
using GameLogic;
using Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace GameServer
{
    ///<总结>
    ///在服务器上代表一个游戏，当独自玩时，它将在本地创建，
    ///或者，如果专用服务器将创建在线多个GameServer，每个比赛一个
    ///管理接收操作、发送刷新和运行AI
    ///</摘要>
    public class Gameserver
    {
        public string gameUID;//Game unique ID
        public int nbPlayers = 2;
        
        public static float gameExpireTime = 30f;      //How long for the game to be deleted when no one is connected
        public static float winExpireTime = 60f;       //How long for a player to be declared winnner if hes the only one connected

        private Game gameData;
        private Gamelogic gamePlay;
        private float expiration = 0f;
        private float winExpiration = 0f;
        private bool isDedicatedServer = false;
        
        private List<ClientData> players = new List<ClientData>();            //Exclude observers, stays in array when disconnected, only players can send commands
        private List<ClientData> connectedClients = new List<ClientData>();  //Include obervers, removed from array when disconnected, all clients receive refreshes
        private List<AIPlayer> aiList = new List<AIPlayer>();                //List of all AI players
        private Queue<QueuedGameAction> queuedActions = new Queue<QueuedGameAction>(); //List of action waiting to be processed
        
        private Dictionary<ushort, CommandEvent> registeredCommands = new Dictionary<ushort, CommandEvent>();
        
        public Gameserver(string uid, int players, bool online)
        {
            Init(uid, players, online);
        }

        

        ~Gameserver()
        {
            Clear();
        }
        
        //todo
        protected virtual void Init(string uid, int players, bool online)
        {
            gameUID = uid;
            nbPlayers = players;
            isDedicatedServer = online;
            gameData = new Game(uid,nbPlayers);
            gamePlay = new Gamelogic(gameData);
            
            //Commands
            RegisterAction(GameAction.PlayerSettings, ReceivePlayerSettings);
            RegisterAction(GameAction.PlayerSettingsAI, ReceivePlayerSettingsAI);
            RegisterAction(GameAction.GameSettings, ReceiveGameplaySettings);
            RegisterAction(GameAction.PlayCard, ReceivePlayCard);
            RegisterAction(GameAction.Attack, ReceiveAttackTarget);
            RegisterAction(GameAction.AttackPlayer, ReceiveAttackPlayer);
            RegisterAction(GameAction.Move, ReceiveMove);
            RegisterAction(GameAction.CastAbility, ReceiveCastCardAbility);
            RegisterAction(GameAction.SelectCard, ReceiveSelectCard);
            
            RegisterAction(GameAction.EvolveCard, ReceiveEvolveCard);
            RegisterAction(GameAction.SuperEvolveCard, ReceiveSuperEvolveCard);
            
            RegisterAction(GameAction.SelectPlayer, ReceiveSelectPlayer);
            RegisterAction(GameAction.SelectSlot, ReceiveSelectSlot);
            RegisterAction(GameAction.SelectChoice, ReceiveSelectChoice);
            RegisterAction(GameAction.SelectCost, ReceiveSelectCost);
            RegisterAction(GameAction.SelectMulligan, ReceiveSelectMulligan);
            RegisterAction(GameAction.CancelSelect,  ReceiveCancelSelection);
            RegisterAction(GameAction.EndTurn, ReceiveEndTurn);
            RegisterAction(GameAction.Resign, ReceiveResign);
            RegisterAction(GameAction.ChatMessage, ReceiveChat);

            //Events
            gamePlay.onGameStart += OnGameStart;
            gamePlay.onGameEnd += OnGameEnd;
            gamePlay.onTurnStart += OnTurnStart;
            gamePlay.onRefresh += RefreshAll;

            gamePlay.onCardPlayed += OnCardPlayed;
            gamePlay.onCardSummoned += OnCardSummoned;
            gamePlay.onCardMoved += OnCardMoved;
            gamePlay.onCardTransformed += OnCardTransformed;
            gamePlay.onCardDiscarded += OnCardDiscarded;
            gamePlay.onCardDraw += OnCardDraw;
            gamePlay.onRollValue += OnValueRolled;
            
            gamePlay.onCardEvolved += OnCardEvolved;
            gamePlay.onCardSuperEvolved += OnCardSuperEvolved;
            gamePlay.onPlayerEvolveCard += OnPlayerEvolved;
            gamePlay.onPlayerSuperEvolveCard += OnPlayerSuperEvolved;

            gamePlay.onAbilityStart += OnAbilityStart;
            gamePlay.onAbilityTargetCard += OnAbilityTargetCard;
            gamePlay.onAbilityTargetPlayer += OnAbilityTargetPlayer;
            gamePlay.onAbilityTargetSlot += OnAbilityTargetSlot;
            gamePlay.onAbilityEnd += OnAbilityEnd;

            gamePlay.onAttackStart += OnAttackStart;
            gamePlay.onAttackEnd += OnAttackEnd;
            gamePlay.onAttackPlayerStart += OnAttackPlayerStart;
            gamePlay.onAttackPlayerEnd += OnAttackPlayerEnd;

            gamePlay.onCardDamaged += OnCardDamaged;
            gamePlay.onPlayerDamaged += OnPlayerDamaged;
            gamePlay.onCardHealed += OnCardHealed;
            gamePlay.onPlayerHealed += OnPlayerHealed ;

            gamePlay.onSecretTrigger += OnSecretTriggered;
            gamePlay.onSecretResolve += OnSecretResolved;
        }

        //todo
        protected virtual void Clear()
        {
            gamePlay.onGameStart -= OnGameStart;
            gamePlay.onGameEnd -= OnGameEnd;
            gamePlay.onTurnStart -= OnTurnStart;
            gamePlay.onRefresh -= RefreshAll;

            gamePlay.onCardPlayed -= OnCardPlayed;
            gamePlay.onCardSummoned -= OnCardSummoned;
            gamePlay.onCardMoved -= OnCardMoved;
            gamePlay.onCardTransformed -= OnCardTransformed;
            gamePlay.onCardDiscarded -= OnCardDiscarded;
            gamePlay.onCardDraw -= OnCardDraw;
            gamePlay.onRollValue -= OnValueRolled;
            
            gamePlay.onCardEvolved -= OnCardEvolved;
            gamePlay.onCardSuperEvolved -= OnCardSuperEvolved;
            gamePlay.onPlayerEvolveCard -= OnPlayerEvolved;
            gamePlay.onPlayerSuperEvolveCard -= OnPlayerSuperEvolved;

            gamePlay.onAbilityStart -= OnAbilityStart;
            gamePlay.onAbilityTargetCard -= OnAbilityTargetCard;
            gamePlay.onAbilityTargetPlayer -= OnAbilityTargetPlayer;
            gamePlay.onAbilityTargetSlot -= OnAbilityTargetSlot;
            gamePlay.onAbilityEnd -= OnAbilityEnd;
            gamePlay.onAttackStart -= OnAttackStart;
            gamePlay.onAttackEnd -= OnAttackEnd;
            gamePlay.onAttackPlayerStart -= OnAttackPlayerStart;
            gamePlay.onAttackPlayerEnd -= OnAttackPlayerEnd;
            gamePlay.onCardDamaged -= OnCardDamaged;
            gamePlay.onPlayerDamaged -= OnPlayerDamaged;

            gamePlay.onSecretTrigger -= OnSecretTriggered;
            gamePlay.onSecretResolve -= OnSecretResolved;
            
            
        }

        public virtual void Update()
        {
            //如果没有人连接或游戏结束，则游戏到期
            int connectedPlayers = CountConnectedClients();
            if (HasGameEnded() || connectedPlayers == 0)
                expiration += Time.deltaTime;
            
            //Win expiration if all other players left
            if (connectedPlayers == 1 && HasGameStarted() && !HasGameEnded())
                winExpiration += Time.deltaTime;
            
            if (isDedicatedServer && !HasGameEnded() && IsWinExpired())
                EndExpiredGame();
            
            //Timer during game
            if (gameData.state == GameState.Play && !gamePlay.IsResolving())
            {
                gameData.turnTimer -= Time.deltaTime;
                if (gameData.turnTimer <= 0)
                {
                    gamePlay.NextStep();
                }
            }
            
            //Start Game when ready
            if (gameData.state == GameState.Connecting)
            {
                bool allConnected = gameData.AreAllPlayersConnected();
                bool allReady = gameData.AreAllPlayersReady();
                if (allConnected && allReady)
                {
                    StartGame();
                }
            }
            
            //Process queued actions
            if (queuedActions.Count > 0 && !gamePlay.IsResolving())
            {
                QueuedGameAction action = queuedActions.Dequeue();
                ExecuteAction(action.type, action.client, action.sdata);
            }
            
            //Update game logic
            gamePlay.Update(Time.deltaTime);

            foreach (AIPlayer ai in aiList)
            {
                ai.Update();
            }

        }

        protected virtual void StartGame()
        {
            bool aiVsAi = !isDedicatedServer&&GamePlayData.Get().aiVsAi;
            foreach (Player player in gameData.players)
            {
                if (player.isAI || aiVsAi)
                {
                    AIPlayer aiGamePlay = AIPlayer.Create(GamePlayData.Get().aiType, gamePlay, player.id, player.aiLevel);
                    aiList.Add(aiGamePlay);
                }
            }
            gamePlay.StartGame();
        }
        
        //End game when it has expired (only one player is still connected)
        protected virtual void EndExpiredGame()
        {
            Game gdata = gamePlay.GetGameData();
            foreach (Player player in gdata.players)
            {
                if (player.IsConnected())
                {
                    gamePlay.EndGame(player.id);
                    return;
                }
            }
        }
        
        //------ Receive Actions -------
        private void RegisterAction(ushort tag, UnityAction<ClientData, SerializedData> callback)
        {
            CommandEvent cmdevt = new CommandEvent
            {
                tag = tag,
                callback = callback
            };
            registeredCommands.Add(tag, cmdevt);
        }

        public void ReceiveAction(ulong clientID, FastBufferReader reader)
        {
            ClientData client = GetClient(clientID);
            if (client != null)
            {
                reader.ReadValueSafe(out ushort type);
                SerializedData sdata = new SerializedData(reader);
                if (!gamePlay.IsResolving())
                {
                    //Not resolving, execute now
                    ExecuteAction(type, client, sdata);
                }
                else
                {
                    //Resolving, wait before executing
                    QueuedGameAction action = new QueuedGameAction
                    {
                        type = type,
                        client = client,
                        sdata = sdata
                    };
                    sdata.PreRead();
                    queuedActions.Enqueue(action);
                }
            }
        }

        public void ExecuteAction(ushort type, ClientData client, SerializedData sdata)
        {
            bool found = registeredCommands.TryGetValue(type, out CommandEvent command);
            if(found)
                command.callback.Invoke(client, sdata);
        }
        
        //----------

        public void ReceivePlayerSettings(ClientData iclient, SerializedData sdata)
        {
            PlayerSettings msg = sdata.Get<PlayerSettings>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null)
            {
                SetPlayerSettings(player.id, msg);
            }
        }

        public void ReceivePlayerSettingsAI(ClientData iclient, SerializedData sdata)
        {
            PlayerSettings msg = sdata.Get<PlayerSettings>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null)
            {
                SetPlayerSettingsAI(player.id, msg);
            }
        }

        public void ReceiveGameplaySettings(ClientData iclient, SerializedData sdata)
        {
            GameSetting settings = sdata.Get<GameSetting>();
            if (settings != null)
            {
                SetGameSettings(settings);
            }
        }

        public void ReceivePlayCard(ClientData iclient, SerializedData sdata)
        {
            MsgPlayCard msg = sdata.Get<MsgPlayCard>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && gameData.IsPlayerActionTurn(player) && !gamePlay.IsResolving())
            {
                Card card = player.GetCard(msg.cardUID);
                if (card != null && card.playerID == player.id)
                    gamePlay.PlayCard(card, msg.slot);
            }
        }

        public void ReceiveAttackTarget(ClientData iclient, SerializedData sdata)
        {
            MsgAttack msg = sdata.Get<MsgAttack>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && gameData.IsPlayerActionTurn(player) && !gamePlay.IsResolving())
            {
                Card attacker = player.GetCard(msg.attackerUID);
                Card target = gameData.GetCard(msg.targetUID);
                if (attacker != null && target != null && attacker.playerID == player.id)
                {
                    gamePlay.AttackTarget(attacker, target);
                }
            }
        }

        public void ReceiveAttackPlayer(ClientData iclient, SerializedData sdata)
        {
            MsgAttackPlayer msg = sdata.Get<MsgAttackPlayer>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && gameData.IsPlayerActionTurn(player) && !gamePlay.IsResolving())
            {
                Player target = gameData.GetPlayer(msg.targetID);
                Card attacker = player.GetCard(msg.attackerUID);
                if (attacker != null && target != null && attacker.playerID == player.id)
                {
                    gamePlay.AttackPlayer(attacker, target);
                }
            }
        }

        public void ReceiveMove(ClientData iclient, SerializedData sdata)
        {
            MsgPlayCard msg = sdata.Get<MsgPlayCard>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && gameData.IsPlayerActionTurn(player) && !gamePlay.IsResolving())
            {
                Card card = player.GetCard(msg.cardUID);
                if (card != null && card.playerID == player.id)
                {
                    gamePlay.MoveCard(card, msg.slot);
                }
            }
        }

        public void ReceiveEvolveCard(ClientData iclient, SerializedData sdata)
        {
            MsgEvolveCard msg = sdata.Get<MsgEvolveCard>();
            bool isPlayer = msg.isPlayer;
            Card card = gameData.GetCard(msg.cardUID);
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && gameData.IsPlayerActionTurn(player) && !gamePlay.IsResolving())
            {
                Debug.Log("Evolve");
                gamePlay.EvolveCard(card, isPlayer);
            }
        }
        
        public void ReceiveSuperEvolveCard(ClientData iclient, SerializedData sdata)
        {
            MsgEvolveCard msg = sdata.Get<MsgEvolveCard>();
            bool isPlayer = msg.isPlayer;
            Card card = gameData.GetCard(msg.cardUID);
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && gameData.IsPlayerActionTurn(player) && !gamePlay.IsResolving())
            {
                Debug.Log("Super Evolve");
                gamePlay.SuperEvolveCard(card,isPlayer);
            }
        }

        public void ReceiveCastCardAbility(ClientData iclient, SerializedData sdata)
        {
            MsgCastAbility msg = sdata.Get<MsgCastAbility>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && gameData.IsPlayerActionTurn(player) && !gamePlay.IsResolving())
            {
                Card card = player.GetCard(msg.casterUID);
                AbilityData ability = AbilityData.Get(msg.abilityID);
                if (card != null && ability != null && card.playerID == player.id)
                {
                    gamePlay.CastAbility(card, ability);
                }
            }
        }

        public void ReceiveSelectCard(ClientData iclient, SerializedData sdata)
        {
            MsgCard msg = sdata.Get<MsgCard>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && gameData.IsPlayerSelectorTurn(player) && !gamePlay.IsResolving())
            {
                Card card = gameData.GetCard(msg.cardUID);
                gamePlay.SelectCard(card);
            }
        }

        public void ReceiveSelectPlayer(ClientData iclient, SerializedData sdata)
        {
            MsgPlayer msg = sdata.Get<MsgPlayer>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && gameData.IsPlayerSelectorTurn(player) && !gamePlay.IsResolving())
            {
                Player target = gameData.GetPlayer(msg.playerID);
                gamePlay.SelectPlayer(target);
            }
        }

        public void ReceiveSelectSlot(ClientData iclient, SerializedData sdata)
        {
            Slot slot = sdata.Get<Slot>();
            Player player = GetPlayer(iclient);
            if (player != null && slot != null && gameData.IsPlayerSelectorTurn(player) && !gamePlay.IsResolving())
            {
                gamePlay.SelectSlot(slot);
            }
        }

        public void ReceiveSelectChoice(ClientData iclient, SerializedData sdata)
        {
            MsgInt msg = sdata.Get<MsgInt>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && gameData.IsPlayerSelectorTurn(player) && !gamePlay.IsResolving())
            {
                gamePlay.SelectChoice(msg.value);
            }
        }

        public void ReceiveSelectCost(ClientData iclient, SerializedData sdata)
        {
            MsgInt msg = sdata.Get<MsgInt>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && gameData.IsPlayerSelectorTurn(player) && !gamePlay.IsResolving())
            {
                gamePlay.SelectCost(msg.value);
            }
        }

        public void ReceiveSelectMulligan(ClientData iclient, SerializedData sdata)
        {
            MsgMulligan msg = sdata.Get<MsgMulligan>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && gameData.IsPlayerMulliganTurn(player) && !gamePlay.IsResolving())
            {
                gamePlay.Mulligan(player, msg.cards);
            }
        }
        
        public void ReceiveCancelSelection(ClientData iclient, SerializedData sdata)
        {
            Player player = GetPlayer(iclient);
            if (player != null && gameData.IsPlayerSelectorTurn(player) && !gamePlay.IsResolving())
            {
                gamePlay.CancelSelection();
            }
        }

        public void ReceiveResign(ClientData iclient, SerializedData sdata)
        {
            Player player = GetPlayer(iclient);
            if (player != null && gameData.state != GameState.Connecting && gameData.state != GameState.GameEnded)
            {
                int winner = player.id == 0 ? 1 : 0;
                gamePlay.EndGame(winner);
            }
        }

        public void ReceiveEndTurn(ClientData iclient, SerializedData sdata)
        {
            Player player = GetPlayer(iclient);
            if (player != null && gameData.IsPlayerTurn( player))
            {
                gamePlay.NextStep();
            }
        }

        public void ReceiveChat(ClientData iclient, SerializedData sdata)
        {
            MsgChat msg = sdata.Get<MsgChat>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null)
            {
                msg.player_id = player.id; //Force player id to sending client to avoid spoofing
                SendToAll(GameAction.ChatMessage, msg, NetworkDelivery.Reliable);
            }
        }
        
        //--- Setup Commands ------
        public virtual async void SetPlayerDeck(int playerID, string username, UserDeckData deck)
        {
            Player player = gameData.GetPlayer(playerID);
            if (player != null && gameData.state == GameState.Connecting)
            {
                UserData user = Authenticator.Get().UserData; //Offline game, get local user

                if (Authenticator.Get().IsApi())
                {
                    user = await ApiClient.Get().LoadUserData(username); //Online game, validate from api
                }
                
                player.articlesAll.Clear();
                //Use user API deck
                UserDeckData udeck = user?.GetDeck(deck.tid);
                if (user != null && udeck != null)
                {
                    if (user.IsDeckValid(udeck))
                    {
                        gamePlay.SetPlayerDeck(player, udeck);
                        SendPlayerReady(player);
                        return;
                    }
                    else
                    {
                        Debug.Log(user.username + " deck is invalid: " + udeck.title);
                        return;
                    }
                }
                
                //Use premade deck
                DeckData cdeck = DeckData.Get(deck.tid);
                if (cdeck != null)
                    gamePlay.SetPlayerDeck(player, cdeck);
                //Trust client in test mode
                else if (Authenticator.Get().IsTest())
                    gamePlay.SetPlayerDeck(player, deck);
                //Deck not found
                else
                    Debug.Log("Player " + playerID + " deck not found: " + deck.tid);
                SendPlayerReady(player);

            }
        }

        public virtual void SetPlayerSettings(int playerID, PlayerSettings psettings)
        {
            if(gameData.state != GameState.Connecting)
                return;//Cant send setting if game already started
            
            Player player = gameData.GetPlayer(playerID);
            if (player != null && !player.ready)
            {
                player.avatar = psettings.avatar;
                player.cardBack = psettings.cardback;
                player.isAI = false;
                player.ready = true;
                SetPlayerDeck(playerID, player.username, psettings.deck);
                RefreshAll();
            }
        }

        public virtual void SetPlayerSettingsAI(int playerID, PlayerSettings psettings)
        {
            if(gameData.state != GameState.Connecting)
                return;//Cant send setting if game already started
            if(isDedicatedServer)
                return;//No AI allowed online server
            
            Player player = gameData.GetOpponentPlayer(playerID);
            if (player != null && !player.ready)
            {
                player.username = psettings.username;
                player.avatar = psettings.avatar;
                player.cardBack = psettings.cardback;
                player.isAI = true;
                player.ready = true;
                player.aiLevel = psettings.aiLevel;
                
                SetPlayerDeck(player.id, player.username, psettings.deck);
                RefreshAll();
            }
        }
        
        public virtual void SetGameSettings(GameSetting settings)
        {
            if (gameData.state == GameState.Connecting)
            {
                gameData.settings = settings;
                RefreshAll();
            }
        }
        
        //-------------
        public void AddClient(ClientData client)
        {
            if (!connectedClients.Contains(client))
                connectedClients.Add(client);
        }

        public void RemoveClient(ClientData client)
        {
            connectedClients.Remove(client);

            Player player = GetPlayer(client);
            if (player != null && player.IsConnected())
            {
                player.connected = false;
                RefreshAll();
            }
        }

        public ClientData GetClient(ulong clientID)
        {
            return connectedClients.FirstOrDefault(client => client.clientID == clientID);
        }

        public int AddPlayer(ClientData client)
        {
            if (!players.Contains(client))
                players.Add(client);
            
            int playerID = FindPlayerID(client.userID);
            Player player = gameData.GetPlayer(playerID);
            if (player != null)
            {
                player.connected = true;
                player.username = client.username;
            }
            return playerID;
        }

        private int FindPlayerID(string userID)
        {
            int index = 0;
            foreach (ClientData player in players)
            {
                if (player.userID == userID)
                    return index;
                index++;
            }
            return -1;
        }

        public Player GetPlayer(ClientData client)
        {
            return GetPlayer(client.userID);
        }

        public Player GetPlayer(string userID)
        {
            int playerID = FindPlayerID(userID);
            return gameData?.GetPlayer(playerID);
        }

        public bool IsPlayer(string userID)
        {
            Player player = GetPlayer(userID);
            return player != null;
        }

        public bool IsConnectedPlayer(string userID)
        {
            Player player = GetPlayer(userID);
            return player != null && player.connected;
        }

        public int CountPlayers()
        {
            return players.Count;
        }

        public int CountConnectedClients()
        {
            Game game = GetGameData();
            return game.players.Count(player => player.IsConnected());
        }
        
        public Game GetGameData()
        {
            return gamePlay.GetGameData();
        }

        public virtual bool HasGameStarted()
        {
            return gamePlay.IsGameStarted();
        }

        public virtual bool HasGameEnded()
        {
            return gamePlay.IsGameEnded();
        }

        public virtual bool IsGameExpired()
        {
            return expiration > gameExpireTime; //Means that the game expired (everyone left or game ended)
        }

        public virtual bool IsWinExpired()
        {
            return winExpiration > winExpireTime;//Means that only one player is left, and he should win
        }

        protected virtual void OnGameStart()
        {
            SendToAll(GameAction.GameStart);
            if (isDedicatedServer && Authenticator.Get().IsApi())
            {
                ApiClient.Get().CreateMatch(gameData);
            }
            
        }

        protected virtual void OnGameEnd(Player winner)
        {
            MsgPlayer msg = new MsgPlayer();
            msg.playerID=winner?.id ?? -1;
            SendToAll(GameAction.GameEnd, msg, NetworkDelivery.Reliable);
            if (isDedicatedServer && Authenticator.Get().IsApi())
            {
                //End Match and give rewards
                ApiClient.Get().EndMatch(gameData, winner.id);
            }
        }

        protected virtual void OnTurnStart()
        {
            MsgPlayer msg = new MsgPlayer()
            {
                playerID = gameData.currentPlayer,
            };
            SendToAll(GameAction.NewTurn, msg, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardPlayed(Card card, Slot slot)
        {
            MsgPlayCard mdata = new MsgPlayCard()
            {
                cardUID = card.uid,
                slot = slot
            };
            SendToAll(GameAction.CardPlayed, mdata, NetworkDelivery.Reliable);

        }

        protected virtual void OnCardMoved(Card card, Slot slot)
        {
            MsgPlayCard mdata = new MsgPlayCard
            {
                cardUID = card.uid,
                slot = slot
            };
            SendToAll(GameAction.CardMoved, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardSummoned(Card card, Slot slot)
        {
            MsgPlayCard mdata = new MsgPlayCard
            {
                cardUID = card.uid,
                slot = slot
            };
            SendToAll(GameAction.CardSummoned, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardTransformed(Card card)
        {
            MsgCard mdata = new MsgCard
            {
                cardUID = card.uid
            };
            SendToAll(GameAction.CardTransformed, mdata, NetworkDelivery.Reliable);
        }
        
        protected virtual void OnCardDiscarded(Card card)
        {
            MsgCard mdata = new MsgCard
            {
                cardUID = card.uid
            };
            SendToAll(GameAction.CardDiscarded, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardDraw(int nb)
        {
            MsgInt mdata = new MsgInt
            {
                value = nb
            };
            SendToAll(GameAction.CardDrawn, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnPlayerEvolved(Card card)
        {
            MsgCard mdata = new MsgCard
            {
                cardUID = card.uid
            };
            SendToAll(GameAction.PlayerEvolveCard, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnPlayerSuperEvolved(Card card)
        {
            MsgCard mdata = new MsgCard
            {
                cardUID = card.uid
            };
            SendToAll(GameAction.PlayerSuperEvolveCard, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardEvolved(Card card, bool isPlayer)
        {
            MsgEvolveCard mdata = new MsgEvolveCard
            {
                cardUID = card.uid,
                isPlayer = isPlayer
            };
            SendToAll(GameAction.CardEvolved, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardSuperEvolved(Card card, bool isPlayer)
        {
            MsgEvolveCard mdata = new MsgEvolveCard
            {
                cardUID = card.uid,
                isPlayer = isPlayer
            };
            SendToAll(GameAction.CardSuperEvolved, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnValueRolled(int nb)
        {
            MsgInt mdata = new MsgInt
            {
                value = nb
            };
            SendToAll(GameAction.ValueRolled, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnAttackStart(Card attacker, Card target)
        {
            MsgAttack mdata = new MsgAttack
            {
                attackerUID = attacker.uid,
                targetUID = target.uid,
                damage = 0
            };
            SendToAll(GameAction.AttackStart, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnAttackEnd(Card attacker, Card target)
        {
            MsgAttack mdata = new MsgAttack
            {
                attackerUID = attacker.uid,
                targetUID = target.uid,
                damage = 0
            };
            SendToAll(GameAction.AttackEnd, mdata, NetworkDelivery.Reliable);
        }
        
        protected virtual void OnAttackPlayerStart(Card attacker, Player target)
        {
            MsgAttackPlayer mdata = new MsgAttackPlayer
            {
                attackerUID = attacker.uid,
                targetID = target.id,
                damage = 0
            };
            SendToAll(GameAction.AttackPlayerStart, mdata, NetworkDelivery.Reliable);
        }
        
        protected virtual void OnAttackPlayerEnd(Card attacker, Player target)
        {
            MsgAttackPlayer mdata = new MsgAttackPlayer
            {
                attackerUID = attacker.uid,
                targetID = target.id,
                damage = 0
            };
            SendToAll(GameAction.AttackPlayerEnd, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardDamaged(Card card, int damage)
        {
            MsgCardValue mdata = new MsgCardValue
            {
                cardUID = card.uid,
                value = damage
            };
            SendToAll(GameAction.CardDamaged, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnPlayerDamaged(Player player, int damage)
        {
            MsgPlayerValue mdata = new MsgPlayerValue
            {
                playerID = player.id,
                value = damage
            };
            SendToAll(GameAction.PlayerDamaged, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardHealed(Card card, int hp)
        {
            MsgCardValue mdata = new MsgCardValue
            {
                cardUID = card.uid,
                value = hp
            };
            SendToAll(GameAction.CardHealed, mdata, NetworkDelivery.Reliable);
        }
        
        protected virtual void OnPlayerHealed(Player player, int hp)
        {
            MsgPlayerValue mdata = new MsgPlayerValue
            {
                playerID = player.id,
                value = hp
            };
            SendToAll(GameAction.PlayerHealed, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnAbilityStart(AbilityData ability, Card caster)
        {
            MsgCastAbility mdata = new MsgCastAbility
            {
                abilityID = ability.id,
                casterUID = caster.uid,
                targetUID = ""
            };
            SendToAll(GameAction.AbilityTrigger, mdata, NetworkDelivery.Reliable);
        }
        
        protected virtual void OnAbilityTargetCard(AbilityData ability, Card caster, Card target)
        {
            MsgCastAbility mdata = new MsgCastAbility
            {
                abilityID = ability.id,
                casterUID = caster.uid,
                targetUID = target != null ? target.uid : ""
            };
            SendToAll(GameAction.AbilityTargetCard, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnAbilityTargetPlayer(AbilityData ability, Card caster, Player target)
        {
            MsgCastAbilityPlayer mdata = new MsgCastAbilityPlayer
            {
                abilityID = ability.id,
                casterUID = caster.uid,
                targetID = target?.id ?? -1
            };
            SendToAll(GameAction.AbilityTargetPlayer, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnAbilityTargetSlot(AbilityData ability, Card caster, Slot target)
        {
            MsgCastAbilitySlot mdata = new MsgCastAbilitySlot
            {
                abilityID = ability.id,
                casterUID = caster.uid,
                slot = target
            };
            SendToAll(GameAction.AbilityTargetSlot, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnAbilityEnd(AbilityData ability, Card caster)
        {
            MsgCastAbility mdata = new MsgCastAbility
            {
                abilityID = ability.id,
                casterUID = caster.uid,
                targetUID = ""
            };
            SendToAll(GameAction.AbilityEnd, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnSecretTriggered(Card secret, Card trigger)
        {
            MsgSecret mdata = new MsgSecret
            {
                secretUID = secret.uid,
                triggerUID = trigger != null ? trigger.uid : ""
            };
            SendToAll(GameAction.SecretTriggered, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnSecretResolved(Card secret, Card trigger)
        {
            MsgSecret mdata = new MsgSecret
            {
                secretUID = secret.uid,
                triggerUID = trigger != null ? trigger.uid : ""
            };
            SendToAll(GameAction.SecretResolved, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void SendPlayerReady(Player player)
        {
            if (player != null && player.IsReady())
            {
                MsgInt mdata = new MsgInt();
                mdata.value = player.id;
                SendToAll(GameAction.PlayerReady, mdata, NetworkDelivery.Reliable);
            }
        }
        
        public virtual void RefreshAll()
        {
            MsgRefreshAll mdata = new MsgRefreshAll
            {
                gameData = GetGameData()
            };
            SendToAll(GameAction.RefreshAll, mdata, NetworkDelivery.ReliableFragmentedSequenced);
        }

        private void SendToAll(ushort tag)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(tag);
            foreach (var client in connectedClients)
            {
                if (client != null)
                {
                    Messaging.Send("refresh", client.clientID, writer, NetworkDelivery.Reliable);
                }
            }
        }
        

        private void SendToAll(ushort tag, INetworkSerializable data, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(tag);
            writer.WriteNetworkSerializable(data);
            foreach (ClientData iclient in connectedClients)
            {
                if (iclient != null)
                {
                    Messaging.Send("refresh", iclient.clientID, writer, delivery);
                }
            }
            writer.Dispose();
        }
        
        public ulong ServerID => TcgNetwork.Get().ServerID;
        public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;
    }
    
    public struct QueuedGameAction
    {
        public ushort type;
        public ClientData client;
        public SerializedData sdata;
    }
}