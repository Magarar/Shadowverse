using System;
using System.Collections.Generic;
using Data;
using UnityEngine.Serialization;

namespace GameLogic
{
    //Contains all gameplay state data that is sync across network
    [Serializable]
    public class Game
    {
        public string gameUID;
        public GameSetting settings;
        
        //GameState
        public int firstPlayer = 0;
        public int currentPlayer = 0;
        public int turnCount = 0;
        public float turnTimer = 0;
        
        public GameState state = GameState.Connecting;
        public GamePhase phase = GamePhase.None;
        
        //Players
        public Player[]  players;
        
        //Selector
        public SelectorType selector = SelectorType.None;
        public int selectorPlayerId = 0;
        public string selectorAbilityId;
        [FormerlySerializedAs("selectorCasterId")] public string selectorCasterUid;
        
        //Other reference values
        public string lastPlayed;
        public string lastDestroyed;
        public string lastTarget;
        public string lastSummoned;
        public string abilityTrigger;
        public int rolledValue;
        public int selectedValue;
        
        //Other reference arrays 
        public HashSet<string> abilityPlayed = new HashSet<string>();
        public HashSet<string> cardsAttacked = new HashSet<string>();

        public Game()
        {
            
        }
        
        public Game(string uid, int nbPlayers)
        {
            this.gameUID = uid;
            players = new Player[nbPlayers];
            for (int i = 0; i < nbPlayers; i++)
                players[i] = new Player(i);
            settings = GameSetting.Default;
        }

        public virtual bool AreAllPlayersReady()
        {
            int ready = 0;
            foreach (Player p in players)
            {
                if(p.IsReady())
                    ready++;
            }
            return ready >= settings.nbPlayers;
        }

        public virtual bool AreAllPlayersConnected()
        {
            int ready = 0;
            foreach (Player player in players)
            {
                if (player.IsConnected())
                    ready++;
            }
            return ready >= settings.nbPlayers;
        }
        
        //Check if its player's turn
        public virtual bool IsPlayerTurn(Player player)
        {
            return IsPlayerActionTurn(player) || IsPlayerSelectorTurn(player);
        }

        public virtual bool IsPlayerActionTurn(Player player)
        {
            return player!=null&&currentPlayer == player.id&&state == GameState.Play&&phase == GamePhase.Main&&selector==SelectorType.None;
        }
        
        public virtual bool IsPlayerSelectorTurn(Player player)
        {
            return player != null && selectorPlayerId == player.id 
                                  && state == GameState.Play && phase == GamePhase.Main && selector != SelectorType.None;
        }

        public virtual bool IsPlayerMulliganTurn(Player player)
        {
            return phase==GamePhase.Mulligan&&!player.ready;
        }
        
        //Check if a card is allowed to be played on slot
        public virtual bool CanPlayCard(Card card, Slot slot, bool skipCost = false)
        {
            if(card==null)
                return false;
            
            Player player = GetPlayer(card.playerID);
            if(!skipCost&&!player.CanPayMana(card))
                return false;//Cant pay Mana
            if(!player.HasCard(player.cardsHand,card))
                return false;//Cant pay Mana
            if(player.isAI&&card.CardData.IsDynamicManaCost()&&player.Mana==0)
                return false;// AI cant play X-cost card at 0 cost

            if (card.CardData.IsBoardCard())
            {
                if(!slot.IsValid()||IsCardOnSlot(slot))
                    return false;//Slot already occupied
                if (Slot.GetP(card.playerID) != slot.p)
                    return false; //Cant play on opponent side
                return true;
            }

            if (card.CardData.IsEquipment())
            {
                if(!slot.IsValid())
                    return false;
                Card target = GetSlotCard(slot);
                if(target==null||target.CardData.type!=CardType.Character||target.playerID!=card.playerID)
                    return false;//Target must be an allied character
                return true;
            }

            if (card.CardData.IsRequireTargetSpell())
            {
                return IsPlayTargetValid(card, slot); //Check play target on slot
            }
            
            if (card.CardData.type == CardType.Spell)
            {
                return CanAnyPlayAbilityTrigger(card); //Check if spell will have abilities
            }
            
            return true;
            
        }

