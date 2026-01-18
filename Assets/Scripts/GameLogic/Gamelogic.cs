using System.Collections.Generic;
using System.Linq;
using Data;
using Unit;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.Profiling;

namespace GameLogic
{
    /// <summary>
    /// Execute and resolves game rules and logic
    /// 执行并解析游戏规则和逻辑。
    /// </summary>
    public class Gamelogic
    {
        public UnityAction onGameStart;
        public UnityAction<Player> onGameEnd;
        
        public UnityAction onTurnStart;
        public UnityAction onTurnPlay;
        public UnityAction onTurnEnd;
        
        public UnityAction<Card, Slot> onCardPlayed;

        public UnityAction<Card, bool> onCardEvolved;
        public UnityAction<Card, bool> onCardSuperEvolved;
        public UnityAction<Card> onPlayerEvolveCard;
        public UnityAction<Card> onPlayerSuperEvolveCard;
        
        public UnityAction<Card, Slot> onCardSummoned;
        public UnityAction<Card, Slot> onCardMoved;
        public UnityAction<Card> onCardTransformed;
        public UnityAction<Card> onCardDiscarded;
        public UnityAction<int> onCardDraw;
        public UnityAction<int> onRollValue;
        
        public UnityAction<AbilityData, Card> onAbilityStart;
        public UnityAction<AbilityData, Card, Card> onAbilityTargetCard;  //Ability, Caster, Target
        public UnityAction<AbilityData, Card, Player> onAbilityTargetPlayer;
        public UnityAction<AbilityData, Card, Slot> onAbilityTargetSlot;
        public UnityAction<AbilityData, Card> onAbilityEnd;
        
        public UnityAction<Card, Card> onAttackStart;  //Attacker, Defender
        public UnityAction<Card, Card> onAttackEnd;     //Attacker, Defender
        public UnityAction<Card, Player> onAttackPlayerStart;
        public UnityAction<Card, Player> onAttackPlayerEnd;
        
        public UnityAction<Card, int> onCardDamaged;
        public UnityAction<Card, int> onCardHealed;
        public UnityAction<Player, int> onPlayerDamaged;
        public UnityAction<Player, int> onPlayerHealed;
        
        public UnityAction<Card, Card> onSecretTrigger;    //Secret, Triggerer
        public UnityAction<Card, Card> onSecretResolve;    //Secret, Triggerer
        
        public UnityAction onRefresh;

        public Game gameData;
        
        private ResolveQueue resolveQueue;
        private bool isAiPredict;

        private System.Random rand = new System.Random();

        private ListSwap<Card> cardArray = new();
        private ListSwap<Player> playerArray = new();
        private ListSwap<Slot> slotArray = new();
        private ListSwap<CardData> cardDataArray = new();
        private List<Card> cardsToClear = new();

        public Gamelogic(bool isAi)
        {
            //is_instant ignores all gameplay delays and process everything immediately, needed for AI prediction
            resolveQueue = new ResolveQueue(null,isAi);
            isAiPredict = isAi;
        }

        public Gamelogic(Game game)
        {
            gameData = game;
            resolveQueue = new ResolveQueue(game,false);
        }

        public virtual void SetData(Game game)
        {
            gameData = game;
            resolveQueue.SetData(game);
        }

        public virtual void Update(float delta)
        {
            resolveQueue.Update(delta);
        }
        
        //----- Turn Phases ----------
        public virtual void StartGame()
        {
            if(gameData.state == GameState.GameEnded)
                return;
            
            //Choose first player
            gameData.state = GameState.Play;
            gameData.firstPlayer = rand.NextDouble()<0.5? 0:1;
            gameData.currentPlayer = gameData.firstPlayer;
            gameData.turnCount = 0;
            
            bool shouldMulligan = GamePlayData.Get().mulligan;

            
            //Adventure settings
            LevelData level = gameData.settings.GetLevel();
            if (level != null)
            {
                if (level != null && level.firstPlayer == LevelFirst.Player)
                    gameData.firstPlayer = 0;
                if (level != null && level.firstPlayer == LevelFirst.AI)
                    gameData.firstPlayer = 1;
                gameData.currentPlayer = gameData.firstPlayer;
                shouldMulligan = level.mulligan;
            }
            
            //Init each players
            foreach (var player in gameData.players)
            {
                //Puzzle level deck
                DeckPuzzleData pdeck = DeckPuzzleData.Get(player.deck);
                
                player.hpMax = pdeck!=null? pdeck.startHp:GamePlayData.Get().hpStart;
                player.hp = player.hpMax;
                player.manaMax = pdeck!=null? pdeck.startMana:GamePlayData.Get().manaStart;
                player.mana = player.manaMax;
                player.evolutionPointMax = 2;
                player.superEvolutionPointMax = 2;
                player.evolutionPoint = 0;
                player.superEvolutionPoint = 0;
                
                //Draw starting cards
                int dcards = pdeck != null ? pdeck.startCards : GamePlayData.Get().cardsStart;
                DrawCard(player, dcards);
                
                //Add coin second player
                bool isRandom = level==null||level.firstPlayer == LevelFirst.Random;
                if (isRandom && player.id == gameData.firstPlayer)
                {
                    player.SetCanUseExtraMana(false);
                    player.enableEvolutionPointTurn = 5;
                    player.enableSuperEvolutionPointTurn = 7;
                }
                else
                {
                    player.enableEvolutionPointTurn = 4;
                    player.enableSuperEvolutionPointTurn = 6; 
                }
            }
            
            //Start state
            RefreshData();
            onGameStart?.Invoke();

            if(shouldMulligan)
                GoToMulligan();
            else
                StartTurn();
        }

        public virtual void StartTurn()
        {
            if(gameData.state==GameState.GameEnded)
                return;

            ClearTurnData();
            gameData.phase = GamePhase.StartTurn;
            RefreshData();
            onTurnPlay?.Invoke();

            Player player = gameData.GetActivePlayer();
            if (gameData.turnCount > 0)
            {
                DrawCard(player, GamePlayData.Get().cardPerTurn);
            }
            
            //Mana
            player.manaMax += GamePlayData.Get().manaPerTurn;
            player.manaMax = Mathf.Min(player.manaMax, GamePlayData.Get().manaMax);
            player.mana = player.manaMax;
            
            //Turn timer and history
            gameData.turnTimer = GamePlayData.Get().turnDuration;
            player.historyList.Clear();
            
            //Player poison
            if(player.HasStatus(StatusType.Poisoned))
                player.hp -= player.GetStatusValue(StatusType.Poisoned);
            if(player.hero!=null)
                player.hero.Refresh();

            if (player.id != gameData.firstPlayer && gameData.turnCount is 0 or 5)
            {
                player.SetCanUseExtraMana(true);
            }

            player.canUseEvolution = true;

            player.enableEvolutionPointTurn--;
            if (player.enableEvolutionPointTurn == 0)
            {
                player.enableEvolution = true;
                player.evolutionPoint = player.evolutionPointMax;
            }

            player.enableSuperEvolutionPointTurn--;
            if (player.enableSuperEvolutionPointTurn == 0)
            {
                player.enableSuperEvolution = true;
                player.superEvolutionPoint = player.superEvolutionPointMax;
            }
            
            //Refresh Cards and Status Effects
            for (int i = player.cardsBoard.Count - 1; i >= 0; i--)
            {
                Card card = player.cardsBoard[i];
                if(!card.HasStatus(StatusType.Sleep))
                    card.Refresh();
                if (card.HasStatus(StatusType.Poisoned))
                    DamageCard(card, card.GetStatusValue(StatusType.Poisoned));
            }
            
            //Ongoing Abilities
            UpdateOngoing();
            
            //StartTurn Abilities
            TriggerPlayerCardsAbilityType(player, AbilityTrigger.StartOfTurn);
            TriggerPlayerSecrets(player, AbilityTrigger.StartOfTurn);
            
            resolveQueue.AddCallback(StartMainPhase);
            resolveQueue.ResolveAll(0.2f);
        }

