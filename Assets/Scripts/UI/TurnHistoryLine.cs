using System.Collections.Generic;
using Data;
using GameClient;
using GameLogic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// One of the squares in the history bar
    /// </summary>
    public class TurnHistoryLine: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public HoverTargetUI hover;
        public Image cardImg;
        
        private Card card;
        private float timer = 0f;
        private bool isHover = false;
        
        private static List<TurnHistoryLine> lineList = new List<TurnHistoryLine>();

        private void Awake()
        {
            lineList.Add(this);
        }

        private void OnDestroy()
        {
            lineList.Remove(this);
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void Update()
        {
            timer += Time.deltaTime;
        }

        public void SetLine(ActionHistory history)
        {
            Game gdata = Gameclient.Get().GetGameData();
            Card acard = gdata.GetCard(history.cardUid);
            Card target = gdata.GetCard(history.targetUid);
            Player ptarget = gdata.GetPlayer(history.targetId);
            CardData icard = CardData.Get(history.cardId);
            CardData itarget = CardData.Get(target?.cardId);
            VariantData variant = acard.VariantData;
            AbilityData iability = AbilityData.Get(history.abilityId);
            card = acard;
            
            if(icard==null)
                return;

            if (history.type == GameAction.PlayCard)
            {
                string text = icard.title + "was played";
                SetLine(icard, variant, text);
            }
            
            if (history.type == GameAction.Move)
            {
                string text = icard.title + " moved";
                SetLine(icard, variant, text);
            }

            if (history.type == GameAction.Attack && itarget != null)
            {
                string text = icard.title + " attacked " + itarget.title;
                SetLine(icard, variant, text);
            }

            if (history.type == GameAction.AttackPlayer && ptarget != null)
            {
                string text = icard.title + " attacked " + ptarget.username;
                SetLine(icard, variant, text);
            }

            if (history.type == GameAction.CastAbility && iability != null)
            {
                if (iability.target == AbilityTarget.SelectTarget && itarget != null)
                {
                    string text = icard.title + " casted " + iability.GetTitle() + " on " + itarget.title;
                    SetLine(icard, variant, text);
                }
                else
                {
                    string text = icard.title + " casted " + iability.GetTitle();
                    SetLine(icard, variant, text);
                }
            }

            if (history.type == GameAction.SecretTriggered)
            {
                string text = icard.title + " was triggered";
                SetLine(icard, variant, text);
            }
            
        }

        public void SetLine(CardData icard, VariantData variant, string text)
        {
            cardImg.sprite = icard.GetFullArt(variant);
            hover.text = text;
            gameObject.SetActive(true);
            timer = 0f;
        }
        
        public void Hide()
        {
            card = null;
            if (timer > 0.05f)
                gameObject.SetActive(false);
        }
        


        public void OnPointerEnter(PointerEventData eventData)
        {
            timer = 0f;
            isHover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            timer = 0f;
            isHover = false;
        }

        void OnDisable()
        {
            isHover = false;
        }

        public static Card GetHoverCard()
        {
            foreach (TurnHistoryLine line in lineList)
            {
                if (line.card != null && line.isHover)
                    return line.card;
            }
            return null;
        }
    }
}