using GameLogic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class CardMulligan:MonoBehaviour
    {
        public CardUI cardUI;
        public Image xImg;

        private Card card;
        
        public UnityAction<CardMulligan> onClick;
        
        private void Awake()
        {
            if (xImg != null)
                xImg.enabled = false;

            cardUI.onClick += OnClick;
        }
        
        public void SetCard(Card card)
        {
            this.card = card;
            cardUI.SetCard(card.CardData, card.VariantData);
            gameObject.SetActive(true);
        }
        
        public void SetSelected(bool discard)
        {
            if (xImg != null)
                xImg.enabled = discard;
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public bool IsSelected()
        {
            if (xImg != null)
                return xImg.enabled;
            return false;
        }
        
        public Card GetCard()
        {
            return card;
        }

        private void OnClick(CardUI cardUI)
        {
            onClick?.Invoke(this);
        }
    }
}