        public virtual void StartNextTurn()
        {
            if(gameData.state==GameState.GameEnded)
                return;
            gameData.currentPlayer = (gameData.currentPlayer+1)%gameData.settings.nbPlayers;
            if (gameData.currentPlayer == gameData.firstPlayer)
            {
                gameData.turnCount++;
            }
               
            CheckForWinner();
            StartTurn();
        }

        public virtual void StartMainPhase()
        {
            if(gameData.state==GameState.GameEnded)
                return;
            gameData.phase = GamePhase.Main;
            onTurnPlay?.Invoke();
            RefreshData();
        }

        public virtual void EndTurn()
        {
            if(gameData.state==GameState.GameEnded)
                return;
            if(gameData.phase!=GamePhase.Main)
                return;

            gameData.selector = SelectorType.None;
            gameData.phase = GamePhase.EndTurn;
            
            //Reduce status effects with duration
            foreach (var aplayer in gameData.players)
            {
                aplayer.ReduceStatusDurations();
                foreach (var card in aplayer.cardsBoard)
                    card.ReduceStatusDurations();
                foreach (var card in aplayer.cardsEquip)
                    card.ReduceStatusDurations();
            }
            
            //End of turn abilities
            Player player = gameData.GetActivePlayer();
            player.manaOngoing = 0;
            TriggerPlayerCardsAbilityType(player, AbilityTrigger.EndOfTurn);
            
            onTurnPlay?.Invoke();
            RefreshData();
            
            resolveQueue.AddCallback(StartNextTurn);
            resolveQueue.ResolveAll(0.2f);
        }
        
        //End game with winner
        public virtual void EndGame(int winner)
        {
            if (gameData.state != GameState.GameEnded)
            {
                gameData.state = GameState.GameEnded;
                gameData.phase = GamePhase.None;
                gameData.selector = SelectorType.None;
                gameData.currentPlayer = winner;
                resolveQueue.Clear();
                Player player = gameData.GetPlayer(winner);
                onGameEnd?.Invoke(player);
                RefreshData();
            }
        }
        
        //Progress to the next step/phase 
        public virtual void NextStep()
        {
            if (gameData.state == GameState.GameEnded)
                return;
            if (gameData.phase == GamePhase.Mulligan)
            {
                StartTurn();
                return;
            }
            
            CancelSelection();
            //Add to resolve queue in case its still resolving
            resolveQueue.AddCallback(EndTurn);
            resolveQueue.ResolveAll();
        }

        //Check if a player is winning the game, if so end the game
        //Change or edit this function for a new win condition
        protected virtual void CheckForWinner()
        {
            int countAlive = 0;
            Player alive = null;
            foreach (var player in gameData.players)
            {
                if (!player.IsDead())
                {
                    alive = player;
                    countAlive++;
                }
            }
            if (countAlive == 0)
            {
                EndGame(-1); //Everyone is dead, Draw
            }
            else if (countAlive == 1)
            {
                EndGame(alive.id); //Player win
            }
        }

        protected virtual void ClearTurnData()
        {
            gameData.selector = SelectorType.None;
            resolveQueue.Clear();
            cardArray.Clear();
            playerArray.Clear();
            slotArray.Clear();
            cardDataArray.Clear();
            gameData.lastPlayed = null;
            gameData.lastDestroyed = null;
            gameData.lastTarget= null;
            gameData.lastSummoned = null;
            gameData.abilityTrigger = null;
            gameData.selectedValue = 0;
            gameData.abilityPlayed.Clear();
            gameData.cardsAttacked.Clear();
        }
        
        //--- Setup ------
        
        //Set deck using a Deck in Resources
        public virtual void SetPlayerDeck(Player player, DeckData deck)
        {
            player.cardsAll.Clear();
            player.cardsDeck.Clear();
            player.deck = deck.id;
            player.hero = null;
            
            if (deck.hero != null)
            {
                player.hero = Card.Create(deck.hero,deck.hero.GetVariant(),player);
            }

            foreach (var card in deck.cards)
            {
                if (card != null)
                {
                    Card acard = Card.Create(card, card.GetVariant(), player);
                    player.cardsDeck.Add(acard);
                }
            }
            DeckPuzzleData puzzle = deck as DeckPuzzleData;
            if (puzzle != null)
            {
                foreach (var card in puzzle.boardCards)
                {
                    Card acard = Card.Create(card.card, card.card.GetVariant(), player);
                    acard.slot = new Slot(card.slot,Slot.GetP(player.id));
                    player.cardsBoard.Add(acard);
                }
            }
            if (puzzle == null || !puzzle.dontShuffleDeck)
                ShuffleDeck(player.cardsDeck);
        }
        
        //Set deck using custom deck in save file or database
        public virtual void SetPlayerDeck(Player player, UserDeckData deck)
        {
            player.cardsAll.Clear();
            player.cardsDeck.Clear();
            player.deck = deck.tid;
            player.hero = null;

            if (deck.hero != null)
            {
                CardData hdata = CardData.Get(deck.hero.tid);
                VariantData variant = hdata.GetVariant();
                if (hdata != null && variant != null)
                {
                    Debug.Log("Hero: " + deck.hero.tid);
                    player.hero = Card.Create(hdata, variant, player);
                }
            }

            foreach (var card in deck.cards)
            {
                CardData cdata = CardData.Get(card.tid);
                VariantData variant = cdata.GetVariant();
                if (cdata != null && variant != null)
                {
                    for (int i = 0; i < card.quantity; i++)
                    {
                        Card acard = Card.Create(cdata, variant, player);
                        player.cardsDeck.Add(acard);
                    }
                }
            }
            
            //Shuffle deck
            ShuffleDeck(player.cardsDeck);
        }
        
        //---- Gameplay Actions --------------
        public virtual void PlayCard(Card card, Slot slot, bool skipCost = false)
        {
            if (gameData.CanPlayCard(card, slot, skipCost))
            {
                Player player = gameData.GetPlayer(card.playerID);
                
                //cost
                if (!skipCost)
                    player.PayMana(card);
                
                //Play card
                player.RemoveCardFromAllGroups(card);
                
                //Add to board
                CardData icard = card.CardData;
                if (icard.IsBoardCard())
                {
                    player.cardsBoard.Add(card);
                    card.slot = slot;
                    card.exhausted = true; //Cant attack first turn
                    card.canAttackPlayer = false;
                }else if (icard.IsEquipment())
                {
                    Card bearer = gameData.GetSlotCard(card.slot);
                    EquipCard(bearer, card);
                    card.exhausted = true;
                    card.canAttackPlayer = false;
                }else if (icard.IsSecret())
                {
                    player.cardsSecret.Add(card);
                }
                else
                {
                    player.cardsDiscard.Add(card);
                    card.slot = slot;//Save slot in case spell has PlayTarget
                }
                
                //History
                if(!isAiPredict&&!icard.IsSecret())
                    player.AddHistory(GameAction.PlayCard, card);
                
                //Update ongoing effects
                gameData.lastPlayed = card.uid;
                UpdateOngoing();
                
                //Trigger abilities
                if (card.CardData.IsDynamicManaCost())
                {
                    GoToSelectorCost(card);
                }
                else
                {
                    TriggerSecrets(AbilityTrigger.OnPlayOther, card); //After playing card
                    TriggerCardAbilityType(AbilityTrigger.OnPlay, card);
                    TriggerOtherCardsAbilityType(AbilityTrigger.OnPlayOther, card);
                }
                
                RefreshData();
                onCardPlayed?.Invoke(card,slot);
                resolveQueue.ResolveAll(0.3f);
            }
        }

        public virtual void MoveCard(Card card, Slot slot, bool skipCost = false)
        {
            if (gameData.CanMoveCard(card, slot, skipCost))
            {
                card.slot = slot;
                
                //Also move the equipment
                Card equip = gameData.GetEquipCard(card.uid);
                if (equip != null)
                    equip.slot = slot;
                UpdateOngoing();
                RefreshData();
                
                onCardMoved?.Invoke(card,slot);
                resolveQueue.ResolveAll(0.2f);
            }
        }