        public virtual bool CanMoveCard(Card card, Slot slot, bool skipCost = false)
        {
            if (card == null || !slot.IsValid())
                return false;

            if (!IsOnBoard(card))
                return false; //Only cards in play can move

            if (!card.CanMove(skipCost))
                return false; //Card cant move

            if (Slot.GetP(card.playerID) != slot.p)
                return false; //Card played wrong side

            if (card.slot == slot)
                return false; //Cant move to same slot

            Card slotCard = GetSlotCard(slot);
            if (slotCard != null)
                return false; //Already a card there

            return false;
        }
        
        //Check if a card is allowed to attack a player
        public virtual bool CanAttackTarget(Card attacker, Player target, bool skipCost = false)
        {
            if(attacker == null || target == null)
                return false;
            if (!attacker.CanAttack(skipCost))
                return false; //Card cant attack
            if(!attacker.canAttackPlayer)
                return false;
            if(attacker.playerID==target.id)
                return false; //Cant attack same player
            if (!IsOnBoard(attacker) || !attacker.CardData.IsCharacter())
                return false; //Cards not on board
            if (target.HasStatus(StatusType.Protected) && !attacker.HasStatus(StatusType.Flying))
                return false; //Protected by taunt
            return true;
            
        }
        
        //Check if a card is allowed to attack another one
        public virtual bool CanAttackTarget(Card attacker, Card target, bool skipCost = false)
        {
            if (attacker == null || target == null)
                return false;
            if(!attacker.CanAttack(skipCost))
                return false;
            if (attacker.playerID == target.playerID)
                return false; //Cant attack same player
            if (!IsOnBoard(attacker) || !IsOnBoard(target))
                return false; //Cards not on board
            if (!attacker.CardData.IsCharacter() || !target.CardData.IsBoardCard())
                return false; //Only character can attack
            if (target.HasStatus(StatusType.Stealth))
                return false; //Stealth cant be attacked
            if (target.HasStatus(StatusType.Protected) && !attacker.HasStatus(StatusType.Flying))
                return false; //Protected by adjacent card
            return true;
        }

        public virtual bool CanCastAbility(Card card, AbilityData ability)
        {
            if (ability == null || card == null || !card.CanDoActivatedAbilities())
                return false; //This card cant cast
            if (ability.trigger != AbilityTrigger.Activate)
                return false; //Not an activated ability
            Player player = GetPlayer(card.playerID);
            if (!player.CanPayAbility(card, ability))
                return false; //Cant pay for ability
            if (!ability.AreTriggerConditionsMet(this, card))
                return false; //Conditions not met
            return true;
            
        }
        
        //For choice selector
        public virtual bool CanSelectAbility(Card card, AbilityData ability)
        {
            if (ability == null || card == null || !card.CanDoAbilities())
                return false; //This card cant cast
            Player player = GetPlayer(card.playerID);
            if(!player.CanPayAbility(card, ability))
                return false;//Cant pay for ability
            if (!ability.AreTriggerConditionsMet(this, card))
                return false; //Conditions not met
            return true;
            
        }

        public virtual bool CanAnyPlayAbilityTrigger(Card card)
        {
            if (card == null)
                return false;
            if (card.CardData.IsDynamicManaCost())
                return true; //Cost not decided so condition could be false
            foreach (AbilityData ability in card.GetAbilities())
            {
                if (ability.trigger == AbilityTrigger.OnPlay && ability.AreTriggerConditionsMet(this, card))
                    return true;
            }
            return false;
        }
        
        //检查玩家的游戏目标是否有效，当一个法术需要直接拖动到另一张牌上时，游戏目标就是目标
        public virtual bool IsPlayTargetValid(Card caster, Player target)
        {
            if (caster == null || target == null)
                return false;
            foreach (AbilityData ability in caster.GetAbilities())
            {
                if (ability && ability.trigger == AbilityTrigger.OnPlay && ability.target == AbilityTarget.PlayTarget)
                {
                    if (!ability.CanTarget(this, caster, target))
                        return false;
                }
            }
            return true;
        }
        
