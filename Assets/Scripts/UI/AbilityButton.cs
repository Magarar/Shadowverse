using System;
using System.Collections.Generic;
using Data;
using GameClient;
using GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AbilityButton: MonoBehaviour
    {
        public TextMeshProUGUI text;
        public Image focusHighlight;
        
        private Card card;
        private AbilityData ability;
        
        private CanvasGroup canvasGroup;
        private float targetAlpha = 0f;
        private bool focus = false;
        private bool nextFocus = false;
        private bool interactable = false;
        
        private static List<AbilityButton> buttonList = new List<AbilityButton>();

        private void Awake()
        {
            buttonList.Add(this);
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            if (focusHighlight != null)
            {
                focusHighlight.enabled = false;
            }
        }

        private void OnDestroy()
        {
            buttonList.Remove(this);
        }

        public void Update()
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * 5f);
            focus = nextFocus;

            if (focusHighlight != null && IsVisible())
            {
                focusHighlight.enabled = focus&&interactable;
            }
        }

        public void SetAbility(Card card, AbilityData iability)
        {
            this.card = card;
            this.ability = iability;
            text.text = ability.title;
            if(ability.manaCost>0)
                text.text += " ("+ability.manaCost+")";
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            targetAlpha = 1f;

        }

        public void SetInteractable(bool interact)
        {
            interactable = interact;
        }
        
        public void Hide()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            card = null;
            ability = null;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            targetAlpha = 0f;
        }

        public void OnClick()
        {
            if (card != null && ability != null)
            {
                if (!Tutorial.Get().CanDo(TutoEndTrigger.CastAbility, card))
                    return;
                
                Gameclient.Get().CastAbility(card, ability);
                PlayerControls.Get().UnselectAll();
            }
        }

        public AbilityData GetAbility()
        {
            return ability;
        }

        private bool IsVisible()
        {
            return canvasGroup.alpha > 0.5f;
        }

        public bool IsInteractable()
        {
            return interactable && IsVisible();
        }

        public void MouseEnter()
        {
            focus = true;
            nextFocus = true;
        }
        
        public void MouseExit()
        {
            nextFocus = false; //Keep it focused 1 more frame to work on mobile
        }
        
        

        public static AbilityButton GetFocus(Vector3 pos, float range=999f)
        {
            AbilityButton nearest = null;
            float minDist = range;
            foreach (AbilityButton button in buttonList)
            {
                float dist = (button.transform.position - pos).magnitude;
                if (button.focus && button.IsVisible() && dist < minDist)
                {
                    minDist = dist;
                    nearest = button;
                }
            }
            return nearest;
        }

        public static AbilityButton GetNearest(Vector3 pos, float range = 999f)
        {
            AbilityButton nearest = null;
            float minDist = range;
            foreach (AbilityButton button in buttonList)
            {
                float dist = (button.transform.position - pos).magnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = button;
                }
            }
            return nearest;
        }
        
        
    }
}