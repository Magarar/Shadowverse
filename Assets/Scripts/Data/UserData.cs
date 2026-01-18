using System;
using System.Collections.Generic;
using System.Linq;
using Network;
using Unit;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    [Serializable]
    public class UserData
    {
        public string id;
        public string username;
        
        public string email;
        public string avatar;
        public string cardback;
        public int permissionLevel = 1;
        public int validationLevel = 1;
        
        public int coins;
        public int xp;
        public int elo;
        
        public int matches;
        public int victories;
        public int defeats;
        
        public UserCardData[] cards;
        public UserCardData[] packs;
        public UserDeckData[] decks;
        public string[] rewards;
        public string[] avatars;
        public string[] cardbacks;
        public string[] friends;
        
        public UserData()
        {
            cards = Array.Empty<UserCardData>();
            packs = Array.Empty<UserCardData>();
            decks = Array.Empty<UserDeckData>();
            rewards = Array.Empty<string>();
            avatars = Array.Empty<string>();
            cardbacks = Array.Empty<string>();
            friends = Array.Empty<string>();
            permissionLevel = 1;
            coins = 10000;
            elo = 1000;
        }
        
        public int GetLevel()
        {
            return Mathf.FloorToInt(xp / 1000) + 1;
        }
        
        public string GetAvatar()
        {
            if (avatar != null)
                return avatar;
            return "";
        }
        
        public string GetCardback()
        {
            if (cardback != null)
                return cardback;
            return "";
        }
        
        public void SetDeck(UserDeckData deck)
        {
            for(int i=0; i<decks.Length; i++)
            {
                if (decks[i].tid == deck.tid)
                {
                    decks[i] = deck;
                    return;
                }
            }

            //Not found
            List<UserDeckData> ldecks = new List<UserDeckData>(decks);
            ldecks.Add(deck);
            this.decks = ldecks.ToArray();
        }
        
        public UserDeckData GetDeck(string tid)
        {
            foreach (UserDeckData deck in decks)
            {
                if (deck.tid == tid)
                    return deck;
            }
            return null;
        }
        
        public UserCardData GetCard(string tid, string variant)
        {
            foreach (UserCardData card in cards)
            {
                if (card.tid == tid && card.variant == variant)
                    return card;
            }
            return null;
        }
        
        public int GetCardQuantity(CardData card, VariantData variant)
        {
            return GetCardQuantity(card.id, variant.id, variant.isDefault);
        }
        
        public int GetCardQuantity(string tid, string variant, bool defaultVariant = false)
        {
            if (cards == null)
                return 0;

            foreach (UserCardData card in cards)
            {
                if (card.tid == tid && card.variant == variant)
                    return card.quantity;
                if (card.tid == tid && card.variant == "" && defaultVariant)
                    return card.quantity;
            }
            return 0;
        }
        
        public UserCardData GetPack(string tid)
        {
            foreach (UserCardData pack in packs)
            {
                if (pack.tid == tid)
                    return pack;
            }
            return null;
        }
        
        public int GetPackQuantity(string tid)
        {
            if (packs == null)
                return 0;

            foreach (UserCardData pack in packs)
            {
                if (pack.tid == tid)
                    return pack.quantity;
            }
            return 0;
        }
        
        public int CountUniqueCards()
        {
            if (cards == null)
                return 0;

            HashSet<string> unique_cards = new HashSet<string>();
            foreach (UserCardData card in cards)
            {
                if (!unique_cards.Contains(card.tid))
                    unique_cards.Add(card.tid);
            }
            return unique_cards.Count;
        }
        
        public int CountCardType(VariantData variant)
        {
            int value = 0;
            foreach (UserCardData card in cards)
            {
                if (card.variant == variant.id)
                    value += 1;
            }
            return value;
        }
        
        public bool HasDeckCards(UserDeckData deck)
        {
            foreach (UserCardData card in deck.cards)
            {
                bool defaultVariant = true; //Count "" variant as valid for compatibilty with older vers
                if (GetCardQuantity(card.tid, card.variant, defaultVariant) < card.quantity)
                    return false;
            }

            return true;
        }
        
        public bool IsDeckValid(UserDeckData deck)
        {
            if (Authenticator.Get().IsApi())
                return HasDeckCards(deck) && deck.IsValid();
            return deck.IsValid();
        }
        
        public void AddDeck(UserDeckData deck)
        {
            List<UserDeckData> udecks = new List<UserDeckData>(decks);
            udecks.Add(deck);
            decks = udecks.ToArray();

            foreach (UserCardData card in deck.cards)
            {
                AddCard(card.tid, card.variant, 1);
            }
        }
        
        public void AddPack(string tid, int quantity)
        {
            bool found = false;
            foreach (UserCardData pack in packs)
            {
                if (pack.tid == tid)
                {
                    found = true;
                    pack.quantity += quantity;
                }
            }
            if (!found)
            {
                UserCardData npack = new UserCardData();
                npack.tid = tid;
                npack.quantity = quantity;
                List<UserCardData> apacks = new List<UserCardData>(packs);
                apacks.Add(npack);
                packs = apacks.ToArray();
            }
        }
        
        public void AddCard(string tid, string variant, int quantity)
        {
            bool found = false;
            foreach (UserCardData card in cards)
            {
                if (card.tid == tid && card.variant == variant)
                {
                    found = true;
                    card.quantity += quantity;
                }
            }
            if (!found)
            {
                UserCardData ncard = new UserCardData();
                ncard.tid = tid;
                ncard.variant = variant;
                ncard.quantity = quantity;
                List<UserCardData> acards = new List<UserCardData>(cards);
                acards.Add(ncard);
                cards = acards.ToArray();
            }
        }
        
        public void AddReward(string tid)
        {
            if (!HasReward(tid))
            {
                List<string> arewards = new List<string>(rewards);
                arewards.Add(tid);
                rewards = arewards.ToArray();
            }
        }
        
        public bool HasCard(string cardTid, string variant, int quantity = 1)
        {
            foreach (UserCardData card in cards)
            {
                if (card.tid == cardTid && card.variant == variant && card.quantity >= quantity)
                    return true;
            }
            return false;
        }
        
        public bool HasPack(string packTid, int quantity=1)
        {
            foreach (UserCardData pack in packs)
            {
                if (pack.tid == packTid && pack.quantity >= quantity)
                    return true;
            }
            return false;
        }
        
        public bool HasAvatar(string avatarTid)
        {
            return avatars.Contains(avatarTid);
        }
        
        public bool HasCardback(string cardbackTid)
        {
            return cardbacks.Contains(cardbackTid);
        }
        
        public bool HasReward(string rewardID)
        {
            foreach (string reward in rewards)
            {
                if (reward == rewardID)
                    return true;
            }
            return false;
        }
        
        public string GetCoinsString()
        {
            return coins.ToString();
        }

        public bool HasFriend(string username)
        {
            List<string> flist = new List<string>(friends);
            return flist.Contains(username);
        }
        
        public void AddFriend(string username)
        {
            List<string> flist = new List<string>(friends);
            if (!flist.Contains(username))
                flist.Add(username);
            friends = flist.ToArray();
        }
        
        public void RemoveFriend(string username)
        {
            List<string> flist = new List<string>(friends);
            if (flist.Contains(username))
                flist.Remove(username);
            friends = flist.ToArray();
        }
    }

    [Serializable]
    public class UserDeckData : INetworkSerializable
    {
        public string tid;
        public string title;
        public UserCardData hero;
        public UserCardData[] cards;
        
        public UserDeckData() {}
        
        public UserDeckData(string tid, string title)
        {
            this.tid = tid;
            this.title = title;
            hero = new UserCardData();
            cards = Array.Empty<UserCardData>();
        }
        
        public UserDeckData(DeckData deck)
        {
            tid = deck.id;
            title = deck.title;
            hero = new UserCardData(deck.hero, deck.hero.GetVariant());
            cards = new UserCardData[deck.cards.Length];
            for (int i = 0; i < deck.cards.Length; i++)
            {
                cards[i] = new UserCardData(deck.cards[i], deck.cards[i].GetVariant());
            }
        }
        
        public int GetQuantity()
        {
            int count = 0;
            foreach (UserCardData card in cards)
                count += card.quantity;
            return count;
        }
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(tid) && !string.IsNullOrWhiteSpace(title) && GetQuantity() >= GamePlayData.Get().deckSize;
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tid);
            serializer.SerializeValue(ref title);
            serializer.SerializeValue(ref hero);
            NetworkTool.NetSerializeArray(serializer, ref cards);
        }
        
        public static UserDeckData Default
        {
            get
            {
                UserDeckData deck = new UserDeckData();
                deck.tid = "";
                deck.title = "";
                deck.hero = new UserCardData();
                deck.cards = new UserCardData[0];
                return deck;
            }
        }
    }
    
    [Serializable]
    public class UserCardData : INetworkSerializable
    {
        public string tid;
        public string variant;
        public int quantity;

        public UserCardData() { tid = ""; variant = ""; quantity = 1; }
        public UserCardData(string id, string v) { tid = id; variant = v; quantity = 1; }
        public UserCardData(CardData card, VariantData variant) 
        {
            this.tid = card != null ? card.id : "";
            this.variant = variant != null ? card.GetVariant().id : "";
            this.quantity = 1;
        }


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tid);
            serializer.SerializeValue(ref variant);
            serializer.SerializeValue(ref quantity);
        }
    }
}