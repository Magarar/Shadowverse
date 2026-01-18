using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace GameLogic
{
    //Represent the current state of a player during the game (data only)
    [Serializable]
    public class Player
    {
        public string username;
        public int id;
        public string avatar;
        public string cardBack;
        public string deck;
        
        public bool isAI;
        public int aiLevel;
        
        public bool connected = false;//Connected to server and game
        public bool ready = false;//Sent all player data, ready to play
        public bool canUseExtraMana;

        public int hp;
        public int hpMax;
        public int mana;
        public int manaOngoing;
        public int Mana => mana + manaOngoing;

        public int manaMax;
        public int killCount;
        
        //evolutionaryPoint
        public int evolutionPoint;
        public int evolutionPointMax;
        public int superEvolutionPoint;
        public int superEvolutionPointMax;
        public bool enableEvolution;
        [FormerlySerializedAs("enableSuperEvolutionPoint")] public bool enableSuperEvolution;
        public int enableEvolutionPointTurn;
        public int enableSuperEvolutionPointTurn;
        public bool canUseEvolution;
        
        public Dictionary<string,Card> cardsAll = new();
        public Dictionary<string,Article> articlesAll = new();
        public Card hero;
        
        public List<Card> cardsEquip = new();
        public List<Card> cardsDeck= new();
        public List<Card> cardsTemp= new();
        public List<Card> cardsSecret= new();
        public List<Card> cardsDiscard= new();
        public List<Card> cardsBoard= new();
        public List<Card> cardsHand= new();
        
        public List<CardTrait> traits = new List<CardTrait>(); //Current persistant traits the cards has
        public List<CardTrait> ongoingTraits = new List<CardTrait>();
        
        public List<CardStatus> status = new List<CardStatus>();
        public List<CardStatus> ongoingStatus = new List<CardStatus>();
        
        public List<Article> articles = new List<Article>();
        
        public List<ActionHistory> historyList = new List<ActionHistory>();

        public Player(int id)
        {
            this.id = id;
        }

        public bool IsReady()
        {
            return ready && cardsAll.Count > 0;
        }

        public bool IsConnected()
        {
            return connected || isAI;
        }

        public virtual void ClearOngoing()
        {
            ongoingStatus.Clear();
            ongoingTraits.Clear();
        }
        
        //Articles
        public void AddArticle(List<Article> articleList, Article article)
        {
            articleList.Add(article);
        }

        public void RemoveArticle(List<Article> articleList, Article article)
        {
            articleList.Remove(article);
        }

        public virtual void RemoveArticleFromAllGroups(Article article)
        {
            articles.Remove(article);
        }
        
        public Article GetArticle(string uid)
        {
            if (uid != null)
            {
                bool valid = articlesAll.TryGetValue(uid, out Article article);
                if (valid)
                    return article;
            }
            return null;
        }

        public virtual Article GetRandomCard(List<Article> articlesList, Random rand)
        {
            if (articlesList.Count > 0)
                return articlesList[rand.Next(0, articlesList.Count)];
            return null;
        }

        public bool HasArticle(List<Article> articlesList, Article article)
        {
            return articlesList.Contains(article);
        }

        public Article GetBoardArticle(string uid)
        {
            return articles.FirstOrDefault(article => article.uid == uid);
        }
        
        //---- Cards ---------
        public void AddCard(List<Card> cardList, Card card)
        {
            cardList.Add(card);
        }

        public void RemoveCard(List<Card> cardList, Card card)
        {
            cardList.Remove(card);
        }

        public virtual void RemoveCardFromAllGroups(Card card)
        {
            cardsDeck.Remove(card);
            cardsDiscard.Remove(card);
            cardsHand.Remove(card);
            cardsSecret.Remove(card);
            cardsBoard.Remove(card);
            cardsTemp.Remove(card);
            cardsEquip.Remove(card);
            UnequipFromAllCards(card);
            
        }

        public virtual void UnequipFromAllCards(Card card)
        {
            foreach (var c in cardsBoard)
            {
                if (c.equippedUid == card.uid)
                {
                    c.equippedUid = null;
                }
            }
        }
        
        public virtual Card GetRandomCard(List<Card> cardList, Random rand)
        {
            if (cardList.Count > 0)
                return cardList[rand.Next(0, cardList.Count)];
            return null;
        }

        public bool HasCard(List<Card> cardList, Card card)
        {
            return cardList.Contains(card);
        }

        public Card GetHandCard(string uid)
        {
            return cardsHand.FirstOrDefault(card => card.uid == uid);
        }

        public Card GetBoardCard(string uid)
        {
            return cardsBoard.FirstOrDefault(card => card.uid == uid);
        }

        public Card GetDeckCard(string uid)
        {
            return cardsDeck.FirstOrDefault(card => card.uid == uid);
        }

        public Card GetEquipCard(string uid)
        {
            return cardsEquip.FirstOrDefault(card => card.uid == uid);
        }

        public Card GetDiscardCard(string uid)
        {
            return cardsDiscard.FirstOrDefault(card => card.uid == uid);
        }
        
        public Card GetBearerCard(Card equipment)
        {
            return cardsBoard.FirstOrDefault(card => card != null && card.equippedUid == equipment.uid);
        }
        
        public Card GetSlotCard(Slot slot)
        {
            return cardsBoard.FirstOrDefault(card => card != null && card.slot == slot);
        }
        
        public Card GetCard(string uid)
        {
            if (uid != null)
            {
                bool valid = cardsAll.TryGetValue(uid, out Card card);
                if (valid)
                    return card;
            }
            return null;
        }

        public bool IsOnBoard(Card card)
        {
            return card != null && GetBoardCard(card.uid) != null;
        }
        
        //---- Slots ---------
        public Slot GetRandomSlot(Random rand)
        {
            return Slot.GetRandom(id, rand);
        }
        
        public virtual Slot GetRandomEmptySlot(System.Random rand, List<Slot> listMem = null)
        {
            List<Slot> valid = GetEmptySlots(listMem);
            if (valid.Count > 0)
                return valid[rand.Next(0, valid.Count)];
            return Slot.None;
        }
        
        public virtual Slot GetRandomOccupiedSlot(System.Random rand, List<Slot> listMem = null)
        {
            List<Slot> valid = GetOccupiedSlots(listMem);
            if (valid.Count > 0)
                return valid[rand.Next(0, valid.Count)];
            return Slot.None;
        }

        private List<Slot> GetEmptySlots(List<Slot> listMem)
        {
            List<Slot> vaild = listMem ?? new List<Slot>();
            foreach (Slot slot in Slot.GetAll(id))
            {
                Card slotCard = GetSlotCard(slot);
                if (slotCard == null)
                    vaild.Add(slot);
            }
            return vaild;
        }
        
        public List<Slot> GetOccupiedSlots(List<Slot> listMem = null)
        {
            List<Slot> valid = listMem ?? new List<Slot>();
            foreach (Slot slot in Slot.GetAll(id))
            {
                Card slot_card = GetSlotCard(slot);
                if (slot_card != null)
                    valid.Add(slot);
            }
            return valid;
        }
        
        //------ Custom Traits/Stats ---------
        public void SetTrait(string id, int value)
        {
            CardTrait trait = GetTrait(id);
            if (trait != null)
            {
                trait.value = value;
            }
            else
            {
                trait = new CardTrait(id, value);
                traits.Add(trait);
            }
        }
        
        public void AddTrait(string id, int value)
        {
            CardTrait trait = GetTrait(id);
            if (trait != null)
                trait.value += value;
            else
                SetTrait(id, value);
        }
        
        public void AddOngoingTrait(string id, int value)
        {
            CardTrait trait = GetOngoingTrait(id);
            if (trait != null)
            {
                trait.value += value;
            }
            else
            {
                trait = new CardTrait(id, value);
                ongoingTraits.Add(trait);
            }
        }
        
        public void RemoveTrait(string id)
        {
            for (int i = traits.Count - 1; i >= 0; i--)
            {
                if (traits[i].id == id)
                    traits.RemoveAt(i);
            }
        }
        
        public CardTrait GetTrait(string id)
        {
            return traits.FirstOrDefault(trait => trait.id == id);
        }

        private CardTrait GetOngoingTrait(string id)
        {
            return ongoingTraits.FirstOrDefault(trait => trait.id == id);
        }
        
        public List<CardTrait> GetAllTraits()
        {
            List<CardTrait> allTraits = new List<CardTrait>();
            allTraits.AddRange(traits);
            allTraits.AddRange(ongoingTraits);
            return allTraits;
        }
        
        public int GetTraitValue(TraitData trait)
        {
            if (trait != null)
                return GetTraitValue(trait.id);
            return 0;
        }

        public int GetTraitValue(string id)
        {
            int val = 0;
            CardTrait stat1 = GetTrait(id);
            CardTrait stat2 = GetOngoingTrait(id);
            if (stat1 != null)
                val += stat1.value;
            if (stat2 != null)
                val += stat2.value;
            return val;
        }
        
        public bool HasTrait(TraitData trait)
        {
            if (trait != null)
                return HasTrait(trait.id);
            return false;
        }
        
        public bool HasTrait(string id)
        {
            return traits.Any(trait => trait.id == id);
        }
        
        //---- Status ---------

        public void AddStatus(StatusData status, int value, int duration)
        {
            if(status!=null)
                AddStatus(status.effect, value, duration);
        }
        
        public void AddOngoingStatus(StatusData status, int value)
        {
            if (status != null)
                AddOngoingStatus(status.effect, value);
        }

        private void AddStatus(StatusType effect, int value, int duration)
        {
            if (effect != StatusType.None)
            {
                CardStatus status = GetStatus(effect);
                if (status == null)
                {
                    status = new CardStatus(effect, value, duration);
                    this.status.Add(status);
                }
                else
                {
                    status.value += value;
                    status.duration = Mathf.Max(status.duration, duration);
                    status.permanent = status.permanent || duration == 0;
                }
            }
        }
        
        public void AddOngoingStatus(StatusType effect, int value)
        {
            if (effect != StatusType.None)
            {
                CardStatus status = GetOngoingStatus(effect);
                if (status == null)
                {
                    status = new CardStatus(effect, value, 0);
                    ongoingStatus.Add(status);
                }
                else
                {
                    status.value += value;
                }
            }
        }
        
        public void RemoveStatus(StatusType effect)
        {
            for (int i = status.Count - 1; i >= 0; i--)
            {
                if (status[i].type == effect)
                    status.RemoveAt(i);
            }
        }

        private CardStatus GetOngoingStatus(StatusType effect)
        {
            return ongoingStatus.FirstOrDefault(status => status.type == effect);
        }

        private CardStatus GetStatus(StatusType effect)
        {
            return status.FirstOrDefault(status => status.type == effect);
        }
        
        public List<CardStatus> GetAllStatus()
        {
            List<CardStatus> allStatus = new List<CardStatus>();
            allStatus.AddRange(status);
            allStatus.AddRange(ongoingStatus);
            return allStatus;
        }
        
        public bool HasStatus(StatusType effect)
        {
            return GetStatus(effect) != null || GetOngoingStatus(effect) != null;
        }
        
        public virtual int GetStatusValue(StatusType type)
        {
            CardStatus status1 = GetStatus(type);
            CardStatus status2 = GetOngoingStatus(type);
            int v1 = status1?.value ?? 0;
            int v2 = status2?.value ?? 0;
            return v1 + v2;
        }
        
        public virtual void ReduceStatusDurations()
        {
            for (int i = status.Count - 1; i >= 0; i--)
            {
                if (!status[i].permanent)
                {
                    status[i].duration -= 1;
                    if (status[i].duration <= 0)
                        status.RemoveAt(i);
                }
            }
        }
        
        //---- History ---------
        public void AddHistory(ushort type, Card card)
        {
            ActionHistory order = new ActionHistory
            {
                type = type,
                cardId = card.cardId,
                cardUid = card.uid
            };
            historyList.Add(order);
        }
        
        public void AddHistory(ushort type, Card card, Card target)
        {
            ActionHistory order = new ActionHistory
            {
                type = type,
                cardId = card.cardId,
                cardUid = card.uid,
                targetUid = target.uid
            };
            historyList.Add(order);
        }

        public void AddHistory(ushort type, Card card, Player target)
        {
            ActionHistory order = new ActionHistory
            {
                type = type,
                cardId = card.cardId,
                cardUid = card.uid,
                targetId = target.id
            };
        }

        public void AddHistory(ushort type, Card card, AbilityData ability)
        {
            ActionHistory order = new ActionHistory
            {
                type = type,
                cardId = card.cardId,
                cardUid = card.uid,
                abilityId = ability.id
            };
            historyList.Add(order);
        }
        
        public void AddHistory(ushort type, Card card, AbilityData ability, Card target)
        {
            ActionHistory order = new ActionHistory
            {
                type = type,
                cardId = card.cardId,
                cardUid = card.uid,
                abilityId = ability.id,
                targetUid = target.uid
            };
            historyList.Add(order);
        }
        
        public void AddHistory(ushort type, Card card, AbilityData ability, Player target)
        {
            ActionHistory order = new ActionHistory
            {
                type = type,
                cardId = card.cardId,
                cardUid = card.uid,
                abilityId = ability.id,
                targetId = target.id
            };
            historyList.Add(order);
        }
        
        public void AddHistory(ushort type, Card card, AbilityData ability, Slot target)
        {
            ActionHistory order = new ActionHistory();
            order.type = type;
            order.cardId = card.cardId;
            order.cardUid = card.uid;
            order.abilityId = ability.id;
            order.slot = target;
            historyList.Add(order);
        }

        //---- Action Check ---------
        public bool CanPayMana(Card card)
        {
            if (card.CardData.IsDynamicManaCost())
                return true;
            return Mana >= card.GetMana();
        }
        
        public virtual void PayMana(Card card)
        {
            if (!card.CardData.IsDynamicManaCost())
            {
                int cost = card.GetMana();
                if(mana >= cost)
                    mana -= cost;
                else
                {
                    int remainingCost = cost - mana;
                    mana = 0;
                    if (manaOngoing >= remainingCost)
                    {
                        manaOngoing -= remainingCost;
                    }
                    else
                    {
                        manaOngoing = 0;
                    }

                    SetCanUseExtraMana(false);
                }
            }
               
        }

        public virtual bool CanUseEvolution()
        {
            //return true;
            return canUseEvolution && ((enableSuperEvolution && superEvolutionPoint > 0) || (enableEvolution && evolutionPoint > 0));
        }

        public void SetCanUseExtraMana(bool value)
        {
            canUseExtraMana = value;
        }

        public bool CanPayAbility(Card card, AbilityData ability)
        {
            bool exhaust = !card.exhausted||!ability.exhaust;
            return exhaust && Mana >= ability.manaCost;
        }
        
        public virtual bool IsDead()
        {
            if (cardsHand.Count == 0 && cardsBoard.Count == 0 && cardsDeck.Count == 0)
                return true;
            if (hp <= 0)
                return true;
            return false;
        }
        
        //Clone all player variables into another var, used mostly by the AI when building a prediction tree
        public static void Clone(Player source, Player dest)
        {
            dest.id = source.id;
            dest.isAI = source.isAI;
            dest.aiLevel = source.aiLevel;

            //Commented variables are not needed for ai predictions
            dest.username = source.username;
            dest.avatar = source.avatar;
            dest.deck = source.deck;
            dest.connected = source.connected;
            dest.ready = source.ready;

            dest.hp = source.hp;
            dest.hpMax = source.hpMax;
            dest.mana = source.mana;
            dest.manaMax = source.manaMax;
            dest.killCount = source.killCount;

            Card.CloneNone(source.hero, ref dest.hero);
            Card.CloneDict(source.cardsAll, dest.cardsAll);
            Card.CloneListRef(dest.cardsAll, source.cardsBoard, dest.cardsBoard);  
            Card.CloneListRef(dest.cardsAll, source.cardsEquip, dest.cardsEquip);  
            Card.CloneListRef(dest.cardsAll, source.cardsHand, dest.cardsHand);
            Card.CloneListRef(dest.cardsAll, source.cardsDeck, dest.cardsDeck);
            Card.CloneListRef(dest.cardsAll, source.cardsDiscard, dest.cardsDiscard);
            Card.CloneListRef(dest.cardsAll, source.cardsSecret, dest.cardsSecret);
            Card.CloneListRef(dest.cardsAll, source.cardsTemp, dest.cardsTemp);
            
            Article.CloneListRef(dest.articlesAll,source.articles, dest.articles);

            CardStatus.CloneList(source.status, dest.status);
            CardStatus.CloneList(source.ongoingStatus, dest.ongoingStatus);
        }
        
    }
    
    [Serializable]
    public class ActionHistory
    {
        public ushort type;
        public string cardId;
        public string cardUid;
        public string targetUid;
        public string abilityId;
        public int targetId;
        public Slot slot;
    }
}