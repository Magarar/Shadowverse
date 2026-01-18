using System.Collections.Generic;
using System.Linq;
using CameraScripts;
using Data;
using GameLogic;
using UI;
using Unit;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameClient
{
    /// <summary>
    /// Represents the visual aspect of a card in hand.
    /// Will take the data from Card.cs and display it
    /// </summary>
    public class HandCard:MonoBehaviour
    {
        public Image cardGlow;
        public float moveSpeed = 10f;
        [FormerlySerializedAs("moveRateSpeed")] public float moveRotateSpeed = 4f;
        [FormerlySerializedAs("moveMaxRate")] public float moveMaxRotate = 10f;
        
        [HideInInspector]
        public Vector2 deckPosition;
        [FormerlySerializedAs("deckAngel")] [HideInInspector] 
        public float deckAngle;

        private string cardUID;
        
        private CardUI cardUI;
        private RectTransform handTransform;
        private RectTransform cardTransform;
        private Vector3 startScale;
        private Vector3 currentRotate;
        private Vector3 targetRotate;
        private Vector3 prevPos;
        
        private bool destroyed = false;
        private float focusTimer;
        
        private bool focus = false;
        private bool drag = false;
        private bool selected = false;

        private static List<HandCard> cardList = new();

        private void Awake()
        {
            cardList.Add(this);
            cardUI = GetComponent<CardUI>();
            cardTransform = GetComponent<RectTransform>();
            handTransform = transform.parent.GetComponent<RectTransform>();
            startScale = transform.localScale;
        }
        
        private void Start()
        {

        }
        
        private void OnDestroy()
        {
            cardList.Remove(this);
        }

        private void Update()
        {
            if(!Gameclient.Get().IsReady())
                return;
            
            Card card = GetCard();
            Vector2 targetPos = deckPosition;
            Vector3 targetSize = startScale;
            
            focusTimer+= Time.deltaTime;
            if (IsFocus())
            {
                targetPos = deckPosition+Vector2.up * 40f;
            }

            if (IsDrag())
            {
                targetPos = GetTargetPosition();
                targetSize = startScale * 0.75f;
                Vector3 dir = cardTransform.position - prevPos;
                Vector3 addrot = new Vector3(dir.y * 90f, -dir.x * 90f, 0f);
                targetRotate += addrot*moveRotateSpeed*Time.deltaTime;
                targetRotate = new Vector3(Mathf.Clamp(targetRotate.x, -moveMaxRotate, moveMaxRotate),
                    Mathf.Clamp(targetRotate.y, -moveMaxRotate, moveMaxRotate), 0f);
                currentRotate = Vector3.Lerp(currentRotate, targetRotate, moveRotateSpeed*Time.deltaTime);
            }
            else
            {
                targetRotate = new Vector3(0,0,deckAngle);
                currentRotate = new Vector3(0,0,deckAngle);
            }
            
            cardTransform.anchoredPosition = Vector2.Lerp(cardTransform.anchoredPosition, targetPos, moveSpeed*Time.deltaTime);
            cardTransform.localRotation = Quaternion.Slerp(cardTransform.localRotation, Quaternion.Euler(currentRotate),
                moveSpeed * Time.deltaTime);
            cardTransform.localScale = Vector3.Lerp(cardTransform.localScale, targetSize, 5f*Time.deltaTime);
            
            cardUI.SetCard(card);
            cardGlow.enabled = IsFocus() || IsDrag();
            prevPos = Vector3.Lerp(prevPos, cardTransform.position, 5f * Time.deltaTime);
            
            //Unselect
            if(!drag&&selected&&Input.GetMouseButtonDown(0))
                selected = false;
        }

        private Vector2 GetTargetPosition()
        {
            Card card = GetCard();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(handTransform, Input.mousePosition, Camera.main,
                out Vector2 targetPos);
            if (card.CardData.IsRequireTarget())
            {
                targetPos = deckPosition + Vector2.up * 150f + Vector2.right * targetPos.x / 10f;
            }
            return targetPos;
        }

        public void SetCard(Card card)
        {
            cardUID = card.uid;
            cardUI.SetCard(card);
        }

        public void Kill()
        {
            if (!destroyed)
            {
                destroyed = true;
                Destroy(gameObject);
            }
        }
        
        public bool IsFocus()
        {
            if (GameTool.IsMobile())
                return selected && !drag;
            return focus && !drag && focusTimer > 0f;
        }
        
        public bool IsDrag()
        {
            return drag;
        }
        
        public Card GetCard()
        {
            Game gdata = Gameclient.Get().GetGameData();
            return gdata.GetCard(cardUID);
        }

        public CardData GetCardData()
        {
            Card card = GetCard();
            return card?.Data;
        }

        public string GetCardUID()
        {
            return cardUID;
        }

        public void OnMouseEnterCard()
        {
            if (GameUI.IsUIOpened())
                return;
            focus = true;
        }

        public void OnMouseExitCard()
        {
            focus = false;
            focusTimer = -0.2f;
        }

        public void OnMouseDownCard()
        {
            if (GameUI.IsOverUILayer("UI"))
                return;
            
            UnselectAll();
            drag = true;
            selected = true;
            PlayerControls.Get().UnselectAll();
            AudioTool.Get().PlaySFX("hand_card", AssetData.Get().handCardClickAudio);

        }

        public void OnMouseUpCard()
        {
            Vector2 mpos = GameCamera.Get().MouseToPercent(Input.mousePosition);
            Vector3 boardPos = GameBoard.Get().RaycastMouseBoard();
            if(drag&&mpos.y>0.25f)
                TryPlayCard(boardPos);
            else if (!GameTool.IsMobile())
                HandCardArea.Get().SortCards();
            drag = false;
        }

        private void TryPlayCard(Vector3 boardPos)
        {
            if (!Gameclient.Get().IsYourTurn())
            {
                WarningText.ShowNotYourTurn();
                return;
            }
            
            int playerID = Gameclient.Get().GetPlayerID();
            Game gdata = Gameclient.Get().GetGameData();
            Player player = gdata.GetPlayer(playerID);
            Card card = GetCard();
            BSlot bslot = BSlot.GetNearest(boardPos);
            if (card.CardData.IsBoardCard())
            {
                bslot = BSlot.GetRandom(player);
            }
            
            Slot slot = Slot.None;
            if (bslot != null)
                slot = bslot.GetEmptySlot(boardPos);
            if(bslot != null && card.CardData.IsRequireTarget())
                slot = bslot.GetSlot(boardPos);
            
            if (!Tutorial.Get().CanDo(TutoEndTrigger.PlayCard, card))
                return;
            
            Card slotCard = bslot?.GetSlotCard(boardPos);
            if (bslot != null && card.CardData.IsRequireTargetSpell() && slotCard != null &&
                slotCard.HasStatus(StatusType.SpellImmunity))
            {
                WarningText.ShowSpellImmune();
                return;
            }
            
            if (!player.CanPayMana(card))
            {
                WarningText.ShowNoMana();
                return;
            }
            
            if (gdata.CanPlayCard(card, slot, true))
            {
                PlayCard(slot);
            }
            
        }

        private void PlayCard(Slot slot)
        {
            Gameclient.Get().PlayCard(GetCard(), slot);
            HandCardArea.Get().DelayRefresh(GetCard());
            Destroy(gameObject);
            if (GameTool.IsMobile())
                BoardCard.UnfocusAll();
        }
        
        public CardData CardData => GetCardData();

        public static HandCard GetDrag()
        {
            return cardList.FirstOrDefault(card => card.IsDrag());
        }

        public static HandCard GetFocus()
        {
            return cardList.FirstOrDefault(card => card.IsFocus());
        }

        public static HandCard Get(string uid)
        {
            return cardList.FirstOrDefault(card => card.GetCardUID() == uid);
        }

        

        public static void UnselectAll()
        {
            foreach (HandCard card in cardList)
                card.selected = false;
        }

        public static List<HandCard> GetAll()
        {
            return cardList;
        }
        

    }
}