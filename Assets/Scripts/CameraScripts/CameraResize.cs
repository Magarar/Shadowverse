using UnityEngine;

namespace CameraScripts
{
    /// <summary>
    /// 将相机帧调整到支持的纵横比的脚本
    /// 默认情况下：仅支持16/9和16/10
    /// 如果窗户不同，侧面会出现黑条
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraResize : MonoBehaviour
    {
        private Camera cam;
        private int sheight;
        private int swidth;
        
        private void Start()
        {
            cam = GetComponent<Camera>();
            sheight = Screen.height;
            swidth = Screen.width;
            UpdateSize();
        }
        
        private void Update()
        {
            if (sheight != Screen.height || swidth != Screen.width)
            {
                sheight = Screen.height;
                swidth = Screen.width;
                UpdateSize();
            }
        }

        private void UpdateSize()
        {
            float screenRatio = Screen.width / (float)Screen.height;
            float targetRatio = GetAspectRatio();
            
            if (Mathf.Approximately(screenRatio, targetRatio))
            {
                // 屏幕或窗口是目标纵横比：使用整个区域。
                cam.rect = new Rect(0, 0, 1, 1);
            }
            else if (screenRatio > targetRatio)
            {
                // Screen or window is wider than the target: pillarbox.
                float normalizedWidth = targetRatio / screenRatio;
                float barThickness = (1f - normalizedWidth) / 2f;
                cam.rect = new Rect(barThickness, 0, normalizedWidth, 1);
            }
            else
            {
                // Screen or window is narrower than the target: letterbox.
                float normalizedHeight = screenRatio / targetRatio;
                float barThickness = (1f - normalizedHeight) / 2f;
                cam.rect = new Rect(0, barThickness, 1, normalizedHeight);
            }
        }
        
        public static float GetAspectMin()
        {
            float min = 16f / 10f;
            return min;
        }

        public static float GetAspectMax()
        {
            //bool allow_wide = TheGame.IsMobile() && TheGame.Get() != null;
            //float max = allow_wide ? 16f / 8f : 16f / 9f;
            float max = 16f / 9f;
            return max;
        }
        
        public static float GetCamSizeMin()
        {
            //bool allow_wide = TheGame.IsMobile() && TheGame.Get() != null;
            //float max = allow_wide ? 4.2f : 4.5f;
            float max = 4.5f;
            return max;
        }

        public static float GetCamSizeMax()
        {
            return 5f;
        }
        
        public static float GetAspectRatio()
        {
            float max = GetAspectMax();
            float min = GetAspectMin();
            float screenRatio = Screen.width / (float)Screen.height;
            float targetRatio = Mathf.Clamp(screenRatio, min, max);
            return targetRatio;
        }

        public static float GetAspectValue()
        {
            float max = GetAspectMax();
            float min = GetAspectMin();
            float aspect = GetAspectRatio();
            float value = (aspect - min) / (max - min);
            return value;
        }
        
    }
}
