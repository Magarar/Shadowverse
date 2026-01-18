using System.Collections.Generic;
using UI;

namespace Menu
{
    public class AdventurePanel:UIPanel
    {
        private List<LevelUI> levelUIs = new List<LevelUI>();
        
        private static AdventurePanel instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Start()
        {
            base.Start();
            levelUIs.AddRange(GetComponentsInChildren<LevelUI>());
        }

        private void RefreshLevels()
        {
            foreach (var levelUI in levelUIs)
            {
                levelUI.RefreshLevel();
            }
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshLevels();
        }
        
        public static AdventurePanel Get()
        {
            return instance;
        }
    }
}