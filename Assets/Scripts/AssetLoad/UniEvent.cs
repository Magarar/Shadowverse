using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLoad
{
    public class UniEvent
    {
        private class PostWrapper
        {
            public int PostFrame;
            public int EventID;
            public IEventMessage Message;

            public void OnRelease()
            {
                PostFrame = 0;
                EventID = 0;
                Message = null;
            }
        }
        
        private static bool isInitialize = false;
        private static GameObject driver = null;
        private static readonly Dictionary<int, LinkedList<Action<IEventMessage>>> listeners = new Dictionary<int, LinkedList<Action<IEventMessage>>>(1000);
        private static readonly List<PostWrapper> postingList = new List<PostWrapper>(1000);
        
        /// <summary>
        /// 初始化事件系统
        /// </summary>
        public static void Initalize()
        {
            if (isInitialize)
                throw new Exception($"{nameof(UniEvent)} is initialized !");

            if (isInitialize == false)
            {
                // 创建驱动器
                isInitialize = true;
                driver = new UnityEngine.GameObject($"[{nameof(UniEvent)}]");
                driver.AddComponent<UniEventDriver>();
                UnityEngine.Object.DontDestroyOnLoad(driver);
                Debug.Log($"{nameof(UniEvent)} initalize !");
            }
        }
        
        public static void Destroy()
        {
            if (isInitialize)
            {
                ClearAll();

                isInitialize = false;
                if (driver != null)
                    GameObject.Destroy(driver);
                Debug.Log($"{nameof(UniEvent)} destroy all !");
            }
        }
        
        /// <summary>
        /// 更新事件系统
        /// </summary>
        internal static void Update()
        {
            for (int i = postingList.Count - 1; i >= 0; i--)
            {
                var wrapper = postingList[i];
                if (UnityEngine.Time.frameCount > wrapper.PostFrame)
                {
                    SendMessage(wrapper.EventID, wrapper.Message);
                    postingList.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// 清空所有监听
        /// </summary>
        public static void ClearAll()
        {
            foreach (int eventId in listeners.Keys)
            {
                listeners[eventId].Clear();
            }
            listeners.Clear();
            postingList.Clear();
        }
        
        /// <summary>
        /// 添加监听
        /// </summary>
        public static void AddListener<TEvent>(System.Action<IEventMessage> listener) where TEvent : IEventMessage
        {
            System.Type eventType = typeof(TEvent);
            int eventId = eventType.GetHashCode();
            AddListener(eventId, listener);
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        public static void AddListener(System.Type eventType, System.Action<IEventMessage> listener)
        {
            int eventId = eventType.GetHashCode();
            AddListener(eventId, listener);
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        public static void AddListener(int eventId, System.Action<IEventMessage> listener)
        {
            if (listeners.ContainsKey(eventId) == false)
                listeners.Add(eventId, new LinkedList<Action<IEventMessage>>());
            if (listeners[eventId].Contains(listener) == false)
                listeners[eventId].AddLast(listener);
        }
        
        /// <summary>
        /// 移除监听
        /// </summary>
        public static void RemoveListener<TEvent>(System.Action<IEventMessage> listener) where TEvent : IEventMessage
        {
            System.Type eventType = typeof(TEvent);
            int eventId = eventType.GetHashCode();
            RemoveListener(eventId, listener);
        }

        /// <summary>
        /// 移除监听
        /// </summary>
        public static void RemoveListener(System.Type eventType, System.Action<IEventMessage> listener)
        {
            int eventId = eventType.GetHashCode();
            RemoveListener(eventId, listener);
        }

        /// <summary>
        /// 移除监听
        /// </summary>
        public static void RemoveListener(int eventId, System.Action<IEventMessage> listener)
        {
            if (listeners.ContainsKey(eventId))
            {
                if (listeners[eventId].Contains(listener))
                    listeners[eventId].Remove(listener);
            }
        }
        
        /// <summary>
        /// 实时广播事件
        /// </summary>
        public static void SendMessage(IEventMessage message)
        {
            int eventId = message.GetType().GetHashCode();
            SendMessage(eventId, message);
        }

        /// <summary>
        /// 实时广播事件
        /// </summary>
        public static void SendMessage(int eventId, IEventMessage message)
        {
            if (UniEvent.listeners.ContainsKey(eventId) == false)
                return;

            LinkedList<Action<IEventMessage>> listeners = UniEvent.listeners[eventId];
            if (listeners.Count > 0)
            {
                var currentNode = listeners.Last;
                while (currentNode != null)
                {
                    currentNode.Value.Invoke(message);
                    currentNode = currentNode.Previous;
                }
            }
        }
        
        public static void PostMessage(IEventMessage message)
        {
            int eventId = message.GetType().GetHashCode();
            PostMessage(eventId, message);
        }

        /// <summary>
        /// 延迟广播事件
        /// </summary>
        public static void PostMessage(int eventId, IEventMessage message)
        {
            var wrapper = new PostWrapper();
            wrapper.PostFrame = UnityEngine.Time.frameCount;
            wrapper.EventID = eventId;
            wrapper.Message = message;
            postingList.Add(wrapper);
        }
    }
    
    
}