using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ToggleText: MonoBehaviour
    {
        public Color onColor = Color.yellow;
        public Color offColor = Color.white;
        
        private Toggle toggle;
        private TextMeshProUGUI toggleText;
        
        private bool previous = false;
        
        void Awake()
        {
            toggle = GetComponent<Toggle>();
            toggleText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        private void Start()
        {
            Refresh();
        }
        
        void Update()
        {
            if (previous != toggle.isOn)
                Refresh();
        }
        
        private void Refresh()
        {
            toggleText.color = toggle.isOn ? onColor : offColor;
            previous = toggle.isOn;
        }
    }
}