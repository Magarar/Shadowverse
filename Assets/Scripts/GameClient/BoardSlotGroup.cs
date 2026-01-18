using System.Collections.Generic;
using System.Linq;
using GameLogic;
using UnityEngine;

namespace GameClient
{
    ///<总结>
    ///Slot.cs的视觉表示，可自动定位卡片
    ///</摘要>
    
    public class BoardSlotGroup:BSlot
    {
        public BoardSlotType type;
        public int xMin = 1;
        public int xMax = 5;
        public int y = 1;

        public float spacing = 2.5f;
        public float reduceDelay = 1f;
        
        private int nbOccupied = 0;
        
        private List<GroupSlot> groupSlotsList = new();

        protected override void Awake()
        {
            base.Awake();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void Start()
        {
            if (xMin < Slot.xMin || xMax > Slot.xMax || y < Slot.yMin || y > Slot.yMax)
                Debug.LogError("Board Slot X and Y value must be within the min and max set for those values, check Slot.cs script to change those min/max.");
            
            Gameclient.Get().onConnectGame += OnConnect;
        }

        private void OnConnect()
        {
            foreach (var slot in Slot.GetAll())
            {
                if (IsInGroup(slot))
                {
                    GroupSlot pos = new GroupSlot();
                    pos.slot = slot;
                    pos.pos = transform.position;
                    groupSlotsList.Add(pos);
                }
            }
        }

        protected override void Update()
        {
            base.Update();
            
            if(!Gameclient.Get().IsReady())
                return;
            
            Game gdata = Gameclient.Get().GetGameData();
            HandCard dragCard = HandCard.GetDrag();
            bool yourTurn = Gameclient.Get().IsYourTurn();
            Card dcard = dragCard?.GetCard();

            targetAlpha = 0;
            if (yourTurn && dcard != null && dcard.CardData.IsBoardCard())
            {
                foreach (var groupSlot in groupSlotsList)
                {
                    if(gdata.CanPlayCard(dcard,groupSlot.slot))
                        targetAlpha = 1;
                }
            }
            
            UpdateOccupied();
            UpdatePositions();

        }
        
        private void UpdateOccupied()
        {
            int count = 0;
            Game gdata = Gameclient.Get().GetGameData();

            foreach (var groupSlot in groupSlotsList)
            {
                Card slotCard = gdata.GetSlotCard(groupSlot.slot);
                groupSlot.timer+= (slotCard != null ? 1f : -1f) * Time.deltaTime / reduceDelay;
                groupSlot.timer = Mathf.Clamp01(groupSlot.timer);
                
                if(groupSlot.IsOccupied)
                    count++;
            }
            nbOccupied = count;
        }

        private void UpdatePositions()
        {
            bool even = nbOccupied % 2 == 0;
            float offset = (nbOccupied / 2) * -spacing;
            if(even)
                offset += spacing / 2;
            int index = 0;
            foreach (GroupSlot slot in groupSlotsList)
            {
                if (slot.IsOccupied)
                {
                    slot.pos = transform.position+Vector3.right*(index*spacing+offset);
                    index++;
                }
                else
                {
                    slot.pos = transform.position+Vector3.right*(nbOccupied*spacing+offset);
                }
            }
        }
        
        public bool IsInGroup(Slot slot)
        {
            return IsInGroup(slot.x, slot.y, slot.p);
        }
        
        public bool IsInGroup(int x, int y)
        {
            Slot min = GetSlotMin();
            Slot max = GetSlotMax();
            return x >= min.x && x <= max.x && y >= min.y && y <= max.y;
        }
        
        public bool IsInGroup(int x, int y, int p)
        {
            Slot min = GetSlotMin();
            Slot max = GetSlotMax();
            return x >= min.x && x <= max.x && y >= min.y && y <= max.y && p >= min.p && p <= max.p;
        }
        
        public Slot GetSlotMin()
        {
            return GetSlot(xMin, y);
        }

        public Slot GetSlotMax()
        {
            return GetSlot(xMax, y);
        }
        
        //Find the actual slot coordinates of this board slot
        public Slot GetSlot(int x, int y)
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

        public override Slot GetSlot(Vector3 wpos)
        {
            GroupSlot nearest = null;
            float minDist = 99f;

            foreach (GroupSlot spos in groupSlotsList)
            {
                float dist = (spos.pos - wpos).magnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = spos;
                }
            }

            return nearest != null ? nearest.slot : Slot.None;
        }
        
        public virtual Slot GetSlotOccupied(Vector3 wpos)
        {
            GroupSlot nearest = null;
            float minDist = 99f;

            foreach (GroupSlot spos in groupSlotsList)
            {
                float dist = (spos.pos - wpos).magnitude;
                if (spos.IsOccupied && dist < minDist)
                {
                    minDist = dist;
                    nearest = spos;
                }
            }

            return nearest != null ? nearest.slot : Slot.None;
        }
        
        public override Card GetSlotCard(Vector3 wpos)
        {
            Game gdata = Gameclient.Get().GetGameData();
            Slot slot = GetSlotOccupied(wpos);
            if (slot != Slot.None)
                return gdata.GetSlotCard(slot);
            return null;
        }
        
        public override bool HasSlot(Slot slot)
        {
            return groupSlotsList.Any(spos => spos.slot == slot);
        }

        public override Vector3 GetPosition(Slot slot)
        {
            foreach (GroupSlot spos in groupSlotsList)
            {
                if (spos.slot == slot)
                    return spos.pos;
            }
            return transform.position;
        }

        public override Slot GetEmptySlot(Vector3 wpos)
        {
            foreach (GroupSlot slot in groupSlotsList)
            {
                if (!slot.IsOccupied)
                    return slot.slot;
            }
            return Slot.None;
        }
    }
    
    public class GroupSlot
    {
        public Slot slot;
        public Vector3 pos;
        public float timer;

        public bool IsOccupied => timer > 0.01f;
    }
}