        public virtual void CastAbility(Card card, AbilityData iability)
        {
            Debug.Log("Cast ability 1: " + iability.name+" "+card.cardId);
            if (gameData.CanCastAbility(card, iability))
            {
                Debug.Log("Cast ability 2: " + iability.name+" "+card.cardId);
                Player player = gameData.GetPlayer(card.playerID);
                if(!isAiPredict&&iability.target!=AbilityTarget.SelectTarget)
                    player.AddHistory(GameAction.CastAbility, card, iability);
                card.RemoveStatus(StatusType.Stealth);
                TriggerCardAbility(iability, card);
                resolveQueue.ResolveAll(0.2f);
            }
        }

        public virtual void AttackTarget(Card attacker, Card target, bool skipCost = false)
        {
            if (gameData.CanAttackTarget(attacker, target, skipCost))
            {
                Player player = gameData.GetPlayer(attacker.playerID);
                if (!isAiPredict)
                {
                    player.AddHistory(GameAction.Attack, attacker, target);
                }
                gameData.lastTarget = target.uid;
                
                //Trigger before attack abilities
                TriggerCardAbilityType(AbilityTrigger.OnBeforeAttack, attacker, target);
                TriggerCardAbilityType(AbilityTrigger.OnBeforeDefend, target, attacker);
                TriggerSecrets(AbilityTrigger.OnBeforeAttack, attacker);
                TriggerSecrets(AbilityTrigger.OnBeforeDefend, target);
                
                resolveQueue.AddAttack(attacker, target,ResolveAttack, skipCost);
                resolveQueue.ResolveAll();
            }
        }

        protected virtual void ResolveAttack(Card attacker, Card target, bool skipCost)
        {
            if(!gameData.IsOnBoard(attacker)||!gameData.IsOnBoard(target))
                return;
            onAttackStart?.Invoke(attacker,target);
            
            attacker.RemoveStatus(StatusType.Stealth);
            UpdateOngoing();
            
            resolveQueue.AddAttack(attacker, target, ResolveAttackHit, skipCost);
            resolveQueue.ResolveAll(0.3f);
        }

        protected virtual void ResolveAttackHit(Card attacker, Card target, bool skipCost)
        {
            //Count attack damage
            int datt1 = attacker.GetAttack();
            int datt2 = target.GetAttack();
            
            //Damage Cards
            DamageCard(attacker, target, datt1);
            
            //Counter Damage
            if (!attacker.HasStatus(StatusType.Intimidate))
                DamageCard(target, attacker, datt2);
            
            //Save attack and exhaust
            if (!skipCost)
                ExhaustBattle(attacker);
            
            //Recalculate bonus
            UpdateOngoing();

            //Abilities
            bool attBoard = gameData.IsOnBoard(attacker);
            bool defBoard = gameData.IsOnBoard(target);
            if (attBoard)
                TriggerCardAbilityType(AbilityTrigger.OnAfterAttack, attacker, target);
            if (defBoard)
                TriggerCardAbilityType(AbilityTrigger.OnAfterDefend, target, attacker);
            if (attBoard)
                TriggerSecrets(AbilityTrigger.OnAfterAttack, attacker);
            if (defBoard)
                TriggerSecrets(AbilityTrigger.OnAfterDefend, target);
            
            onAttackEnd?.Invoke(attacker,target);
            RefreshData();
            CheckForWinner();
            
            resolveQueue.ResolveAll(0.3f);
        }

        public virtual void AttackPlayer(Card attacker, Player target, bool skipCost = false)
        {
            if (attacker == null || target == null)
                return;
            
            if(!gameData.CanAttackTarget(attacker, target, skipCost))
                return;
            
            Player player = gameData.GetPlayer(attacker.playerID);
            if (!isAiPredict)
                player.AddHistory(GameAction.AttackPlayer, attacker, target);
            
            //Resolve abilities
            TriggerSecrets(AbilityTrigger.OnBeforeAttack, attacker);
            TriggerCardAbilityType(AbilityTrigger.OnBeforeAttack, attacker, target);
            
            resolveQueue.AddAttack(attacker, target, ResolveAttackPlayer, skipCost);
            resolveQueue.ResolveAll();

        }

        protected virtual void ResolveAttackPlayer(Card attacker, Player target, bool skipCost)
        {
            if(!gameData.IsOnBoard(attacker))
                return;
            onAttackPlayerStart?.Invoke(attacker,target);
            
            attacker.RemoveStatus(StatusType.Stealth);
            UpdateOngoing();
            
            resolveQueue.AddAttack(attacker, target, ResolveAttackPlayerHit, skipCost);
            resolveQueue.ResolveAll(0.3f);
        }

        protected virtual void ResolveAttackPlayerHit(Card attacker, Player target, bool skipCost)
        {
            DamagePlayer(attacker, target, attacker.GetAttack());
            
            //Save attack and exhaust
            if(!skipCost)
                ExhaustBattle(attacker);
            
            //Recalculate bonus
            UpdateOngoing();
            
            if(gameData.IsOnBoard(attacker))
                TriggerCardAbilityType(AbilityTrigger.OnAfterAttack, attacker, target);
            TriggerSecrets(AbilityTrigger.OnAfterAttack, attacker);
            
            onAttackPlayerEnd?.Invoke(attacker,target);
            RefreshData();
            CheckForWinner();
            
            resolveQueue.ResolveAll(0.2f);
        }
        
        //Exhaust after battle
        public virtual void ExhaustBattle(Card attacker)
        {
            bool attackedBefore = gameData.cardsAttacked.Contains(attacker.uid);
            gameData.cardsAttacked.Add(attacker.uid);
            bool attackAgain = attacker.HasStatus(StatusType.Fury)&&!attackedBefore;
            attacker.exhausted = !attackAgain;
        }
        
        //Redirect attack to a new target
        public virtual void RedirectAttack(Card attacker, Card newTarget)
        {
            foreach (AttackQueueElement att in resolveQueue.GetAttackQueue())
            {
                if (att.attacker.uid == attacker.uid)
                {
                    att.target = newTarget;
                    att.ptarget = null;
                    att.callback = ResolveAttack;
                    att.pcallback = null;
                }
            }
        }

        public virtual void RedirectAttack(Card attacker, Player newTarget)
        {
            foreach (AttackQueueElement att in resolveQueue.GetAttackQueue())
            {
                if (att.attacker.uid == attacker.uid)
                {
                    att.ptarget = newTarget;
                    att.target = null;
                    att.callback = null;
                    att.pcallback = ResolveAttackPlayer;
                }
            }
        }

        public virtual void ShuffleDeck(List<Card> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Card temp = cards[i];
                int randomIndex = rand.Next(i, cards.Count);
                cards[i] = cards[randomIndex];
                cards[randomIndex] = temp;
            }
        }

        public virtual void DrawCard(Player player, int nb = 1)
        {
            for (int i = 0; i < nb; i++)
            {
                if (player.cardsDeck.Count > 0 && player.cardsHand.Count < GamePlayData.Get().cardMax)
                {
                    Card card = player.cardsDeck[0];
                    player.cardsDeck.RemoveAt(0);
                    player.cardsHand.Add(card);
                }
            }

            onCardDraw?.Invoke(nb);
        }
        
        public virtual void DrawCardWithTrait(Player player,TraitData trait, int nb = 1)
        {
            List<Card> cards =player.cardsDeck.FindAll(c=>c.HasTrait(trait));
            nb = Mathf.Min(nb,cards.Count);
            for (int i = 0; i < nb; i++)
            {
                if (player.cardsDeck.Count > 0 && player.cardsHand.Count < GamePlayData.Get().cardMax)
                {
                    Card card = cards[i];
                    player.cardsDeck.Remove(card);
                    player.cardsHand.Add(card);
                }
            }

            onCardDraw?.Invoke(nb);
        }

        public virtual void AddArticle(Player player,Article article)
        {
            if (player.articles.Any(t => t.articleId == article.articleId))
            {
                return;
            }
            if(player.articles.Count>=5)
                return;
            player.articles.Add(article);

        }
        
