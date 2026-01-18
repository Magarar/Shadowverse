using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UI
{
    /// <summary>
    /// Select a value between min and max
    /// </summary>
    public class NumberSelector:SelectorPanel
    {
        [Header("Options")]
        public int value;
        public int valueMin;
        public int valueMax;
        
        [Header("Display")]
        public TextMeshProUGUI selectText;

        public UnityAction onChange;

        private bool isLocked = false;
        
        protected override void Start()
        {
            SetValue(0);
        }
        
        private void AfterChangeOption()
        {
            if (selectText != null)
                selectText.text = value.ToString();
            onChange?.Invoke();
        }
        
        public void OnClickLeft()
        {
            if (isLocked)
                return;

            value--;
            value = Mathf.Clamp(value, valueMin, valueMax);
            AfterChangeOption();
        }
        
        public void OnClickRight()
        {
            if (isLocked)
                return;

            value++;
            value = Mathf.Clamp(value, valueMin, valueMax);
            AfterChangeOption();
        }
        
        public void SetValue(int val)
        {
            value = Mathf.Clamp(val, valueMin, valueMax);

            if (selectText != null)
                selectText.text = value.ToString();
        }
        
        public void SetMax(int max)
        {
            valueMax = max;
        }

        public void SetMin(int min)
        {
            valueMin = min;
        }

        public void SetLocked(bool locked)
        {
            isLocked = locked;
        }

   
    }
}