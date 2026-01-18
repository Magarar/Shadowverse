using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameClient
{
    /// <summary>
    /// A reference to a position on the board,
    /// It is currently being used by the PackOpen board
    /// </summary>
    public class BoardRef:MonoBehaviour
    {
        public BoardRefType type;
        public int index;
        public bool opponent;

        private static List<BoardRef> refList = new List<BoardRef>();

        void Awake()
        {
            refList.Add(this);
        }

        void OnDestroy()
        {
            refList.Remove(this);
        }

        public static BoardRef Get(BoardRefType type, bool opponent)
        {
            return refList.FirstOrDefault(refItem => refItem.type == type && refItem.opponent == opponent);
        }

        public static BoardRef Get(BoardRefType type, int index)
        {
            return refList.FirstOrDefault(refItem => refItem.type == type && refItem.index == index);
        }
    }
    
    public enum BoardRefType
    {
        None = 0,
        PackCard = 4,
    }
}