        public virtual void AddArticle(Player player, ArticleData articleData)
        {
            if (player.articles.Any(t => t.articleId == articleData.id))
            {
                return;
            }
            if(player.articles.Count>=5)
                return;
            Article article = Article.Create(articleData,player);
            AddArticle(player,article);
        }
        
        public virtual void RemoveArticle(Article article)
        {
            if (article == null)
                return;
            
            // if (gameData.IsInDiscard(article))
            //     return; //Already discarded
            
            ArticleData icard = article.ArticleData;
            Player player = gameData.GetPlayer(article.playerID);
            
            //Remove article from board and add to discard
            player.RemoveArticleFromAllGroups(article);

            // if (wasOnBoard)
            // {
            //     //Trigger on death abilities
            //     TriggerCardAbilityType(AbilityTrigger.OnDeath, article);
            //     TriggerOtherCardsAbilityType(AbilityTrigger.OnDeathOther, article);
            //     TriggerSecrets(AbilityTrigger.OnDeathOther, article);
            //     UpdateOngoingCards(); //Not UpdateOngoing() here to avoid recursive calls in UpdateOngoingKills
            // }
            //
            // cardsToClear.Add(article); //将在下一次更新中清除（），以便同时产生伤害效果
            // onCardDiscarded?.Invoke(article);
        }
        
        //Put a card from deck into discard
        public virtual void DrawDiscardCard(Player player, int nb = 1)
        {
            for (int i = 0; i < nb; i++)
            {
                if (player.cardsDeck.Count > 0)
                {
                    Card card = player.cardsDeck[0];
                    player.cardsDeck.RemoveAt(0);
                    player.cardsDiscard.Add(card);
                }
            }
        }
        
        //Summon copy of an exiting card
        public virtual Card SummonCopy(Player player, Card copy, Slot slot)
        {
            CardData icard = copy.CardData;
            return SummonCard(player, icard, copy.VariantData, slot);
        }
        
        //Summon copy of an exiting card into hand
        public virtual Card SummonCopyHand(Player player, Card copy)
        {
            CardData icard = copy.CardData;
            return SummonCardHand(player, icard, copy.VariantData);
        }

        public virtual Card SummonCard(Player player, CardData card, VariantData variant, Slot slot)
        {
            if (!slot.IsValid())
                return null;
            if(gameData.GetSlotCard(slot)!=null)
                return null;
            Card acard = SummonCardHand(player, card, variant);
            PlayCard(acard, slot, true);
            onCardSummoned?.Invoke(acard, slot);

            return acard;
        }
        
        //Create a new card and send it to your hand
        public virtual Card SummonCardHand(Player player, CardData card, VariantData variant)
        {
            Card acard = Card.Create(card, variant, player);
            player.cardsHand.Add(acard);
            gameData.lastSummoned = acard.uid;
            return acard;
        }

        public virtual Card TransformCard(Card card, CardData transformTo)
        {
            card.SetCard(transformTo, card.VariantData);

            onCardTransformed?.Invoke(card);

            return card;
        }

        public virtual void EquipCard(Card card, Card equipment)
        {
            if (card != null && equipment != null && card.playerID == equipment.playerID)
            {
                if (!card.CardData.IsEquipment() && equipment.CardData.IsEquipment())
                {
                    UnequipAll(card); //Unequip previous cards, only 1 equip at a time
                    
                    Player player = gameData.GetPlayer(card.playerID);
                    player.RemoveCardFromAllGroups(equipment);
                    player.cardsEquip.Add(equipment);
                    card.equippedUid = equipment.uid;
                    equipment.slot = card.slot;
                }
            }
        }

        public virtual void UnequipAll(Card card)
        {
            if (card != null && card.equippedUid != null)
            {
                Player player = gameData.GetPlayer(card.playerID);
                Card equip = player.GetEquipCard(card.equippedUid);
                if (equip != null)
                {
                    card.equippedUid = null;
                    DiscardCard(equip);
                }
            }
        }
        
        //Change owner of a card
        public virtual void ChangeOwner(Card card, Player owner)
        {
            if (card.playerID != owner.id)
            {
                Player cardOwner = gameData.GetPlayer(card.playerID);
                cardOwner.RemoveCardFromAllGroups(card);
                cardOwner.cardsAll.Remove(card.uid);
                owner.cardsAll[card.uid] = card;
                card.playerID = owner.id;
            }
        }

        //Damage a player
        public virtual void DamagePlayer(Card attacker, Player target, int value)
        {
            target.hp -= value;
            target.hp = Mathf.Clamp(target.hp,0, target.hpMax);
            
            //Lifesteal
            Player aplayer = gameData.GetPlayer(attacker.playerID);
            if (attacker.HasStatus(StatusType.LifeSteal))
                aplayer.hp += value;
            
            onPlayerDamaged?.Invoke(target, value);
        }

        public virtual void HealCard(Card target, int value)
        {
            if (target == null)
                return;
            if (target.HasStatus(StatusType.Invincibility))
                return;
            target.damaged -= value;
            target.damaged = Mathf.Max(target.damaged, 0);
            
            onCardHealed?.Invoke(target, value);
        }

        public virtual void HealPlayer(Player target, int value)
        {
            if (target == null)
                return;
            target.hp += value;
            target.hp = Mathf.Clamp(target.hp, 0, target.hpMax);
            
            onPlayerHealed?.Invoke(target, value);
        }
        
        //Generic damage that doesnt come from another card
        public virtual void DamageCard(Card target, int value)
        {
            if (target == null)
                return;
            if (target.HasStatus(StatusType.Invincibility))
                return;
            if (target.HasStatus(StatusType.SpellImmunity))
                return;
            if (target.HasStatus(StatusType.SuperEvolved) && gameData.currentPlayer == target.playerID)
                value = 0;
            target.damaged+= value;
            
            onCardDamaged?.Invoke(target, value);
            if (target.GetHp()<=0)
                DiscardCard(target);
        }
        
        //Damage a card with attacker/caster
        public virtual void DamageCard(Card attacker, Card target, int value, bool spellDamage = false)
        {
            if (attacker == null || target == null)
                return;
            if (target.HasStatus(StatusType.Invincibility))
                return; //Invincible

            if (target.HasStatus(StatusType.SpellImmunity) && attacker.CardData.type != CardType.Character)
                return; //Spell immunity
            //Shell
            bool doublelife = target.HasStatus(StatusType.Shell);
            if (doublelife && value > 0)
            {
                target.RemoveStatus(StatusType.Shell);
                return;
            }
            //Armor
            if (!spellDamage && target.HasStatus(StatusType.Armor))
                value = Mathf.Max(value - target.GetStatusValue(StatusType.Armor), 0);
            
            //Damage
            if (target.HasStatus(StatusType.SuperEvolved) && gameData.currentPlayer == target.playerID)
                value = 0;
            
            int damageMax = Mathf.Min(value, target.GetHp());
            int extra = value-target.GetHp();
            target.damaged += damageMax;
            
            //Trample
            Player tplayer = gameData.GetPlayer(target.playerID);
            if (!spellDamage && extra > 0 && attacker.playerID == gameData.currentPlayer && attacker.HasStatus(StatusType.Trample))
                tplayer.hp -= extra;
            
            //Lifesteal
            Player player = gameData.GetPlayer(attacker.playerID);
            if (!spellDamage && attacker.HasStatus(StatusType.LifeSteal))
                player.hp += damageMax;
            
            //Remove sleep on damage
            target.RemoveStatus(StatusType.Sleep);
            
            //Callback
            onCardDamaged?.Invoke(target, value);
            
            //Deathtouch
            if (value > 0 && attacker.HasStatus(StatusType.Deathtouch) && target.CardData.type == CardType.Character)
                KillCard(attacker, target);
            
            //Kill card if no hp
            if (target.GetHp() <= 0)
                KillCard(attacker, target);
        }

