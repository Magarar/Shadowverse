using System;
using System.Collections.Generic;
using System.Reflection;
using GameLogic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// 所有能力条件的基类，覆盖IsConditionMet函数
    /// </summary>
    public class ConditionData:ScriptableObject
    {
        //Override this, applies to caster, always checked
        public virtual bool IsTriggerConditionMet(Game data, AbilityData abilityData, Card caster)
        {
            return true;
        }
        
        //Override this, condition targeting card
        public virtual bool IsTargetConditionMet(Game data, AbilityData abilityData, Card caster, Card target)
        {
            return true;
        }
        
        //Override this, condition targeting player
        public virtual bool IsTargetConditionMet(Game data, AbilityData abilityData, Card caster, Player target)
        {
            return true;
        }

        //Override this, condition targeting slot
        public virtual bool IsTargetConditionMet(Game data, AbilityData abilityData, Card caster, Slot target)
        {
            return true;
        }
        
        public virtual bool IsTargetConditionMet(Game data, AbilityData abilityData, Article caster, Slot target)
        {
            return true;
        }

        //Override this, for effects that create new cards
        public virtual bool IsTargetConditionMet(Game data, AbilityData abilityData, Card caster, CardData target)
        {
            return true;
        }
        
        public virtual bool IsTriggerConditionMet(Game data, AbilityData abilityData, Article caster)
        {
            return true;
        }
        
        public virtual bool IsTargetConditionMet(Game data, AbilityData abilityData, Article caster, Article target)
        {
            return true;
        }
        
        public virtual bool IsTargetConditionMet(Game data, AbilityData abilityData, Article caster, Card target)
        {
            return true;
        }
        
        public virtual bool IsTargetConditionMet(Game data, AbilityData abilityData, Article caster, Player target)
        {
            return true;
        }
        
        public virtual bool IsTargetConditionMet(Game data, AbilityData abilityData, Article caster, CardData target)
        {
            return true;
        }

        public bool CompareBool(bool cond, ConditionOperatorBool op)
        {
            if (op == ConditionOperatorBool.IsFalse)
                return !cond;
            return cond;
        }
        
        public bool CompareInt(int ival1, ConditionOperatorInt oper, int ival2)
        {
            if (oper == ConditionOperatorInt.Equal)
            {
                return ival1 == ival2;
            }
            if (oper == ConditionOperatorInt.NotEqual)
            {
                return ival1 != ival2;
            }
            if (oper == ConditionOperatorInt.GreaterEqual)
            {
                return ival1 >= ival2;
            }
            if (oper == ConditionOperatorInt.LessEqual)
            {
                return ival1 <= ival2;
            }
            if (oper == ConditionOperatorInt.Greater)
            {
                return ival1 > ival2;
            }
            if (oper == ConditionOperatorInt.Less)
            {
                return ival1 < ival2; ;
            }
            return false;
        }
    
        
        public enum ConditionOperatorInt
        {
            Equal,
            NotEqual,
            GreaterEqual,
            LessEqual,
            Greater,
            Less,
        }
        
        public enum ConditionOperatorBool
        {
            IsTrue,
            IsFalse,
        }
    }

   
}