        //Check if Card play target is valid, play target is the target when a spell requires to drag directly onto another card
        public virtual bool IsPlayTargetValid(Card caster, Card target)
        {
            if (caster == null || target == null)
                return false;
            foreach (AbilityData ability in caster.GetAbilities())
            {
                if (ability && ability.trigger == AbilityTrigger.OnPlay && ability.target == AbilityTarget.PlayTarget)
                {
                    if (!ability.CanTarget(this, caster, target))
                        return false;
                }
            }
            return true;
        }
        
        //Check if Slot play target is valid, play target is the target when a spell requires to drag directly onto another card
        public virtual bool IsPlayTargetValid(Card caster, Slot target)
        {
            if (caster == null)
                return false;
            
            if (target.IsPlayerSlot())
                return IsPlayTargetValid(caster, GetPlayer(target.p)); //Slot 0,0, means we are targeting a player
            
            Card slotCard = GetSlotCard(target);
            if(slotCard != null)
                return IsPlayTargetValid(caster, slotCard);//Slot has card, check play target on that card

            foreach (AbilityData ability in caster.GetAbilities())
            {
                if (ability && ability.trigger == AbilityTrigger.OnPlay && ability.target == AbilityTarget.PlayTarget)
                {
                    if (!ability.CanTarget(this, caster, target))
                        return false;
                }
            }
            return true;
            
        }
        
        public Player GetPlayer(int id)
        {
            if (id >= 0 && id < players.Length)
            {
                return players[id];
            }
            return null;
        }

        public Player GetActivePlayer()
        {
            return GetPlayer(currentPlayer);
        }
        
        public Player GetOpponentPlayer(int id)
        {
            int oid = id == 0 ? 1 : 0;
            return GetPlayer(oid);
        }
        
        public Card GetCard(string cardUid)
        {
            foreach (var player in players)
            {
                Card acrad = player.GetCard(cardUid);
                if (acrad != null)
                    return acrad;
            }
            return null;
        }
        
        public Article GetArticle(string articleUid)
        {
            foreach (var player in players)
            {
                Article article = player.GetArticle(articleUid);
                if (article != null)
                    return article;
            }
            return null;
        }

        public Card GetBoardCard(string cardUid)
        {
            foreach (var player in players)
            {
                foreach (Card card in player.cardsBoard)
                {
                    if(card!=null&&card.uid==cardUid)
                        return card;
                }
            }
            return null;
        }

        public Card GetEquipCard(string cardUid)
        {
            foreach (var player in players)
            {
                foreach (Card card in player.cardsEquip)
                {
                    if(card!=null&&card.uid==cardUid)
                        return card;
                }
            }
            return null;
        }

        public Card GetHandCard(string cardUid)
        {
            foreach (var player in players)
            {
                foreach (Card card in player.cardsHand)
                {
                    if(card!=null&&card.uid==cardUid)
                        return card;
                }
            }
            return null;
        }

        public Card GetDeckCard(string cardUid)
        {
            foreach (var player in players)
            {
                foreach (Card card in player.cardsDeck)
                {
                    if(card!=null&&card.uid==cardUid)
                        return card;
                }
            }
            return null;
        }
        
        public Card GetDiscardCard(string cardUid)
        {
            foreach (Player player in players)
            {
                foreach (Card card in player.cardsDiscard)
                {
                    if (card != null && card.uid == cardUid)
                        return card;
                }
            }
            return null;
        }

        public Card GetSecretCard(string card_uid)
        {
            foreach (Player player in players)
            {
                foreach (Card card in player.cardsSecret)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }
            return null;
        }

        public Card GetTempCard(string card_uid)
        {
            foreach (Player player in players)
            {
                foreach (Card card in player.cardsTemp)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }
            return null;
        }

        public Card GetSlotCard(Slot slot)
        {
            foreach (Player player in players)
            {
                foreach (Card card in player.cardsBoard)
                {
                    if (card != null && card.slot == slot)
                        return card;
                }
            }
            return null;
        }
        
