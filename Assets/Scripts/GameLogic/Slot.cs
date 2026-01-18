using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace GameLogic
{
    /// <summary>
    /// Represent a slot in gameplay (data only)
    /// </summary>
    [Serializable]
    public struct Slot:INetworkSerializable
    {
        public int x; //From 1 to 5
        public int y; //Not in use, could be used to add more rows or different locations on the board
        public int p;//0 or 1, represent player ID

        public static int xMin = 1;//Dont change this, should start at 1  (0,0,0 represent invalid slot)
        public static int xMax = 5; //Number of slots in a row/zone
        
        public static int yMin = 1; //Dont change this, should start at 1  (0,0,0 represent invalid slot)
        public static int yMax = 1; //Set this to the number of rows/locations you want to have
        
        public static bool ignoreP = false; //Set to true if you dont want to use P value
        
        private static Dictionary<int, List<Slot>> playerSlots = new Dictionary<int, List<Slot>>();
        private static List<Slot> allSlots = new List<Slot>();
        
        public Slot(int pid)
        {
            this.x = 0;
            this.y = 0;
            this.p = pid;
        }

        public Slot(int x, int y, int p)
        {
            this.x = x;
            this.y = y;
            this.p = p;
        }
        
        public Slot(SlotXY slot, int pid)
        {
            this.x = slot.x;
            this.y = slot.y;
            this.p = pid;
        }
        

        public bool IsInRangeX(Slot slot, int range)
        {
            return Mathf.Abs(x - slot.x) <= range;
        }
        
        public bool IsInRangeY(Slot slot, int range)
        {
            return Mathf.Abs(y - slot.y) <= range;
        }
        
        public bool IsInRangeP(Slot slot, int range)
        {
            return Mathf.Abs(p - slot.p) <= range;
        }
        
        //No Diagonal, Diagonal = 2 dist
        public bool IsInDistanceStraight(Slot slot, int dist)
        {
            int r = Mathf.Abs(x - slot.x) + Mathf.Abs(y - slot.y) + Mathf.Abs(p - slot.p);
            return r <= dist;
        }
        
        public bool IsInDistance(Slot slot, int dist)
        {
            int dx = Mathf.Abs(x - slot.x);
            int dy = Mathf.Abs(y - slot.y);
            int dp = Mathf.Abs(p - slot.p);
            return dx <= dist && dy <= dist && dp <= dist;
        }
        
        public bool IsPlayerSlot()
        {
            return x == 0 && y == 0;
        }

        public bool IsValid()
        {
            return x >= xMin && x <= xMax && y >= yMin && y <= yMax&&p >= 0;
        }
        
        public static int MaxP => ignoreP? 0 : 1;
        
        public static int GetP(int pid)
        {
            return ignoreP ? 0 : pid;
        }
        
        //Get a random slot on player side
        public static Slot GetRandom(int pid, Random rand)
        {
            int p = GetP(pid);
            if(yMax>yMin)
                return new Slot(rand.Next(xMin, xMax + 1), rand.Next(yMin, yMax + 1), p);
            return new Slot(rand.Next(xMin, xMax + 1), yMin, p);
        }
        
        //Get a random slot amongts all valid ones
        public static Slot GetRandom(Random rand)
        {
            if(yMax>yMin)
                return new Slot(rand.Next(xMin, xMax + 1), rand.Next(yMin, yMax + 1), rand.Next(0, 2));
            return new Slot(rand.Next(xMin, xMax + 1), yMin, rand.Next(0, 2));
        }

        public static Slot Get(int x, int y, int p)
        {
            List<Slot> slots = GetAll();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].x == x && slots[i].y == y && slots[i].p == p)
                    return slots[i];
            }
            return new Slot(x, y, p);
        }

        public static List<Slot> GetAll(int pid)
        {
            int p = GetP(pid);
            if(playerSlots.TryGetValue(p, out var s))
                return s;
            List<Slot> list = new List<Slot>();
            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                    list.Add(new Slot(x, y, p));
            }
            playerSlots.Add(p, list);
            return list;
        }

        public static List<Slot> GetAll()
        {
            if(allSlots.Count>0)
                return allSlots;
            for (int p = 0; p <= MaxP; p++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int y = yMin; y <= yMax; y++)
                    {
                        allSlots.Add(new Slot(x, y, p));
                    }
                }
            }
            return allSlots;
        }

        public static bool operator ==(Slot a, Slot b)
        {
            return a.x == b.x && a.y == b.y && a.p == b.p;
        }


        public static bool operator !=(Slot a, Slot b)
        {
            return a.x != b.x || a.y != b.y || a.p != b.p;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref y);
            serializer.SerializeValue(ref p);
        }

        public override string ToString()
        {
            return $"{x},{y},{p}";
        }

        public static Slot None => new(0, 0, 0);
    }
    
    [Serializable]
    public struct SlotXY
    {
        public int x;
        public int y;
    }
}