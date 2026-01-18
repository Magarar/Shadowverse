using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace CameraScripts
{
    /// <summary>
    ///游戏相机具有一些有用的功能，如抖动
    ///或者将鼠标光线投射到世界/屏幕位置
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class GameCamera:MonoBehaviour
    {
        private static GameCamera instance;
        public static GameCamera GetInstance() => instance;
        
        [BoxGroup("Camera")]
        private Camera cam;
        private Vector3 startPos;
        
        [BoxGroup("Tween")]
        private Tween shakeTween;

        private void Awake()
        {
            instance = this;
            cam = GetComponent<Camera>();
            startPos = transform.position;
            
        }

        public void Update()
        {
            
        }
        
        [Button]
        public void Shake(float intensity = 1f, float duration = 0.5f)
        {
            shakeTween?.Kill();
            // 使用DOTween创建shake效果
            shakeTween = transform.DOShakePosition(duration, intensity * 0.1f, 20, 90f)
                .OnComplete(() => {
                    // 动画完成后重置位置
                    transform.position = startPos;
                });
        }

        public Vector2 MouseToPercent(Vector3 mousePos)
        {
            float x = mousePos.x / Screen.width;
            float y = mousePos.y / Screen.height;
            return new Vector2(x, y);
        }

        public Ray MouseToRay(Vector3 mousePos)
        {
            return cam.ScreenPointToRay(mousePos);
        }

        public Vector3 MouseToWorld(Vector3 mousePos, float distance = 10f)
        {
            Vector3 wpos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance));
            return wpos;
        }
        
        public static Camera GetCamera()
        {
            if (instance != null)
                return instance.cam;
            return null;
        }

        public static GameCamera Get()
        {
            return instance;
        }
        
        
    }
}