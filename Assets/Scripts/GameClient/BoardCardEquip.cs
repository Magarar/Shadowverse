using GameLogic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameClient
{
    public class BoardCardEquip:MonoBehaviour
    {
        public Image equipSprite;
        public Image equipGlow;
        public TextMeshProUGUI equipHp;
        
        public Color glowAlly;
        public Color glowEnemy;
        
        private Canvas canvas;
        private RectTransform rect;
        
        private Card equip;
        private bool focus;
        private float targetAlpha = 0f;
        
        void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            rect = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (equip != null)
            {
                targetAlpha = focus ? 1f : 0f;
                focus = GameUI.IsOverRectTransform(canvas, rect);
            }
            else
            {
                targetAlpha = 0f;
                focus = false;
            }

            if (equipGlow != null)
            {
                int playerID = Gameclient.Get().GetPlayerID();
                Color color = playerID==equip.playerID?glowAlly:glowEnemy;
                float alpha = Mathf.MoveTowards(equipGlow.color.a, targetAlpha, Time.deltaTime*4f);
                equipGlow.color = new Color(color.r, color.g, color.b, alpha);
            }
        }
        
        public void SetEquip(Card equip)
        {
            if (equip != null)
            {
                this.equip = equip;
                equipSprite.sprite = equip.CardData.GetBoardArt(equip.VariantData);
                equipHp.text = equip.GetHp().ToString();
                
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);
                
            }
            else
            {
                Hide();
            }
        }
        
        public void Hide()
        {
            this.equip = null;
            focus = false;
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        public bool IsFocus()
        {
            return equip != null && focus;

        }
        
        public Card GetCard()
        {
            return equip;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            focus = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            focus = false;
        }
        
        void OnDisable()
        {
            focus = false;
        }

        
    }
}