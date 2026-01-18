using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Bar that contain multiple icons to represent a value
    /// Such as the Mana bar during the game
    /// </summary>
    public class IconBar : MonoBehaviour
    {
        public int value = 0;
        public int maxValue = 4;
        public bool autoRefresh = true;

        public Image[] icons;
        public Sprite spriteFull;
        public Sprite spriteEmpty;

        private void Update()
        {
            if (autoRefresh)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            int index = 0;
            foreach (Image icon in icons)
            {
                icon.gameObject.SetActive(index < value || index < maxValue);
                icon.sprite = (index < value) ? spriteFull : spriteEmpty;
                index++;
            }
        }
        
        public void SetMat(Material mat)
        {
            foreach (Image icon in icons)
            {
                icon.material = mat;
            }
        }
    }
}