        public virtual Player GetRandomPlayer(Random rand)
        {
            Player player = GetPlayer(rand.NextDouble() < 0.5 ? 1 : 0);
            return player;
        }
        
        public virtual Card GetRandomBoardCard(Random rand)
        {
            Player player = GetRandomPlayer(rand);
            return player.GetRandomCard(player.cardsBoard, rand);
        }
        
        public virtual Slot GetRandomSlot(Random rand)
        {
            Player player = GetRandomPlayer(rand);
            return player.GetRandomSlot(rand);
        }
        
        public bool IsInHand(Card card)
        {
            return card != null && GetHandCard(card.uid) != null;
        }
        
        public bool IsOnBoard(Card card)
        {
            return card != null && GetBoardCard(card.uid) != null;
        }

        public bool IsEquipped(Card card)
        {
            return card != null && GetEquipCard(card.uid) != null;
        }
        
        public bool IsInDeck(Card card)
        {
            return card != null && GetDeckCard(card.uid) != null;
        }
        
        public bool IsInDiscard(Card card)
        {
            return card != null && GetDiscardCard(card.uid) != null;
        }

        public bool IsInSecret(Card card)
        {
            return card != null && GetSecretCard(card.uid) != null;
        }

        public bool IsInTemp(Card card)
        {
            return card != null && GetTempCard(card.uid) != null;
        }
        
        public bool IsCardOnSlot(Slot slot)
        {
            return GetSlotCard(slot) != null;
        }
        
        public bool HasStarted()
        {
            return state != GameState.Connecting;
        }
        
        public bool HasEnded()
        {
            return state == GameState.GameEnded;
        }
        
        //Same as clone, but also instantiates the variable (much slower)
        public static Game CloneNew(Game source)
        {
            Game game = new Game();
            Clone(source, game);
            return game;
        }
        
        //Clone all variables into another var, used mostly by the AI when building a prediction tree
        public static void Clone(Game source, Game dest)
        {
            dest.gameUID = source.gameUID;
            dest.settings = source.settings;
            
            dest.firstPlayer = source.firstPlayer;
            dest.currentPlayer = source.currentPlayer;
            dest.turnCount = source.turnCount;
            dest.turnTimer = source.turnTimer;
            
            dest.state = source.state;
            dest.phase = source.phase;

            if (dest.players == null)
            {
                dest.players = new Player[source.players.Length];
                for(int i=0; i< source.players.Length; i++)
                    dest.players[i] = new Player(i);
            }
            
            for (int i = 0; i < source.players.Length; i++)
                Player.Clone(source.players[i], dest.players[i]);
            
            dest.selector = source.selector;
            dest.selectorPlayerId = source.selectorPlayerId;
            dest.selectorAbilityId = source.selectorAbilityId;
            dest.selectorCasterUid = source.selectorCasterUid;
            
            dest.lastPlayed = source.lastPlayed;
            dest.lastDestroyed = source.lastDestroyed;
            dest.lastTarget = source.lastTarget;
            dest.lastSummoned = source.lastSummoned;
            dest.abilityTrigger = source.abilityTrigger;
            dest.rolledValue = source.rolledValue;
            dest.selectedValue = source.selectedValue;

            CloneHash(source.abilityPlayed, dest.abilityPlayed);
            CloneHash(source.cardsAttacked, dest.cardsAttacked);

        }

        private static void CloneHash(HashSet<string> source, HashSet<string> dest)
        {
            dest.Clear();
            foreach (string s in source)
                dest.Add(s);
        }
    }
    
    [Serializable]
    public enum GameState
    {
        Connecting = 0, //Players are not connected
        Play = 20,      //Game is being played
        GameEnded = 99,
    }

    [Serializable]
    public enum GamePhase
    {
        None = 0,
        Mulligan = 5,
        StartTurn = 10, //Start of turn resolution
        Main = 20,      //Main play phase
        EndTurn = 30,   //End of turn resolutions
    }

    [Serializable]
    public enum SelectorType
    {
        None = 0,
        SelectTarget = 10,
        SelectorCard = 20,
        SelectorChoice = 30,
        SelectorCost = 40,
    }
}