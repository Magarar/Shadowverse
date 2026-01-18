using System.Collections.Generic;
using Data;
using GameLogic;

namespace UI
{
    public class SelectorPanel:UIPanel
    {
        private static List<SelectorPanel> panelList = new List<SelectorPanel>();
        
        protected override void Awake()
        {
            base.Awake();
            panelList.Add(this);
        }
        
        protected virtual void OnDestroy()
        {
            panelList.Remove(this);
        }
        
        public virtual void Show(AbilityData ability, Card card)
        {
            //Override this to show panel
        }
        
        public virtual bool ShouldShow()
        {
            return false; //Override this function, when this panel should show
        }
        
        public static List<SelectorPanel> GetAll()
        {
            return panelList;
        }
        
        public static void HideAll()
        {
            foreach (SelectorPanel panel in panelList)
            {
                if(panel.IsVisible())
                    panel.Hide();
            }
        }
        
    }
}