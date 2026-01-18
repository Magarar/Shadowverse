using UnityEngine;
using UnityEngine.Serialization;

namespace Unit
{
    /// <summary>
    /// Add to any UI element to make it resize on mobile (some buttons should be bigger on mobile)
    /// </summary>
    public class MobileResizeUI:MonoBehaviour
    {
        public Vector2 positionOffset;
        public float size = 1f;

        void Start()
        {
            if (GameTool.IsMobile())
            {
                RectTransform rect = GetComponent<RectTransform>();
                rect.anchoredPosition += positionOffset;
                transform.localScale = transform.localScale * size;
            }
        }
    }
}