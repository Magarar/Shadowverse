using TMPro;
using UnityEngine;

namespace FX
{
    /// <summary>
    /// Text number FX that appear when a card receives damage
    /// </summary>
    public class DamageFX:MonoBehaviour
    {
        public TextMeshProUGUI textValue;
        
        public void SetValue(int value)
        {
            if (textValue != null)
                textValue.text = value.ToString();
        }

        public void SetValue(string value)
        {
            if (textValue != null)
                textValue.text = value;
        }
    }
}