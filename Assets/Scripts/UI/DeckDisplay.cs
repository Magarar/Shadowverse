using System.Collections.Generic;
using Data;
using Menu;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    /// <summary>
    /// Can display a deck in the UI
    /// Only shows a few cards and the total amount of cards
    /// </summary>
    public class DeckDisplay: MonoBehaviour
    {
        public TextMeshProUGUI deckTitle;
        public TextMeshProUGUI cardCount;
        public CardUI[] uiCards;
        
        private string deckID;
        
        void Awake()
        {
            Clear();
        }

        void Update()
        {

        }
        
        public void Clear()
        {
            if(deckTitle != null)
                deckTitle.text = "";
            if(cardCount != null)
                cardCount.text = "";
            foreach (var card in uiCards)
                card.Hide();
        }

        public void SetDeck(string tid)
        {
            UserData user = Authenticator.Get().GetUserData();
            UserDeckData udeck = user.GetDeck(tid);
            DeckData ddeck = DeckData.Get(tid);
            if (udeck != null)
                SetDeck(udeck);
            else if (ddeck != null)
                SetDeck(ddeck);
            else
                Clear();
        }

        public void SetDeck(UserDeckData deck)
        {
            Clear();
            if (deckTitle != null)
            {
                deckID = deck.tid;
                if(deck.title != null)
                    deckTitle.text = deck.title;
                if (cardCount != null)
                {
                    cardCount.text = deck.GetQuantity().ToString()+"/"+GamePlayData.Get().deckSize.ToString();
                    cardCount.color = deck.GetQuantity()>=GamePlayData.Get().deckSize?Color.white:Color.red;
                }
                
                List<CardDataQ> cards = new List<CardDataQ>();
                foreach (UserCardData ucard in deck.cards)
                {
                    CardDataQ cq = new CardDataQ
                    {
                        card = CardData.Get(ucard.tid),
                        variant = VariantData.Get(ucard.variant),
                        quantity = ucard.quantity
                    };
                    if(cq.card != null)
                        cards.Add(cq);
                }
                ShowCards(cards);
            }
            gameObject.SetActive(deck != null);
        }

        public void SetDeck(DeckData deck)
        {
            Clear();
            if (deck != null)
            {
                deckID = deck.id;
                if (deckTitle != null)
                    deckTitle.text = deck.title;
                if (cardCount != null)
                {
                    cardCount.text = deck.GetQuantity().ToString()+"/"+GamePlayData.Get().deckSize.ToString();
                    cardCount.color = deck.GetQuantity()>=GamePlayData.Get().deckSize?Color.white:Color.red;
                }
                
                List<CardDataQ> cards = new List<CardDataQ>();
                foreach (CardData card in deck.cards)
                {
                    if (card != null)
                    {
                        CardDataQ cq = new CardDataQ()
                        {
                            card = card,
                            variant = card.GetVariant(),
                            quantity = 1
                        };
                        cards.Add(cq);
                    }
                }

                if (deck is DeckPuzzleData)
                {
                    DeckPuzzleData pdeck = (DeckPuzzleData)deck;
                    foreach (DeckCardSlot slot in pdeck.boardCards)
                    {
                        if (slot.card != null)
                        {
                            CardDataQ cq = new CardDataQ()
                            {
                                card = slot.card,
                                variant = slot.card.GetVariant(),
                                quantity = 1
                            };
                            cards.Add(cq);
                            
                        }
                    }
                }
                ShowCards(cards);
            }
            gameObject.SetActive(deck != null);
        }

        private void ShowCards(List<CardDataQ> cards)
        {
            cards.Sort((CardDataQ a, CardDataQ b) => b.card.mana.CompareTo(a.card.mana));
            
            int index = 0;
            foreach (CardDataQ icard in cards)
            {
                for (int i = 0; i < icard.quantity; i++)
                {
                    if (index < uiCards.Length)
                    {
                        CardUI cardUI = uiCards[index];
                        cardUI.SetCard(icard.card, icard.variant);
                        index++;
                    }
                }
            }
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public string GetDeck()
        {
            return deckID;
        }



     
    }
}