using System;
using System.Collections.Generic;
using Data;
using GameLogic;
using UnityEngine;

namespace Unit
{
    ///<总结>
    ///逐一解决能力和行动，每个之间有可选的延迟
    ///</摘要>
    public class ResolveQueue
    {
        private Pool<AbilityQueueElement> abilityElemPool = new();
        private Pool<AttackQueueElement> attackElemPool = new();
        private Pool<SecretQueueElement> secretElemPool = new();
        private Pool<CallbackQueueElement> callbackElemPool = new();
        
        private Queue<AbilityQueueElement> abilityQueue = new();
        private Queue<AttackQueueElement> attackQueue = new();
        private Queue<SecretQueueElement> secretQueue = new();
        private Queue<CallbackQueueElement> callbackQueue = new();

        private Game gameData;
        private bool isResolving = false;
        private float resolveDelay = 0f;
        private bool skipDelay = false;

        public ResolveQueue(Game data, bool skip)
        {
            gameData = data;
            skipDelay = skip;
        }

        public void SetData(Game data)
        {
            gameData = data;
        }

        public virtual void Update(float delta)
        {
            if (resolveDelay > 0f)
            {
                resolveDelay -= delta;
                if (resolveDelay <= 0f)
                    ResolveAll();
            }
        }

        public virtual void AddAbility(AbilityData abilityData, Card caster, Card trigger,
            Action<AbilityData, Card, Card> callback)
        {
            if (abilityData != null && caster != null)
            {
                AbilityQueueElement elem = abilityElemPool.Create();
                elem.abilityData = abilityData;
                elem.caster = caster;
                elem.trigger = trigger;
                elem.callback = callback;
                abilityQueue.Enqueue(elem);
            }
        }
        

        public virtual void AddAttack(Card attacker, Card target, Action<Card, Card, bool> callback,
            bool skipCost = false)
        {
            if (attacker != null && target != null)
            {
                AttackQueueElement elem = attackElemPool.Create();
                elem.attacker = attacker;
                elem.target = target;
                elem.ptarget = null;
                elem.skipCost = skipCost;
                elem.callback = callback;
                attackQueue.Enqueue(elem);
            }
        }
        
        public virtual void AddAttack(Card attacker, Player target, Action<Card, Player, bool> callback, bool skipCost = false)
        {
            if (attacker != null && target != null)
            {
                AttackQueueElement elem = attackElemPool.Create();
                elem.attacker = attacker;
                elem.target = null;
                elem.ptarget = target;
                elem.skipCost = skipCost;
                elem.pcallback = callback;
                attackQueue.Enqueue(elem);
            }
        }
        
        public virtual void AddSecret(AbilityTrigger secretTrigger, Card secret, Card trigger, Action<AbilityTrigger, Card, Card> callback)
        {
            if (secret != null && trigger != null)
            {
                SecretQueueElement elem = secretElemPool.Create();
                elem.secretTrigger = secretTrigger;
                elem.secret = secret;
                elem.trigger = trigger;
                elem.callback = callback;
                secretQueue.Enqueue(elem);
            }
        }

        public virtual void AddCallback(Action callback)
        {
            if (callback != null)
            {
                CallbackQueueElement elem = callbackElemPool.Create();
                elem.callback = callback;
                callbackQueue.Enqueue(elem);
            }
        }

        public virtual void Resolve()
        {
            if (abilityQueue.Count > 0)
            {
                AbilityQueueElement elem = abilityQueue.Dequeue();
                abilityElemPool.Dispose(elem);
                elem.callback?.Invoke(elem.abilityData, elem.caster, elem.trigger);
            }
            else if (secretQueue.Count > 0)
            {
                SecretQueueElement elem = secretQueue.Dequeue();
                secretElemPool.Dispose(elem);
                elem.callback?.Invoke(elem.secretTrigger, elem.secret, elem.trigger);
            }else if (attackQueue.Count > 0)
            {
                AttackQueueElement elem = attackQueue.Dequeue();
                attackElemPool.Dispose(elem);
                if (elem.ptarget != null)
                    elem.pcallback?.Invoke(elem.attacker, elem.ptarget, elem.skipCost);
                else
                    elem.callback?.Invoke(elem.attacker, elem.target, elem.skipCost);
            }else if (callbackQueue.Count > 0)
            {
                CallbackQueueElement elem = callbackQueue.Dequeue();
                callbackElemPool.Dispose(elem);
                elem.callback?.Invoke();
            }
        }

        public virtual void ResolveAll(float delay)
        {
            SetDelay(delay);
            ResolveAll();
        }
        
        public virtual void ResolveAll()
        {
            if(isResolving)
                return;
            isResolving = true;
            while(CanResolve())
                Resolve();
            isResolving = false;
        }

        private bool CanResolve()
        {
            if(resolveDelay>0f)
                return false;//Is waiting delay
            if(gameData.state == GameState.GameEnded)
                return false;//Cant execute anymore when game is ended
            if(gameData.selector!=SelectorType.None)
                return false;//Waiting for player input, in the middle of resolve loop
            return abilityQueue.Count > 0 || secretQueue.Count > 0 || attackQueue.Count > 0 || callbackQueue.Count > 0;
        }

        public void SetDelay(float delay)
        {
            if(!skipDelay)
                resolveDelay = Mathf.Max(resolveDelay, delay);
        }

        public virtual bool IsResolving()
        {
            return isResolving || resolveDelay > 0f;
        }
        
        public virtual void Clear()
        {
            attackElemPool.DisposeAll();
            abilityElemPool.DisposeAll();
            secretElemPool.DisposeAll();
            callbackElemPool.DisposeAll();
            abilityQueue.Clear();
            attackQueue.Clear();
            secretQueue.Clear();
            callbackQueue.Clear();
        }
        
        public Queue<AbilityQueueElement> GetAbilityQueue() => abilityQueue;
        public Queue<AttackQueueElement> GetAttackQueue() => attackQueue;
        public Queue<SecretQueueElement> GetSecretQueue() => secretQueue;
        public Queue<CallbackQueueElement> GetCallbackQueue() => callbackQueue;
    }

    public class AbilityQueueElement
    {
        public AbilityData abilityData;
        public Card caster;
        public Card trigger;
        public Action<AbilityData,Card,Card> callback;
    }
    
    
    public class AttackQueueElement
    {
        public Card attacker;
        public Card target;
        public Player ptarget;
        public bool skipCost;
        public Action<Card, Card, bool> callback;
        public Action<Card, Player, bool> pcallback;
    }
    
    public class SecretQueueElement
    {
        public AbilityTrigger secretTrigger;
        public Card secret;
        public Card trigger;
        public Action<AbilityTrigger, Card, Card> callback;
    }
    
    public class CallbackQueueElement
    {
        public Action callback;
    }
}