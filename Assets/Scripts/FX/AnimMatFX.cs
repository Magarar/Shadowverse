using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FX
{
    public class AnimMatFX:MonoBehaviour
    {
        public Material target;
        private float timer;
        
        private float startValue;
        private float currentValue;
        
        private AnimMatAction current = null;
        private Queue<AnimMatAction> sequence = new Queue<AnimMatAction>();

        void Update()
        {
            if (target == null)
                return;

            if (current == null && sequence.Count > 0)
            {
                current = sequence.Dequeue();
                startValue = target.GetFloat(current.targetName);
                currentValue = startValue;
                timer = 0f;
                
            }

            if (current != null)
            {
                if (timer < current.duration)
                {
                    timer += Time.deltaTime;
                    if (current.type == AnimMatActionType.Float)
                    {
                        float dist = Mathf.Abs(current.targetValue - startValue);
                        float speed = dist / Mathf.Max(current.duration, 0.01f);
                        currentValue = Mathf.MoveTowards(currentValue, current.targetValue, speed * Time.deltaTime);
                        target.SetFloat(current.targetName, currentValue);
                    }
                }
                else
                {
                    current.callback?.Invoke();
                    current = null;
                }
            }
        }
        
        public void SetFloat(string name, float value, float duration)
        {
            AnimMatAction action = new AnimMatAction();
            action.type = AnimMatActionType.Float;
            action.duration = duration;
            action.targetName = name;
            action.targetValue = value;
            sequence.Enqueue(action);
        }

        public void Callback(float duration, UnityAction callback)
        {
            AnimMatAction action = new AnimMatAction();
            action.type = AnimMatActionType.None;
            action.duration = duration;
            action.callback = callback;
            sequence.Enqueue(action);
        }

        public void Clear()
        {
            target = null;
            timer = 0f;
            sequence.Clear();
        }
        
        public static AnimMatFX Create(GameObject obj, Material target)
        {
            AnimMatFX anim = obj.GetComponent<AnimMatFX>();
            if (anim == null)
                anim = obj.AddComponent<AnimMatFX>();

            anim.Clear();
            anim.target = target;
            return anim;
        }
    }
    
    public enum AnimMatActionType
    {
        None = 0,
        Float = 5,
    }

    public class AnimMatAction
    {
        public AnimMatActionType type;
        public string targetName;
        public float targetValue;
        public float duration = 1f;
        public UnityAction callback = null;
    }
}