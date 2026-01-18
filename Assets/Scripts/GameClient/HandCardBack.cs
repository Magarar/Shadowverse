using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.UI;

namespace GameClient
{
    /// <summary>
    /// Same as HandCard, but simpler version for the opponent's cards
    /// </summary>

    public class HandCardBack: MonoBehaviour
    {
        public Image cardSprite;
        private RectTransform rect;
        private static List<HandCardBack> cardList = new List<HandCardBack>();

        private void Awake()
        {
            cardList.Add(this);
            rect = GetComponent<RectTransform>();
            SetCardBack(null);
        }

        private void OnDestroy()
        {
            cardList.Remove(this);
        }
        
        public void SetCardBack(CardBackData cb)
        {
            if (cb != null && cb.cardBack != null)
                cardSprite.sprite = cb.cardBack;
        }
        
        public RectTransform GetRect()
        {
            if (rect == null)
                return GetComponent<RectTransform>();
            return rect;
        }
    }
}