        public virtual void EvolveCard(Card card,bool isPlayer)
        {
            if(card==null)
                return;
            if (!card.CanEvolve())
                return;

            Player player = gameData.GetPlayer(card.playerID);
             if(!player.CanUseEvolution()&&isPlayer)
                 return;

             if (isPlayer)
             {
                 player.evolutionPoint--;
                 player.evolutionPoint = Mathf.Clamp(player.evolutionPoint, 0, player.evolutionPointMax);
                 player.canUseEvolution = false;
             }
             Debug.Log($"Evolution {card.uid} {player.evolutionPoint}");
             card.hasEvolved = true;
            CastAbility(card,AbilityData.GetEvolveAbility());
            if (isPlayer)
            {
                TriggerCardAbilityType(AbilityTrigger.OnPlayerEvolved, card);
                onPlayerEvolveCard?.Invoke(card);
            }
            TriggerCardAbilityType(AbilityTrigger.OnEvolved, card);
            TriggerOtherCardsAbilityType(AbilityTrigger.OnOtherEvolved, card);
            onCardEvolved?.Invoke(card, isPlayer);
            RefreshData();
            
        }
        
        public virtual void SuperEvolveCard(Card card,bool isPlayer)
        {
            if(card==null)
                return;
            if (!card.CanEvolve())
                return;

            Player player = gameData.GetPlayer(card.playerID);
            if(!player.CanUseEvolution()&&isPlayer)
                return;

            if (isPlayer)
            {
                player.superEvolutionPoint--;
                player.superEvolutionPoint = Mathf.Clamp(player.superEvolutionPoint, 0, player.superEvolutionPointMax);
                player.canUseEvolution = false;
            }
            card.hasEvolved = true;
            CastAbility(card,AbilityData.GetSuperEvolveAbility());
            if (isPlayer)
            {
                TriggerCardAbilityType(AbilityTrigger.OnPlayerSuperEvolved, card);
                onPlayerSuperEvolveCard?.Invoke(card);
            }
            TriggerCardAbilityType(AbilityTrigger.OnEvolved, card);
            TriggerCardAbilityType(AbilityTrigger.OnSuperEvolved, card);
            TriggerOtherCardsAbilityType(AbilityTrigger.OnOtherSuperEvolved, card);
            TriggerOtherCardsAbilityType(AbilityTrigger.OnOtherEvolved, card);
            onCardSuperEvolved?.Invoke(card, isPlayer);
            Debug.Log($"Super Evolution {card.uid} {player.id}");
            RefreshData();
        }
        
        

        //A card that kills another card
        public virtual void KillCard(Card attacker, Card target)
        {
            if (attacker == null || target == null)
                return;
            if (!gameData.IsOnBoard(target) && !gameData.IsEquipped(target))
                return; //Already killed
            if (target.HasStatus(StatusType.Invincibility))
                return; //Cant be killed
            Player pattacker = gameData.GetPlayer(attacker.playerID);
            if (attacker.playerID != target.playerID)
                pattacker.killCount++;
            DiscardCard(target);
            TriggerCardAbilityType(AbilityTrigger.OnKill, attacker, target);
        }


        //Send card into discard
        public virtual void DiscardCard(Card card)
        {
            if (card == null)
                return;
            
            if (gameData.IsInDiscard(card))
                return; //Already discarded
            
            CardData icard = card.CardData;
            Player player = gameData.GetPlayer(card.playerID);
            bool wasOnBoard = gameData.IsOnBoard(card) || gameData.IsEquipped(card);
            
            //Unequip card
            UnequipAll(card);
            
            //Remove card from board and add to discard
            player.RemoveCardFromAllGroups(card);
            player.cardsDiscard.Add(card);
            gameData.lastDestroyed = card.uid;
            
            //Remove from bearer
            Card bearer = player.GetBearerCard(card);
            if (bearer != null)
                bearer.equippedUid = null;

            if (wasOnBoard)
            {
                //Trigger on death abilities
                TriggerCardAbilityType(AbilityTrigger.OnDeath, card);
                TriggerOtherCardsAbilityType(AbilityTrigger.OnDeathOther, card);
                TriggerSecrets(AbilityTrigger.OnDeathOther, card);
                UpdateOngoingCards(); //Not UpdateOngoing() here to avoid recursive calls in UpdateOngoingKills
            }
            
            cardsToClear.Add(card); //将在下一次更新中清除（），以便同时产生伤害效果
            onCardDiscarded?.Invoke(card);
        }

        public int RollRandomValue(int dice)
        {
            return RollRandomValue(1, dice + 1);
        }

        public virtual int RollRandomValue(int min, int max)
        {
            gameData.rolledValue = rand.Next(min, max);
            onRollValue?.Invoke(gameData.rolledValue);
            resolveQueue.SetDelay(1f);
            return gameData.rolledValue;
        }
        
        //--- Abilities --
        public virtual void TriggerCardAbilityType(AbilityTrigger type, Card caster, Card trigger = null)
        {
            foreach (AbilityData ability in caster.GetAbilities())
            {
                if (ability && ability.trigger == type)
                {
                    TriggerCardAbility(ability, caster, trigger);
                }
            }
            Card equipped = gameData.GetEquipCard(caster.equippedUid);
            if (equipped != null)
                TriggerCardAbilityType(type, equipped, trigger);
        }
        
        public virtual void TriggerCardAbilityType(AbilityTrigger type, Card caster, Player trigger)
        {
            foreach (AbilityData iability in caster.GetAbilities())
            {
                if (iability && iability.trigger == type)
                {
                    TriggerCardAbility(iability, caster, trigger);
                }
            }
            Card equipped = gameData.GetEquipCard(caster.equippedUid);
            if (equipped != null)
                TriggerCardAbilityType(type, equipped, trigger);
        }
        
        public virtual void TriggerOtherCardsAbilityType(AbilityTrigger type, Card trigger)
        {
            foreach (Player oplayer in gameData.players)
            {
                if (oplayer.hero != null)
                    TriggerCardAbilityType(type, oplayer.hero, trigger);

                foreach (Card card in oplayer.cardsBoard)
                    TriggerCardAbilityType(type, card, trigger);
            }
        }
        
        public virtual void TriggerPlayerCardsAbilityType(Player player, AbilityTrigger type)
        {
            if (player.hero != null)
                TriggerCardAbilityType(type, player.hero, player.hero);

            foreach (Card card in player.cardsBoard)
                TriggerCardAbilityType(type, card, card);
        }
        

        public virtual void TriggerCardAbility(AbilityData ability, Card caster)
        {
            TriggerCardAbility(ability, caster, caster);
        }
        

        public virtual void TriggerCardAbility(AbilityData ability, Card caster, Card trigger)
        {
            Card triggerCard = trigger ?? caster;
            if(!caster.HasStatus(StatusType.Silenced)&&ability.AreTriggerConditionsMet(gameData, caster, triggerCard))
                resolveQueue.AddAbility(ability, caster, triggerCard, ResolveCardAbility);
        }

        public virtual void TriggerCardAbility(AbilityData ability, Card caster, Player trigger)
        {
            if (!caster.HasStatus(StatusType.Silenced) && ability.AreTriggerConditionsMet(gameData, caster, trigger))
            {
                resolveQueue.AddAbility(ability, caster, caster, ResolveCardAbility);
            }
        }
        

        public virtual void TriggerAbilityDelayed(AbilityData iability, Card caster)
        {
            resolveQueue.AddAbility(iability, caster, caster, TriggerCardAbility);
        }
        

        public virtual void TriggerAbilityDelayed(AbilityData ability, Card caster, Card trigger)
        {
            Card triggerCard = trigger ?? caster; //Triggerer is the caster if not set
            resolveQueue.AddAbility(ability, caster, triggerCard, TriggerCardAbility);
        }
        
        
        //Resolve a card ability, may stop to ask for target
        protected virtual void ResolveCardAbility(AbilityData ability, Card caster, Card trigger)
        {
            if (!caster.CanDoAbilities())
                return; //Silenced card cant cast
            
            //Debug.Log("Trigger Ability " + iability.id + " : " + caster.card_id);
            onAbilityStart?.Invoke(ability, caster);
            gameData.abilityTrigger = trigger.uid;
            gameData.abilityPlayed.Add(ability.id);
            
            Debug.Log("Resolve Ability " + ability.id + " : " + caster.cardId);
            
            bool isSelector = ResolveCardAbilitySelector(ability, caster);
            if(isSelector)
                return;
            ResolveCardAbilityPlayTarget(ability, caster);
            ResolveCardAbilityPlayers(ability, caster);
            ResolveCardAbilityCards(ability, caster);
            ResolveCardAbilitySlots(ability, caster);
            ResolveCardAbilityCardData(ability, caster);
            ResolveCardAbilityNoTarget(ability, caster);
            AfterAbilityResolved(ability, caster);
            
        }
        

