using System.Collections.Generic;
using Data;
using UnityEngine;

namespace GameClient
{
    ///<总结>
    ///TutoStep组不需要按顺序触发，组将在start_trigger条件下触发
    ///然后按顺序执行组内的所有TutoStep。
    ///</摘要>
    public class TutoStepGroup:MonoBehaviour
    {
        public int turnMin = 0;
        public int turnMax = 99;
        public TutoStartTrigger startTrigger;
        public CardData startTarget;
        public bool forced; //Must finish all TutoStep inside group before triggering another group
        
        private int step;
        private bool triggered = false;
        
        private static List<TutoStepGroup> groups = new List<TutoStepGroup>();
        
        void Awake()
        {
            step = transform.GetSiblingIndex();
            groups.Add(this);
        }
        
        public void SetTriggered()
        {
            triggered = true;
        }
        
        public static TutoStepGroup Get(TutoStartTrigger trigger, int turn)
        {
            foreach (TutoStepGroup s in groups)
            {
                if (s.startTrigger == trigger && !s.triggered)
                {
                    if(turn >= s.turnMin&& turn <= s.turnMax)
                        return s;
                }
            }
            return null;
        }
        
        public static TutoStepGroup Get(TutoStartTrigger trigger, CardData target, int turn)
        {
            foreach (TutoStepGroup s in groups)
            {
                if (s.startTrigger == trigger && !s.triggered)
                {
                    if (turn >= s.turnMin && turn <= s.turnMax)
                    {
                        if (s.startTarget == null || s.startTarget == target)
                            return s;
                    }
                }
            }
            return null;
        }
    }
}