using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    /// <summary>
    /// Base class for UI panels that can be hidden or shown, with a fade-in fade-out effect
    /// </summary>
    
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel:MonoBehaviour
    {
        public float displaySpeed = 4f;

        public UnityAction onShow;
        public UnityAction onHide;

        protected CanvasGroup canvasGroup;
        protected bool visible;
        
        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            visible = false;
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            float add = visible ? displaySpeed : -displaySpeed;
            float alpha = Mathf.Clamp01(canvasGroup.alpha + add * Time.deltaTime);
            canvasGroup.alpha = alpha;
            
            if(!visible&&alpha<=0.01f)
                AfterHide();
        }

        public virtual void Toggle(bool instant = false)
        {
            if (IsVisible())
                Hide(instant);
            else
            {
                Show(instant);
            }
        }
        
        public virtual void Show(bool instant = false)
        {
            visible = true;
            gameObject.SetActive(true);
            
            if (instant||displaySpeed<0.01f)
                canvasGroup.alpha = 1f;
            
            onShow?.Invoke();
        }
        
        public virtual void Hide(bool instant = false)
        {
            visible = false;
            if (instant||displaySpeed<0.01f)
                canvasGroup.alpha = 0f;
            
            onHide?.Invoke();
        }

        public void SetVisible(bool visi)
        {
            if(!visible&&visi)
                Show();
            else if(visible&&!visi)
                Hide();
        }

        public virtual void AfterHide()
        {
            gameObject.SetActive(false);
        }
        
        public virtual bool IsVisible()
        {
            return visible;
        }

        public bool IsFullyVisible()
        {
            return visible&&canvasGroup.alpha>=0.99f;
        }
        
        public float GetAlpha()
        {
            return canvasGroup.alpha;
        }



        protected virtual void OnDestroy()
        {
            
        }

        
        
        
        


    }
}