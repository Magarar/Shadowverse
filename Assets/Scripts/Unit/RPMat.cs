using UnityEngine;
using UnityEngine.UI;

namespace Unit
{
    /// <summary>
    /// Change material based on Render Pipeline
    /// </summary>
    public class RPMat:MonoBehaviour
    {
        public Material matUrp;
        
        public SpriteRenderer render;
        public Image image;
        
        void Start()
        {
            render = GetComponent<SpriteRenderer>();
            image = GetComponent<Image>();

            if (render != null && GameTool.IsURP())
                render.material = matUrp;
            if (image != null && GameTool.IsURP())
                image.material = matUrp;
        }
    }
}