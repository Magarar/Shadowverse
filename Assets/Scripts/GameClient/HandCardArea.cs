using System.Collections.Generic;
using GameLogic;
using UnityEngine;

namespace GameClient
{
    /// <summary>
    /// Area where all the hand cards are
    /// Will take card of spawning/despawning hand cards based on the refresh data received from server
    /// </summary>
    public class HandCardArea:MonoBehaviour
    {
        public GameObject cardPrefab;
        public RectTransform cardArea;
        public float cardSpacing = 100f;
        public float cardAngel = 10f;
        public float cardOffsetY = 10f;
        
        private List<HandCard> cards = new();
        
        private bool isDragging;

        private string lastDestroyed;
        private float lastDestroyedTimer;
        
        private static HandCardArea instance;

        public void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (!Gameclient.Get().IsReady())
                return;
            
            int playerId = Gameclient.Get().GetPlayerID();
            Game gdata = Gameclient.Get().GetGameData();
            Player player = gdata.GetPlayer(playerId);
            
            lastDestroyedTimer += Time.deltaTime;

            //Add new cards
            foreach (Card card in player.cardsHand)
            {
                if(!HasCard(card.uid))
                    SpawnNewCard(card);
            }
            
            //Remove destroyed cards
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                HandCard handCard = cards[i];
                if (handCard == null || player.GetHandCard(handCard.GetCard().uid)== null)
                {
                    cards.RemoveAt(i);
                    if(handCard!=null)
                        handCard?.Kill();
                }
            }
            
            //Set card index
            int index = 0;
            float countHalf = cards.Count / 2f;
            foreach (HandCard handCard in cards)
            {
                handCard.deckPosition = new Vector2((index - countHalf) * cardSpacing,
                    (index - countHalf) * (index - countHalf) * -cardOffsetY);
                handCard.deckAngle = (index-countHalf) * cardAngel;
                index++;
            }
            
            HandCard dragCard = HandCard.GetDrag();
            isDragging = dragCard != null;
        }

        public void SpawnNewCard(Card card)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardArea.transform);
            cardObj.GetComponent<HandCard>().SetCard(card);
            cardObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -100f);
            cards.Add(cardObj.GetComponent<HandCard>());
        }
        
        public void DelayRefresh(Card getCard)
        {
            lastDestroyed = getCard.uid;
            lastDestroyedTimer = 0;
        }

        public void SortCards()
        {
            cards.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            int i = 0;
            foreach (HandCard acard in cards)
            {
                acard.transform.SetSiblingIndex(i);
                i++;
            }
        }

        private bool HasCard(string cardUid)
        {
            HandCard card = HandCard.Get(cardUid);
            bool justDestroyed = cardUid==lastDestroyed&&lastDestroyedTimer<0.7f;
            return card != null||justDestroyed;
        }

        public bool IsDragging()
        {
            return isDragging;
        }

        public static HandCardArea Get()
        {
            return instance;
        }
        

       
    }
}