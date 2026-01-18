using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// UI element that shows a value with a progress bar
    /// </summary>
    public class ProgressBar: MonoBehaviour
    {
        public float value;
        public float valueMax;
        
        public Image fill;
        
        void Update()
        {
            float ratio = value / Mathf.Max(valueMax, 0.01f);
            fill.fillAmount = ratio;
        }
    }
}