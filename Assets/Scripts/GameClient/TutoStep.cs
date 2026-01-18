using System.Collections.Generic;
using System.Linq;
using Data;
using UI;
using UnityEngine;

namespace GameClient
{
    //<总结>
    ///一个组内的所有步骤都将按顺序触发，当满足end_trigger或触发另一个组时，游戏将继续进行下一步
    ///</摘要>
    public class TutoStep:UIPanel
    {
        [Header("Tuto Step")]
        public TutoEndTrigger endTrigger;
        public CardData triggerTarget;
        public bool forced;                //Player MUST do the end_trigger action to proceed
        
        private TutoStepGroup group;
        private int step;
        private TutoBox tutoBox;
        
        private static List<TutoStep> steps = new List<TutoStep>();

        protected override void Awake()
        {
            base.Awake();
            step = transform.GetSiblingIndex();
            group = GetComponentInParent<TutoStepGroup>();
            tutoBox = GetComponentInChildren<TutoBox>();
            steps.Add(this);
        }

        protected override void Start()
        {
            base.Start();
            tutoBox.SetNextButton(endTrigger == TutoEndTrigger.Click);
        }
        
        protected override void OnDestroy()
        {
            steps.Remove(this);
        }
        
        public int GetStepIndex()
        {
            return step;
        }
        
        public static TutoStep Get(TutoStepGroup group, int step)
        {
            return steps.FirstOrDefault(s => s.group == group && s.step == step);
        }
        
        public static void HideAll()
        {
            foreach (TutoStep s in steps)
            {
                s.Hide();
            }
        }

        
        
    }
}