        protected virtual bool ResolveCardAbilitySelector(AbilityData ability, Card caster)
        {
            if (ability.target == AbilityTarget.SelectTarget)
            {
                //Wait for target
                GoToSelectTarget(ability, caster);
                return true;
            }
            else if (ability.target == AbilityTarget.CardSelector)
            {
                GoToSelectorCard(ability, caster);
                return true;
            }else if (ability.target == AbilityTarget.ChoiceSelector)
            {
                GoToSelectorChoice(ability, caster);
                return true;
            }
            return false;
        }

        //spell only
        protected virtual void ResolveCardAbilityPlayTarget(AbilityData ability, Card caster)
        {
            if (ability.target == AbilityTarget.PlayTarget)
            {
                Slot slot = caster.slot;
                Card slotCard = gameData.GetSlotCard(slot);
                if (slot.IsPlayerSlot())
                {
                    Player tplayer = gameData.GetPlayer(slot.p);
                    if (ability.CanTarget(gameData, caster, tplayer))
                        ResolveEffectTarget(ability, caster, tplayer);
                }else if (slotCard != null)
                {
                    if (ability.CanTarget(gameData, caster, slotCard))
                    {
                        gameData.lastTarget = slotCard.uid;
                        ResolveEffectTarget(ability, caster, slotCard);
                    }
                }
                else
                {
                    if(ability.CanTarget(gameData, caster, slot))
                    {
                        ResolveEffectTarget(ability, caster, slot);
                    }
                }
            }
        }

        protected virtual void ResolveCardAbilityPlayers(AbilityData ability, Card caster)
        {
            //Get Player Targets based on conditions
            List<Player> targets = ability.GetPlayerTargets(gameData, caster, playerArray);

            foreach (Player target in targets)
            {
                ResolveEffectTarget(ability, caster, target);
            }
        }

        protected virtual void ResolveCardAbilityCards(AbilityData ability, Card caster)
        {
            //Get Cards Targets based on conditions
            List<Card> targets = ability.GetCardTargets(gameData, caster, cardArray);
            foreach (Card target in targets)
                ResolveEffectTarget(ability, caster, target);
        }

        protected virtual void ResolveCardAbilitySlots(AbilityData ability, Card caster)
        {
            //Get Slots Targets based on conditions
            List<Slot> targets = ability.GetSlotTargets(gameData, caster, slotArray);
            Debug.Log($"GetSlotTargets Filter {targets.Count} {ability.GetTitle()}");

            foreach (Slot target in targets)
                ResolveEffectTarget(ability, caster, target);
        }

        protected virtual void ResolveCardAbilityCardData(AbilityData ability, Card caster)
        {
            //Get CardData Targets based on conditions
            List<CardData> targets = ability.GetCardDataTargets(gameData, caster, cardDataArray);
            foreach (CardData target in targets)
                ResolveEffectTarget(ability, caster, target);
        }

        protected virtual void ResolveCardAbilityNoTarget(AbilityData ability, Card caster)
        {
            if (ability.target == AbilityTarget.None)
                ability.DoEffects(this, caster);
        }

        protected virtual void ResolveEffectTarget(AbilityData ability, Card caster, Player target)
        {
            ability.DoEffects(this, caster, target);
            onAbilityTargetPlayer?.Invoke(ability, caster, target);
        }

        protected virtual void ResolveEffectTarget(AbilityData ability, Card caster, Card target)
        {
            ability.DoEffects(this, caster, target);
            onAbilityTargetCard?.Invoke(ability, caster, target);
        }

        protected virtual void ResolveEffectTarget(AbilityData ability, Card caster, Slot target)
        {
            ability.DoEffects(this, caster, target);
            onAbilityTargetSlot?.Invoke(ability, caster, target);
        }

        protected virtual void ResolveEffectTarget(AbilityData ability, Card caster, CardData target)
        {
            ability.DoEffects(this, caster, target);
        }

        protected virtual void AfterAbilityResolved(AbilityData ability, Card caster)
        {
            Player player = gameData.GetPlayer(caster.playerID);
            
            //Pay cost
            if (ability.trigger == AbilityTrigger.Activate || ability.trigger == AbilityTrigger.None)
            {
                player.mana -= ability.manaCost;
                caster.exhausted = caster.exhausted || ability.exhaust;
            }
            //Recalculate and clear
            UpdateOngoing();
            CheckForWinner();
            
            //Chain ability
            if (ability.target != AbilityTarget.ChoiceSelector && gameData.state != GameState.GameEnded)
            {
                foreach (AbilityData chain in ability.chainAbilities)
                {
                    if(chain!=null)
                        TriggerCardAbility(chain, caster);
                }
            }
            
            onAbilityEnd?.Invoke(ability, caster);
            resolveQueue.ResolveAll(0.5f);
            RefreshData();
        }
        
        //经常调用此函数来更新受持续能力影响的状态/统计数据
        //It basically first reset the bonus to 0 (CleanOngoing) and then recalculate it to make sure it it still present
        //Only cards in hand and on board are updated in this way
        public virtual void UpdateOngoing()
        {
            Profiler.BeginSample("Update Ongoing");
            UpdateOngoingCards(); //Update status and stats
            UpdateOngoingKills(); //Kill cards with 0 HP
            Profiler.EndSample();
        }

        protected virtual void UpdateOngoingCards()
        {
            foreach (var player in gameData.players)
            {
                player.ClearOngoing();
                foreach (var c in player.cardsBoard)
                    c.ClearOngoing();
                foreach (var c in player.cardsHand)
                    c.ClearOngoing();
                foreach (var c in player.cardsEquip)
                    c.ClearOngoing();
            }
            
            foreach (var player in gameData.players)
            {
                player.ClearOngoing();
                UpdateOngoingAbilities(player, player.hero);  //Remove this line if hero is on the board
                foreach (var card in player.cardsBoard)
                    UpdateOngoingAbilities(player, card);
                foreach (var card in player.cardsHand)
                    UpdateOngoingAbilities(player, card);
            }
            
            //Stats bonus
            foreach (var player in gameData.players)
            {
                foreach (var card in player.cardsBoard)
                {
                    //Taunt effect
                    if (card.HasStatus(StatusType.Protection) && !card.HasStatus(StatusType.Stealth))
                    {
                        player.AddOngoingStatus(StatusType.Protected, 0);

                        foreach (var tcard in player.cardsBoard.Where(tcard => !tcard.HasStatus(StatusType.Protection) && !tcard.HasStatus(StatusType.Protected)))
                        {
                            tcard.AddOngoingStatus(StatusType.Protected, 0);
                        }
                    }

                    //Status bonus
                    foreach (CardStatus status in card.status)
                        AddOngoingStatusBonus(card, status);
                    foreach (CardStatus status in card.ongoingStatus)
                        AddOngoingStatusBonus(card, status);
                }

                foreach (var card in player.cardsHand)
                {
                    foreach (CardStatus status in card.status)
                        AddOngoingStatusBonus(card, status);
                    foreach (CardStatus status in card.ongoingStatus)
                        AddOngoingStatusBonus(card, status);
                }
            }
        }

