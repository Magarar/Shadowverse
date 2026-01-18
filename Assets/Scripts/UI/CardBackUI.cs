using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class CardBackUI:MonoBehaviour
    {
        public UnityAction<CardBackData> onClick;
        
        private Image cardBackImage;
        private Button cardBackButton;
        private Sprite defaultIcon;
        
        private CardBackData cardBack;

        void Awake()
        {
            cardBackImage = GetComponent<Image>();
            cardBackButton = GetComponent<Button>();
            defaultIcon = cardBackImage.sprite;
            
            if (cardBackButton != null)
                cardBackButton.onClick.AddListener(OnClick);
        }

        public void SetCardBack(CardBackData cardBack)
        {
            this.cardBack = cardBack;
            cardBackImage.sprite = defaultIcon;
            cardBackImage.enabled = true;

            if (cardBackButton != null)
            {
                cardBackImage.sprite = cardBack.cardBack;
            }
        }

        public void SetDefaultCardBack()
        {
            cardBackImage.sprite = defaultIcon;
            cardBackImage.enabled = true;
            cardBack = null;
        }
        
        public void Hide()
        {
            this.cardBack = null;
            cardBackImage.enabled = false;
        }
        
        public CardBackData GetCardback()
        {
            return cardBack;
        }

        private void OnClick()
        {
            onClick?.Invoke(cardBack);
        }
    }
}