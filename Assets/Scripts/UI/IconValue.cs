using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Icon that changes based on the value assigned
    /// </summary>
    public class IconValue: MonoBehaviour
    {
        public int value;
        public bool autoRefresh = true;

        public Sprite[] values;

        private Image image;
        
        void Awake()
        {
            image = GetComponent<Image>();
        }
        
        void Update()
        {
            if (autoRefresh)
                Refresh();
        }

        private void Refresh()
        {
            if (image == null)
                image = GetComponent<Image>();

            if (value >= 0 && value < values.Length)
            {
                image.sprite = values[value];
                image.enabled = image.sprite != null;
            }
        }
        
        public void SetMat(Material mat)
        {
            if (image == null)
                image = GetComponent<Image>();

            image.material = mat;
        }
    }
}