        protected virtual void UpdateOngoingKills()
        {
            //Kill stuff with 0 hp
            foreach (var player in gameData.players)
            {
                for (int i = player.cardsBoard.Count - 1; i >= 0; i--)
                {
                    if (i < player.cardsBoard.Count)
                    {
                        Card card = player.cardsBoard[i];
                        if (card.GetHp() <= 0)
                            DiscardCard( card);
                    }
                }

                for (int i = player.cardsEquip.Count - 1; i >= 0; i--)
                {
                    if (i < player.cardsEquip.Count)
                    {
                        Card card = player.cardsEquip[i];
                        if (card.GetHp() <= 0)
                            DiscardCard(card);
                        Card before = player.GetBearerCard(card);
                        if (before == null)
                            DiscardCard(card);
                    }
                }
            }
            
            //Clear cards
            foreach (var card in cardsToClear)
                card.Clear();
            cardsToClear.Clear();
        }

        protected virtual void UpdateOngoingAbilities(Player player, Card card)
        {
            if (card == null || !card.CanDoAbilities())
                return;
            List<AbilityData> abilities = card.GetAbilities();
            for (int i = 0; i < abilities.Count; i++)
            {
                AbilityData ability = abilities[i];
                if (ability != null && ability.trigger == AbilityTrigger.Ongoing &&
                    ability.AreTriggerConditionsMet(gameData, card))
                {
                    if (ability.target == AbilityTarget.Self)
                    {
                        if(ability.AreTriggerConditionsMet(gameData, card,card))
                            ability.DoOngoingEffects(this, card,card);
                    }
                    
                    if (ability.target == AbilityTarget.PlayerSelf)
                    {
                        if (ability.AreTargetConditionsMet(gameData, card, player))
                        {
                            ability.DoOngoingEffects(this, card, player);
                        }
                    }
                    
                    if (ability.target == AbilityTarget.AllPlayers || ability.target == AbilityTarget.PlayerOpponent)
                    {
                        for (int tp = 0; tp < gameData.players.Length; tp++)
                        {
                            if (ability.target == AbilityTarget.AllPlayers || tp != player.id)
                            {
                                Player oplayer = gameData.players[tp];
                                if (ability.AreTargetConditionsMet(gameData, card, oplayer))
                                {
                                    ability.DoOngoingEffects(this, card, oplayer);
                                }
                            }
                        }
                    }
                    
                    if (ability.target == AbilityTarget.EquippedCard)
                    {
                        if (card.CardData.IsEquipment())
                        {
                            //Get bearer of the equipment
                            Card target = player.GetBearerCard(card);
                            if (target != null && ability.AreTargetConditionsMet(gameData, card, target))
                            {
                                ability.DoOngoingEffects(this, card, target);
                            }
                        }
                        else if (card.equippedUid != null)
                        {
                            //Get equipped card
                            Card target = gameData.GetCard(card.equippedUid);
                            if (target != null && ability.AreTargetConditionsMet(gameData, card, target))
                            {
                                ability.DoOngoingEffects(this, card, target);
                            }
                        }
                    }
                    
                    if (ability.target == AbilityTarget.AllCardsAllPiles || ability.target == AbilityTarget.AllCardsHand || ability.target == AbilityTarget.AllCardsBoard)
                    {
                        for (int tp = 0; tp < gameData.players.Length; tp++)
                        {
                            //Looping on all cards is very slow, since there are no ongoing effects that works out of board/hand we loop on those only
                            Player tplayer = gameData.players[tp];

                            //Hand Cards
                            if (ability.target == AbilityTarget.AllCardsAllPiles || ability.target == AbilityTarget.AllCardsHand)
                            {
                                for (int tc = 0; tc < tplayer.cardsHand.Count; tc++)
                                {
                                    Card tcard = tplayer.cardsHand[tc];
                                    if (ability.AreTargetConditionsMet(gameData, card, tcard))
                                    {
                                        ability.DoOngoingEffects(this, card, tcard);
                                    }
                                }
                            }

                            //Board Cards
                            if (ability.target == AbilityTarget.AllCardsAllPiles || ability.target == AbilityTarget.AllCardsBoard)
                            {
                                for (int tc = 0; tc < tplayer.cardsBoard.Count; tc++)
                                {
                                    Card tcard = tplayer.cardsBoard[tc];
                                    if (ability.AreTargetConditionsMet(gameData, card, tcard))
                                    {
                                        ability.DoOngoingEffects(this, card, tcard);
                                    }
                                }
                            }

                            //Equip Cards
                            if (ability.target == AbilityTarget.AllCardsAllPiles)
                            {
                                for (int tc = 0; tc < tplayer.cardsEquip.Count; tc++)
                                {
                                    Card tcard = tplayer.cardsEquip[tc];
                                    if (ability.AreTargetConditionsMet(gameData, card, tcard))
                                    {
                                        ability.DoOngoingEffects(this, card, tcard);
                                    }
                                }
                            }
                        }
                    }

                    
                }
            }
        }

        private void AddOngoingStatusBonus(Card card, CardStatus status)
        {
            if (status.type == StatusType.AddAttack)
                card.attackOngoing += status.value;
            if (status.type == StatusType.AddHP)
                card.hpOngoing += status.value;
            if (status.type == StatusType.AddManaCost)
                card.manaOngoing += status.value;
        }

