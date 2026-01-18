using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLoad
{
    public class EventGroup
    {
        private readonly Dictionary<System.Type, List<Action<IEventMessage>>> cachedListener = new Dictionary<System.Type, List<Action<IEventMessage>>>();
        
        /// <summary>
        /// 添加一个监听
        /// </summary>
        public void AddListener<TEvent>(System.Action<IEventMessage> listener) where TEvent : IEventMessage
        {
            System.Type eventType = typeof(TEvent);
            if (cachedListener.ContainsKey(eventType) == false)
                cachedListener.Add(eventType, new List<Action<IEventMessage>>());

            if (cachedListener[eventType].Contains(listener) == false)
            {
                cachedListener[eventType].Add(listener);
                UniEvent.AddListener(eventType, listener);
            }
            else
            {
                Debug.LogWarning($"Event listener is exist : {eventType}");
            }
        }
        
        /// <summary>
        /// 移除所有缓存的监听
        /// </summary>
        public void RemoveAllListener()
        {
            foreach (var pair in cachedListener)
            {
                System.Type eventType = pair.Key;
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    UniEvent.RemoveListener(eventType, pair.Value[i]);
                }
                pair.Value.Clear();
            }
            cachedListener.Clear();
        }
    }
}