using System.Collections.Generic;
using GameLogic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class SecretIconUI:MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Card card = null;
        private bool isHover = false;


        private static List<SecretIconUI> iconList = new List<SecretIconUI>();

        private void Awake()
        {
            iconList.Add(this);
        }

        private void OnDestroy()
        {
            iconList.Remove(this);
        }

        public void SetCard(Card card)
        {
            this.card = card;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHover = false;
        }

        void OnDisable()
        {
            isHover = false;
        }
        
        public Card GetCard()
        {
            return card;
        }
        
        public static Card GetHoverCard()
        {
            foreach (SecretIconUI line in iconList)
            {
                if (line.card != null && line.isHover)
                    return line.card;
            }
            return null;
        }
    }
}