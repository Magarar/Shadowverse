using System.Collections.Generic;
using Data;
using GameClient;
using GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// The UI for card selector, appears when an ability with CardSelector target is triggered
    /// </summary>
    public class CardSelector:SelectorPanel
    {
        public GameObject cardPrefab;
        
        public RectTransform content;
        public TextMeshProUGUI title;
        public TextMeshProUGUI subtitle;
        
        public Button selectButton;
        public TextMeshProUGUI selectButtonText;
        public float cardSpacing = 100f;
        
        private AbilityData iability;
        
        private List<Card> cardList = new List<Card>();
        private List<CardSelectorCard> selectorList = new List<CardSelectorCard>();
        
        private Vector2 mouseStart;
        private int mouseStartIndex;
        private int selectionIndex = 0;
        private bool drag = false;
        private bool forceShow = false;
        private float mouseScroll = 0f;
        private float timer = 0f;

        private static CardSelector instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
            Hide();
        }

        protected override void Update()
        {
            base.Update();
            
            timer+= Time.deltaTime;
            
            //DragCard
            Vector2 mousePos = GetMouseRectPosition();
            Vector2 move = mousePos - mouseStart;
            if (drag && move.magnitude > 0.1f)
            {
                selectionIndex = mouseStartIndex - Mathf.RoundToInt(move.x / cardSpacing);
                selectionIndex = Mathf.Clamp(selectionIndex, 0, selectorList.Count - 1);
            }
            
            //Mouse scroll
            mouseScroll += -Input.mouseScrollDelta.y;
            if (mouseScroll > 0.5f)
            {
                OnClickNext();
                mouseScroll -= 1f;
            }else if (mouseScroll < -0.5f)
            {
                OnClickPrev();
                mouseScroll += 1f;
            }
            
            //Refresh cards
            foreach (CardSelectorCard card in selectorList)
            {
                bool isSelected = card.GetIndex() == selectionIndex;
                Vector3 pos = GetCardPosition(card);
                Vector3 scale = isSelected ? Vector3.one : Vector3.one / 2f;
                card.SetTargetPos(pos);
                card.SetTargetScale(scale);

            }
            
            //Close on right click if not a selection
            if (iability == null && Input.GetMouseButtonDown(1) && timer > 1f)
                Hide();
            
            Game game = Gameclient.Get().GetGameData();
            if (game != null && iability != null && game.selector == SelectorType.None)
                Hide(); //Ability was selected already, close panel
            
        }

        public void RefreshPanel()
        {
            foreach (CardSelectorCard card in selectorList)
                Destroy(card.gameObject);
            
            selectorList.Clear();
            drag = false;
            mouseScroll = 0f;
            
            selectButtonText.text = (iability != null) ? "Select" : "OK";
            selectButton.gameObject.SetActive(iability != null);
            
            int index = 0;
            foreach (Card card in cardList)
            {
                CardData icard = CardData.Get(card.cardId);
                
                GameObject obj = Instantiate(cardPrefab, content.transform);
                RectTransform rect = obj.GetComponent<RectTransform>();
                CardSelectorCard selectorCard = obj.GetComponent<CardSelectorCard>();
                selectorCard.SetCard(card);
                selectorCard.SetIndex(index);
                
                Vector3 pos = GetCardPosition(selectorCard);
                Vector3 scale = (index == selectionIndex ? 1 : 0.5f) * Vector3.one;
                selectorCard.SetTargetPos(pos);
                selectorCard.SetTargetScale(scale);
                rect.anchoredPosition = pos;
                selectorList.Add(selectorCard);

                index++;
            }
        }

        //Show ability
        public override void Show(AbilityData iability, Card caster)
        {
            Game data = Gameclient.Get().GetGameData();
            this.cardList = iability.GetCardTargets(data, caster);
            this.iability = iability;
            forceShow = false;
            title.text = iability.title;
            subtitle.text = iability.desc;
            selectionIndex = 0;
            timer = 0f;
            Show();
        }
        
        //Show deck/discard
        public void Show(List<Card> cardList, string title)
        {
            this.cardList.Clear();
            this.cardList.AddRange(cardList);
            this.cardList.Sort((Card a, Card b) => a.CardData.title.CompareTo(b.CardData.title)); //Reorder to not show the deck order
            iability = null;
            forceShow = false;
            this.title.text = title;
            subtitle.text = "";
            selectionIndex = 0;
            timer = 0f;
            Show();
        }

        public void OnClickOK()
        {
            Game data = Gameclient.Get().GetGameData();
            if (iability != null && data.selector == SelectorType.SelectorCard)
            {
                CardSelectorCard selectorCard = null;

                if (selectionIndex >= 0 && selectionIndex < selectorList.Count)
                    selectorCard = selectorList[selectionIndex];

                if (selectorCard != null)
                {
                    Card selectedCard = selectorCard.GetCard();
                    Card caster = data.GetCard(data.selectorCasterUid);
                    if (selectedCard != null && iability.AreTargetConditionsMet(data, caster, selectedCard))
                    {
                        Gameclient.Get().SelectCard(selectedCard);
                        Hide();
                    }
                }
            }
            else
            {
                Hide();
            }
        }

        public void OnClickMouseDown()
        {
            mouseStart = GetMouseRectPosition();
            mouseStartIndex = selectionIndex;
            drag = true;
        }

        public void OnClickMouseUp()
        {
            drag = false;
        }
        
        public void OnClickCancel()
        {
            Gameclient.Get().CancelSelection();
            Hide();
        }
        
        public void OnClickNext()
        {
            selectionIndex += 1;
            selectionIndex = Mathf.Clamp(selectionIndex, 0, selectorList.Count - 1);
        }
        
        public void OnClickPrev()
        {
            selectionIndex -= 1;
            selectionIndex = Mathf.Clamp(selectionIndex, 0, selectorList.Count - 1);
        }
        
        private Vector3 GetCardPosition(CardSelectorCard card)
        {
           int indexOffset = card.GetIndex() - selectionIndex;
           Vector2 pos = new Vector2(indexOffset * cardSpacing, (indexOffset!=0?50f:0));
           float centerOffset = (indexOffset != 0) ? (Mathf.Sign(indexOffset) * 140f) : 0;
           pos += Vector2.right * centerOffset;
           return pos;
        }
        
        private Vector2 GetMouseRectPosition()
        {
            Vector2 localpoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(content, Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out localpoint);
            return localpoint;
        }
        
        public bool IsAbility()
        {
            return IsVisible() && iability != null;
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }
        
        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            forceShow = false;
        }
        
        public override bool ShouldShow()
        {
            Game data = Gameclient.Get().GetGameData();
            int playerID = Gameclient.Get().GetPlayerID();
            return forceShow || (data.selector == SelectorType.SelectorCard && data.selectorPlayerId == playerID);
        }
        
        public static CardSelector Get()
        {
            return instance;
        }
        
    }
}