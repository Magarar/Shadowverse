using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Box that appears when hovering a HoverTarget
    /// </summary>
    public class HoverTextBox:MonoBehaviour
    {
        public UIPanel panelLeft;
        public UIPanel panelRight;
        public TextMeshProUGUI text1;
        public TextMeshProUGUI text2;

        private HoverTarget current;
        private HoverTargetUI currentUI;
        
        private RectTransform rectLeft;
        private RectTransform rectRight;
        
        private static HoverTextBox instance;

        private void Awake()
        {
            instance = this;
            rectLeft = panelLeft.GetComponent<RectTransform>();
            rectRight = panelRight.GetComponent<RectTransform>();
        }

        void Update()
        {
            if (current != null || currentUI != null)
            {
                transform.position = GameUI.MouseToWorld(Input.mousePosition);
                transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector2.up);
                panelLeft.SetVisible(transform.position.x > 0f);
                panelRight.SetVisible(transform.position.x <= 0f);
                
                if (current != null && !current.IsHover())
                    Hide();
                if (currentUI != null && !currentUI.IsHover())
                    Hide();
            }
        }

        public void Show(HoverTarget hover)
        {
            current = hover;
            currentUI = null;
            text1.text = hover.GetText();
            text2.text = hover.GetText();
            text1.fontSize = hover.textSize;
            text2.fontSize = hover.textSize;
            rectLeft.sizeDelta = new Vector2(hover.width, hover.height);
            rectRight.sizeDelta = new Vector2(hover.width, hover.height);
        }

        public void Show(HoverTargetUI hover)
        {
            current = null;
            currentUI = hover;
            text1.text = hover.GetText();
            text2.text = hover.GetText();
            text1.fontSize = hover.textSize;
            text2.fontSize = hover.textSize;
            rectLeft.sizeDelta = new Vector2(hover.width, hover.height);
            rectRight.sizeDelta = new Vector2(hover.width, hover.height);
        }
        
        public void Hide()
        {
            current = null;
            currentUI = null;
            panelLeft.Hide();
            panelRight.Hide();
        }

        public static HoverTextBox Get()
        {
            return instance;
        }
        
    }
}