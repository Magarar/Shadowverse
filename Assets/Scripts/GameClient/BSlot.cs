using System;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;

namespace GameClient
{
    /// <summary>
    /// Base class for BoardSlot, BoardSlotPlayer and BoardSlotGroup
    /// </summary>

    public class BSlot:MonoBehaviour
    {
        protected SpriteRenderer render;
        protected Collider collider;
        protected Bounds bounds;
        protected float startAlpha = 0f;
        protected float currentAlpha = 0f;
        protected float targetAlpha = 0f;
        
        private static List<BSlot> slotList = new List<BSlot>();

        protected virtual void Awake()
        {
            slotList.Add(this);
            render = GetComponent<SpriteRenderer>();
            collider = GetComponent<Collider>();
            startAlpha = render.color.a;
            render.color = new Color(render.color.r, render.color.g, render.color.b, 0);
            bounds = collider.bounds;
        }

        protected virtual void OnDestroy()
        {
            slotList.Remove(this);
        }

        protected virtual void Update()
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * 2f);
            render.color = new Color(render.color.r, render.color.g, render.color.b, currentAlpha);
        }

        public virtual Slot GetSlot()
        {
            return Slot.None;
        }
        
        public virtual Slot GetSlot(Vector3 wpos)
        {
            return GetSlot();
        }
        
        public virtual Slot GetEmptySlot(Vector3 wpos)
        {
            return GetSlot();
        }
        
        public virtual Slot GetRandomSlot(Player player)
        {
            return player.GetRandomEmptySlot(new System.Random());
        }
        
        public virtual Card GetSlotCard(Vector3 wpos)
        {
            Game gdata = Gameclient.Get().GetGameData();
            Slot slot = GetSlot(wpos);
            return gdata.GetSlotCard(slot);
        }
        
        public virtual Vector3 GetPosition(Slot slot)
        {
            return transform.position;
        }
        
        public virtual Player GetPlayer()
        {
            return null;
        }
        
        public virtual bool HasSlot(Slot slot)
        {
            Slot aslot = GetSlot();
            return aslot == slot;
        }
        
        public virtual bool IsPlayer()
        {
            Slot slot = GetSlot();
            return slot.x == 0 && slot.y == 0;
        }
        
        public virtual bool IsInside(Vector3 wpos)
        {
            return bounds.Contains(wpos);
        }
        
        public static BSlot GetNearest(Vector3 pos)
        {
            BSlot nearest = null;
            float minDist = 999f;
            foreach (BSlot slot in GetAll())
            {
                float dist = (slot.transform.position - pos).magnitude;
                if (slot.IsInside(pos) && dist < minDist)
                {
                    minDist = dist;
                    nearest = slot;
                }
            }
            return nearest;
        }

        public static BSlot GetRandom(Player player)
        {
            Slot slot = player.GetRandomEmptySlot(new System.Random());
            foreach (BSlot bSlot in GetAll())
            {
                if(bSlot.GetSlot()== slot)
                    return bSlot;
            }
            return null;
        }
        
        public static BSlot Get(Slot slot)
        {
            foreach (BSlot bslot in GetAll())
            {
                if (bslot.HasSlot(slot))
                    return bslot;
            }
            return null;
        }
        
        public static List<BSlot> GetAll()
        {
            return slotList;
        }

        public override string ToString()
        {
            return $"{GetSlot().ToString()}";
        }
    }
    
    public enum BoardSlotType
    {
        Fixed = 0,              //x,y,p = slot
        PlayerSelf = 5,         //p = client player id
        PlayerOpponent = 7,     //p = client's opponent player id
        FlipX = 10,              //p=0,   x=unchanged for first player,  x=reversed for second player
        FlipY = 11,              //p=0,   y=unchanged for first player,  y=reversed for second player
    }
}