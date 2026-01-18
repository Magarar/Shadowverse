using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLoad
{
    public class StateMachine
    {
        private readonly Dictionary<string, System.Object> blackboard = new Dictionary<string, object>(100);
        private readonly Dictionary<string, IStateNode> nodes = new Dictionary<string, IStateNode>(100);
        private IStateNode curNode;
        private IStateNode preNode;
        
        /// <summary>
        /// 状态机持有者
        /// </summary>
        public System.Object Owner { private set; get; }
        
        /// <summary>
        /// 当前运行的节点名称
        /// </summary>
        public string CurrentNode => curNode != null ? curNode.GetType().FullName : string.Empty;
        
        /// <summary>
        /// 之前运行的节点名称
        /// </summary>
        public string PreviousNode => preNode != null ? preNode.GetType().FullName : string.Empty;
        
        private StateMachine() { }
        public StateMachine(System.Object owner)
        {
            Owner = owner;
        }
        
        public void Run<TNode>() where TNode : IStateNode
        {
            var nodeType = typeof(TNode);
            var nodeName = nodeType.FullName;
            Run(nodeName);
        }
        
        public void Run(Type entryNode)
        {
            var nodeName = entryNode.FullName;
            Run(nodeName);
        }
        
        public void Run(string entryNode)
        {
            curNode = TryGetNode(entryNode);
            preNode = curNode;

            if (curNode == null)
                throw new Exception($"Not found entry node: {entryNode }");

            curNode.OnEnter();
        }
        
        public void Update()
        {
            if (curNode != null)
                curNode.OnUpdate();
        }
        
        public void AddNode<TNode>() where TNode : IStateNode
        {
            var nodeType = typeof(TNode);
            var stateNode = Activator.CreateInstance(nodeType) as IStateNode;
            AddNode(stateNode);
        }
        
        public void AddNode(IStateNode stateNode)
        {
            if (stateNode == null)
                throw new ArgumentNullException();

            var nodeType = stateNode.GetType();
            var nodeName = nodeType.FullName;

            if (nodes.ContainsKey(nodeName) == false)
            {
                stateNode.OnCreate(this);
                nodes.Add(nodeName, stateNode);
            }
            else
            {
                Debug.LogError($"State node already existed : {nodeName}");
            }
        }
        
        /// <summary>
        /// 转换状态节点
        /// </summary>
        public void ChangeState<TNode>() where TNode : IStateNode
        {
            var nodeType = typeof(TNode);
            var nodeName = nodeType.FullName;
            ChangeState(nodeName);
        }
        
        public void ChangeState(Type nodeType)
        {
            var nodeName = nodeType.FullName;
            ChangeState(nodeName);
        }
        
        public void ChangeState(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
                throw new ArgumentNullException();

            IStateNode node = TryGetNode(nodeName);
            if (node == null)
            {
                Debug.LogError($"Can not found state node : {nodeName}");
                return;
            }

            Debug.Log($"{curNode.GetType().FullName} --> {node.GetType().FullName}");
            preNode = curNode;
            curNode.OnExit();
            curNode = node;
            curNode.OnEnter();
        }
        
        /// <summary>
        /// 设置黑板数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetBlackboardValue(string key, System.Object value)
        {
            blackboard[key] = value;
        }
        
        /// <summary>
        /// 获取黑板数据
        /// </summary>
        public System.Object GetBlackboardValue(string key)
        {
            if (blackboard.TryGetValue(key, out System.Object value))
            {
                return value;
            }
            else
            {
                Debug.Log($"Not found blackboard value : {key}");
                return null;
            }
        }
        
        private IStateNode TryGetNode(string nodeName)
        {
            nodes.TryGetValue(nodeName, out IStateNode result);
            return result;
        }
    }
}