        //---- Secrets ------------
        public virtual bool TriggerPlayerSecrets(Player player, AbilityTrigger secretTrigger)
        {
            for (int i = player.cardsSecret.Count - 1; i >= 0; i--)
            {
                Card card = player.cardsSecret[i];
                CardData icard = card.CardData;
                if (icard.type == CardType.Secret && !card.exhausted)
                {
                    if (card.AreAbilityConditionsMet(secretTrigger, gameData, card, card))
                    {
                        resolveQueue.AddSecret(secretTrigger, card, card, ResolveSecret);
                        resolveQueue.SetDelay(0.5f);
                        card.exhausted = true;
                        card.canAttackPlayer = false;

                        onSecretTrigger?.Invoke(card, card);
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual bool TriggerSecrets(AbilityTrigger secretTrigger, Card triggerCard)
        {
            if (triggerCard != null && triggerCard.HasStatus(StatusType.SpellImmunity))
                return false;
            for (int p = 0; p < gameData.players.Length; p++)
            {
                if (p != gameData.currentPlayer)
                {
                    Player otherPlayer = gameData.players[p];
                    for (int i = otherPlayer.cardsSecret.Count - 1; i >= 0; i--)
                    {
                        Card card = otherPlayer.cardsSecret[i];
                        CardData icard = card.CardData;
                        if (icard.type == CardType.Secret && !card.exhausted)
                        {
                            Card trigger = triggerCard ?? card;
                            if (card.AreAbilityConditionsMet(secretTrigger, gameData, card, trigger))
                            {
                                resolveQueue.AddSecret(secretTrigger, card, trigger, ResolveSecret);
                                resolveQueue.SetDelay(0.5f);
                                card.exhausted = true;
                                card.canAttackPlayer = false;
                                
                                onSecretTrigger?.Invoke(card, trigger);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
            
        }

        protected virtual void ResolveSecret(AbilityTrigger secretTrigger, Card secretCard, Card trigger)
        {
            CardData icard = secretCard.CardData;
            Player player = gameData.GetPlayer(secretCard.playerID);
            if (icard.type == CardType.Secret)
            {
                Player tplayer = gameData.GetPlayer(trigger.playerID);
                if(!isAiPredict)
                    tplayer.AddHistory(GameAction.SecretTriggered, secretCard, trigger);
                TriggerCardAbilityType(secretTrigger, secretCard, trigger);
                DiscardCard(secretCard);
                if (onSecretResolve != null)
                    onSecretResolve.Invoke(secretCard, trigger);
            }

        }
        
        //---- Resolve Selector -----
        public virtual void SelectCard(Card target)
        {
            if (gameData.selector == SelectorType.None)
                return;
            Card caster = gameData.GetCard(gameData.selectorCasterUid);
            AbilityData ability = AbilityData.Get(gameData.selectorAbilityId);
            
            if (caster == null || target == null || ability == null)
                return;

            if (gameData.selector == SelectorType.SelectTarget)
            {
                if (!ability.CanTarget(gameData, caster, target))
                    return;
                Player player = gameData.GetPlayer(caster.playerID);
                if(isAiPredict)
                    player.AddHistory(GameAction.CastAbility, caster, target);
                
                gameData.selector = SelectorType.None;
                gameData.lastTarget = target.uid;
                ResolveEffectTarget(ability, caster, target);
                AfterAbilityResolved(ability, caster);
                resolveQueue.ResolveAll();
            }
            
            if (gameData.selector == SelectorType.SelectorCard)
            {
                if (!ability.IsCardSelectionValid(gameData, caster, target, cardArray))
                    return; //Supports conditions and filters

                gameData.selector = SelectorType.None;
                gameData.lastTarget = target.uid;
                ResolveEffectTarget(ability, caster, target);
                AfterAbilityResolved(ability, caster);
                resolveQueue.ResolveAll();
            }
        }

        public virtual void SelectPlayer(Player target)
        {
            if (gameData.selector == SelectorType.None)
                return;
            
            Card caster = gameData.GetCard(gameData.selectorCasterUid);
            AbilityData ability = AbilityData.Get(gameData.selectorAbilityId);
            
            if (caster == null || target == null || ability == null)
                return;
            
            if (gameData.selector == SelectorType.SelectTarget)
            {
                if (!ability.CanTarget(gameData, caster, target))
                    return;
                Player player = gameData.GetPlayer(caster.playerID);
                if(isAiPredict)
                    player.AddHistory(GameAction.CastAbility, caster, target);
                
                gameData.selector = SelectorType.None;
                ResolveEffectTarget(ability, caster, target);
                AfterAbilityResolved(ability, caster);
                resolveQueue.ResolveAll();
            }
            
        }

        public virtual void SelectSlot(Slot target)
        {
            if (gameData.selector == SelectorType.None)
                return;
            
            Card caster = gameData.GetCard(gameData.selectorCasterUid);
            AbilityData ability = AbilityData.Get(gameData.selectorAbilityId);
            
            if (caster == null || ability == null || !target.IsValid())
                return;
            
            if (gameData.selector == SelectorType.SelectTarget)
            {
                if (!ability.CanTarget(gameData, caster, target))
                    return;
                Player player = gameData.GetPlayer(caster.playerID);
                if(isAiPredict)
                    player.AddHistory(GameAction.CastAbility, caster, ability,target);
                
                gameData.selector = SelectorType.None;
                ResolveEffectTarget(ability, caster, target);
                AfterAbilityResolved(ability, caster);
                resolveQueue.ResolveAll();
            }
        }

        public virtual void SelectChoice(int choice)
        {
            if (gameData.selector == SelectorType.None)
                return;
            
            Card caster = gameData.GetCard(gameData.selectorCasterUid);
            AbilityData ability = AbilityData.Get(gameData.selectorAbilityId);
            
            if (caster == null || ability == null || choice<0)
                return;
            
            if (gameData.selector == SelectorType.SelectorChoice && ability.target == AbilityTarget.ChoiceSelector)
            {
                if (choice >= 0 && choice < ability.chainAbilities.Length)
                {
                    AbilityData achoice = ability.chainAbilities[choice];
                    if (achoice != null && gameData.CanSelectAbility(caster, achoice))
                    {
                        gameData.selector = SelectorType.None;
                        AfterAbilityResolved(ability, caster);
                        ResolveCardAbility(achoice, caster, caster);
                        resolveQueue.ResolveAll();
                    }
                }
            }
        }

        public virtual void SelectCost(int selectCost)
        {
            if (gameData.selector == SelectorType.None)
                return;
            
            Player player = gameData.GetPlayer(gameData.selectorPlayerId);
            Card caster = gameData.GetCard(gameData.selectorCasterUid);
            
            if (player == null || caster == null || selectCost<0)
                return;

            if (gameData.selector == SelectorType.SelectorCost)
            {
                if (selectCost is >= 0 and < 10 && selectCost <= player.Mana)
                {
                    gameData.selector = SelectorType.None;
                    gameData.selectedValue = selectCost;
                    player.mana -= selectCost;
                    RefreshData();
                    
                    TriggerSecrets(AbilityTrigger.OnPlayOther, caster);
                    TriggerCardAbilityType(AbilityTrigger.OnPlay, caster);
                    TriggerOtherCardsAbilityType(AbilityTrigger.OnPlayOther, caster);
                    resolveQueue.ResolveAll();
                }
            }
        }

        public virtual void CancelSelection()
        {
            if (gameData.selector != SelectorType.None)
            {
                //Return card to hand if was selecting cost
                if (gameData.selector == SelectorType.SelectorCost)
                    CancelPlayCard();

                //End selection
                gameData.selector = SelectorType.None;
                RefreshData();
            }
        }

        public void CancelPlayCard()
        {
            Card card = gameData.GetCard(gameData.selectorCasterUid);
            if (card != null)
            {
                Player player = gameData.GetPlayer(card.playerID);
                if (card.CardData.IsDynamicManaCost())
                    player.mana += gameData.selectedValue;
                else
                    player.mana += card.CardData.cost;
                
                player.RemoveCardFromAllGroups(card);
                player.AddCard(player.cardsHand, card);
                card.Clear();
            }
        }

        public virtual void Mulligan(Player player, string[] cards)
        {
            if (gameData.phase == GamePhase.Mulligan && !player.ready)
            {
                int count = 0;
                List<Card> removeList = new List<Card>();
                foreach (Card card in player.cardsHand)
                {
                    if (cards.Contains(card.uid))
                    {
                        removeList.Add(card);
                        count++;
                    }
                }

                foreach (Card card in removeList)
                {
                    player.RemoveCardFromAllGroups(card);
                    player.cardsDiscard.Add(card);
                }
                
                player.ready = true;
                DrawCard(player, count);
                RefreshData();

                if (gameData.AreAllPlayersReady())
                {
                    StartTurn();
                }
            }
        }
        
        //-----Trigger Selector-----
        protected virtual void GoToSelectTarget(AbilityData ability, Card caster)
        {
            gameData.selector = SelectorType.SelectTarget;
            gameData.selectorPlayerId = caster.playerID;
            gameData.selectorAbilityId = ability.id;
            gameData.selectorCasterUid = caster.uid;
            RefreshData();
        }

        protected virtual void GoToSelectorCard(AbilityData ability, Card caster)
        {
            gameData.selector = SelectorType.SelectorCard;
            gameData.selectorPlayerId = caster.playerID;
            gameData.selectorAbilityId = ability.id;
            gameData.selectorCasterUid = caster.uid;
            RefreshData();
        }

        protected virtual void GoToSelectorChoice(AbilityData ability, Card caster)
        {
            gameData.selector = SelectorType.SelectorChoice;
            gameData.selectorPlayerId = caster.playerID;
            gameData.selectorAbilityId = ability.id;
            gameData.selectorCasterUid = caster.uid;
            RefreshData();
        }

        protected virtual void GoToSelectorCost(Card caster)
        {
            gameData.selector = SelectorType.SelectorCost;
            gameData.selectorPlayerId = caster.playerID;
            gameData.selectorCasterUid = caster.uid;
            gameData.selectedValue = 0;
            gameData.selectorAbilityId = "";
            RefreshData();
        }

        protected virtual void GoToMulligan()
        {
            gameData.phase = GamePhase.Mulligan;
            gameData.turnTimer = GamePlayData.Get().turnDuration;
            foreach (Player player in gameData.players)
            {
                player.ready = false;
            }
            RefreshData();
        }
        

        private void RefreshData()
        {
            onRefresh?.Invoke();
        }

        public virtual void ClearResolve()
        {
            resolveQueue.Clear();
        }

        public virtual bool IsResolving()
        {
            return resolveQueue.IsResolving();
        }

        public virtual bool IsGameStarted()
        {
            return gameData.HasStarted();
        }

        public virtual bool IsGameEnded()
        {
            return gameData.HasEnded();
        }

        public virtual Game GetGameData()
        {
            return gameData;
        }

        public System.Random GetRandom()
        {
            return rand;
        }
        
        public Game GameData => gameData;
        public ResolveQueue ResolveQueue => resolveQueue;
    }
}