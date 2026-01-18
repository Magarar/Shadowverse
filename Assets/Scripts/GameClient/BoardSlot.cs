using System.Collections.Generic;
using Data;
using GameLogic;
using UI;
using UnityEngine;

namespace GameClient
{
    public class BoardSlot:BSlot
    {
        public BoardSlotType type;
        public int x;
        public int y;
        
        private static List<BoardSlot> boardSlotsList = new List<BoardSlot>();
        
        protected override void Awake()
        {
            base.Awake();
            boardSlotsList.Add(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            boardSlotsList.Remove(this);
        }

        private void Start()
        {
            if (x < Slot.xMin || x > Slot.xMax || y < Slot.yMin || y > Slot.yMax)
            {
                Debug.LogError("Invalid slot: " + gameObject.name +
                               " Board Slot X and Y value must be within the min and max set for those values, " +
                               "check Slot.cs script to change those min/max.");
            }
            
        }

        protected override void Update()
        {
            base.Update();
            if(!Gameclient.Get().IsReady())
                return;
            
            BoardCard boardCardSelected = PlayerControls.Get().GetSelected();
            HandCard dragCard = HandCard.GetDrag();
            
            Game gdata = Gameclient.Get().GetGameData();
            Player player = Gameclient.Get().GetPlayer();
            Slot slot = GetSlot();
            Card dcard = dragCard?.GetCard();
            Card slotCard = gdata.GetSlotCard(slot);
            bool yourTurn = Gameclient.Get().IsYourTurn();
            collider.enabled = slotCard == null; //Disable collider when a card is here
            
            //Find target opacity value
            targetAlpha = 0f;
            if (yourTurn && dcard != null && dcard.CardData.IsBoardCard() && gdata.CanPlayCard(dcard, slot))
                targetAlpha = 0f;//hightlight when dragging a character or artifact
            if(yourTurn&&dcard!=null&&dcard.CardData.IsRequireTarget()&&gdata.CanPlayCard(dcard,slot))
                targetAlpha = 1f;//Highlight when dragin a spell with target
            if (gdata.selector == SelectorType.SelectTarget && player.id == gdata.selectorPlayerId)
            {
                Card caster = gdata.GetCard(gdata.selectorCasterUid);
                AbilityData ability = AbilityData.Get(gdata.selectorAbilityId);
                if(ability!=null&&slotCard==null&&ability.CanTarget(gdata,caster,slot))
                    targetAlpha = 1f;//Highlight when selecting a target and slot are valid
                if (ability != null && slotCard != null && ability.CanTarget(gdata, caster, slotCard))
                    targetAlpha = 1f; //Highlight when selecting a target and cards are valid
            }
            
            Card selectCard = boardCardSelected?.GetCard();
            bool canDoMove = yourTurn&&selectCard!=null&&slotCard==null&&gdata.CanMoveCard(selectCard,slot);
            bool canDoAttack = yourTurn&&selectCard!=null&&slotCard!=null&&gdata.CanAttackTarget(selectCard,slotCard);
            if(canDoMove||canDoAttack)
                targetAlpha = 1f;
        }

        //Find the actual slot coordinates of this board slot
        public override Slot GetSlot()
        {
            int p = 0;

            if (type == BoardSlotType.FlipX)
            {
                int pid = Gameclient.Get().GetPlayerID();
                int px = x;
                if ((pid % 2) == 1)
                    px = Slot.xMax - x + Slot.xMin; //Flip X coordinate if not the first player
                return new Slot(px, y, p);
            }

            if (type == BoardSlotType.FlipY)
            {
                int pid = Gameclient.Get().GetPlayerID();
                int py = y;
                if ((pid % 2) == 1)
                    py = Slot.yMax - y + Slot.yMin;
                return new Slot(x, py, p);
            }
            
            if (type == BoardSlotType.PlayerSelf)
                p = Gameclient.Get().GetPlayerID();
            if (type == BoardSlotType.PlayerOpponent)
                p = Gameclient.Get().GetOpponentPlayerID();
            
            return new Slot(x, y, p);
        }
        
        //When clicking on the slot
        public void OnMouseDown()
        {
            if (GameUI.IsOverUI())
                return;
            
            Game gdata = Gameclient.Get().GetGameData();
            int playerId = Gameclient.Get().GetPlayerID();

            if (gdata.selector == SelectorType.SelectTarget && playerId == gdata.selectorPlayerId)
            {
                Slot slot = GetSlot();
                Card slotCard = gdata.GetSlotCard(slot);
                if (slotCard == null)
                {
                    Gameclient.Get().SelectSlot(slot);
                }
            }
        }
    }
}