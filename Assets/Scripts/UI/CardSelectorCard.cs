using Data;
using GameLogic;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// One card in the CardSelector
    /// </summary>
    public class CardSelectorCard:MonoBehaviour
    {
        public CardUI cardUI;
        
        private int index;
        private Vector2 targetPos;
        private Vector3 targetScale;

        private Card card;

        private RectTransform rect;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        private void Start()
        {
            transform.localScale = targetScale;
        }

        private void Update()
        {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, 5f*Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 2f*Time.deltaTime);
        }

        public void SetCard(Card card)
        {
            this.card = card;
            CardData icard = CardData.Get(card.cardId);
            cardUI.SetCard(icard,card.VariantData);
        }
        
        public void SetIndex(int index)
        {
            this.index = index;
        }

        public void SetTargetPos(Vector3 pos)
        {
            targetPos = pos;
        }

        public void SetTargetScale(Vector3 scale)
        {
            targetScale = scale;
        }
        
        public Card GetCard()
        {
            return card;
        }
        
        public int GetIndex()
        {
            return index;
        }
        
        public Vector3 GetTargetPos()
        {
            return targetPos;
        }

        public Vector3 GetTargetScale()
        {
            return targetScale;
        }
        
        
        
    }
}