using System.Collections.Generic;
using Data;
using GameLogic;
using UnityEngine;

namespace GameClient
{
    /// <summary>
    /// Same as HandCardArea but for the opponents hand
    /// Simpler version with display only (no draging of card)
    /// </summary>
    public class OpponentHand: MonoBehaviour
    {
        public GameObject cardPrefab;
        public RectTransform cardArea;
        public float cardSpacing = 100f;
        public float cardAngle = 10f;
        public float cardOffsetY = 10f;

        private List<HandCardBack> cards = new List<HandCardBack>();

        void Update()
        {
            if(!Gameclient.Get().IsReady())
                return;
            Game gdata = Gameclient.Get().GetGameData();
            Player player = gdata.GetPlayer(Gameclient.Get().GetOpponentPlayerID());
            
            if (cards.Count < player.cardsHand.Count)
            {
                GameObject newCard = Instantiate(cardPrefab, cardArea);
                HandCardBack handCard = newCard.GetComponent<HandCardBack>();
                CardBackData cbdata = CardBackData.Get(player.cardBack);
                handCard.SetCardBack(cbdata);
                RectTransform card_rect = newCard.GetComponent<RectTransform>();
                card_rect.anchoredPosition = new Vector2(0f, 100f);
                cards.Add(handCard);
            }
            
            if (cards.Count > player.cardsHand.Count)
            {
                HandCardBack card = cards[cards.Count - 1];
                cards.RemoveAt(cards.Count - 1);
                Destroy(card.gameObject);
            }
            
            int nbCards = Mathf.Min(cards.Count, player.cardsHand.Count);
            for (int i = 0; i < nbCards; i++)
            {
                HandCardBack card = cards[i];
                RectTransform crect = card.GetRect();
                float half = nbCards / 2f;
                Vector3 tpos = new Vector3((i - half) * cardSpacing, (i - half) * (i - half) * cardOffsetY);
                float tangle = (i - half) * cardAngle;
                crect.anchoredPosition = Vector3.Lerp(crect.anchoredPosition, tpos, 4f * Time.deltaTime);
                card.transform.localRotation = Quaternion.Slerp(card.transform.localRotation, Quaternion.Euler(0f, 0f, tangle), 4f * Time.deltaTime);
            }
        }
    }
}