using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// A toggle button that let you change tab panel
    /// </summary>
    public class TabButton: MonoBehaviour
    {
        public string group;
        public bool active;
        public GameObject highlight;
        public UIPanel uiPanel;

        public UnityAction onClick;
        public static UnityAction<TabButton> onClickAny;
        
        private static List<TabButton> tabList = new List<TabButton>();
        
        private void Awake()
        {
            tabList.Add(this);
        }

        private void OnDestroy()
        {
            tabList.Remove(this);
        }
        
        void Start()
        {
            Button button = GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(OnClick);

            if (active && uiPanel != null)
                uiPanel.Show();
        }
        
        void Update()
        {
            if (highlight != null)
                highlight.SetActive(active);
        }

        private void OnClick()
        {
            Activate();
            onClick?.Invoke();
            onClickAny?.Invoke(this);
        }
        
        public void Activate()
        {
            SetAll(group, false);
            active = true;
            if (uiPanel != null)
                uiPanel.Show();
        }
        
        public void Deactivate()
        {
            active = false;
            if (uiPanel != null)
                uiPanel.Hide();
        }
        
        public bool IsActive()
        {
            return active;
        }
        
        public static void SetAll(string group, bool act)
        {
            foreach (var btn in tabList.Where(btn => btn.group == group))
            {
                btn.active = act;
                if(btn.uiPanel != null)
                    btn.uiPanel.SetVisible(act);
            }
        }

        public static List<TabButton> GetAll(string group)
        {
            return tabList.Where(btn => btn.group == group).ToList();
        }
        
        public static List<TabButton> GetAll()
        {
            return tabList;
        }
    }
}