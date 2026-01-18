using System;
using System.Collections.Generic;
using System.Reflection;
using GameLogic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Base class for all ability effects, override the IsConditionMet function
    /// </summary>
    public class EffectData:ScriptableObject
    {
        public virtual void DoEffect(Gamelogic logic, AbilityData abilityData, Card caster)
        {
            //Server side gameplay logic
        }


        public virtual void DoEffect(Gamelogic logic, AbilityData abilityData, Card caster, Card target)
        {
            //Server side gameplay logic
        }

        public virtual void DoEffect(Gamelogic logic, AbilityData abilityData, Card caster, Player target)
        {
            //Server side gameplay logic
        }

        public virtual void DoEffect(Gamelogic logic, AbilityData abilityData, Card caster, Slot target)
        {
            //Server side gameplay logic
        }

        public virtual void DoEffect(Gamelogic logic, AbilityData abilityData, Card caster, CardData target)
        {
            //Server side gameplay logic
        }

        public virtual void DoOngoingEffect(Gamelogic logic, AbilityData abilityData, Card caster, Card target)
        {
            //Ongoing effect only
        }

        public virtual void DoOngoingEffect(Gamelogic logic, AbilityData abilityData, Card caster, Player target)
        {
            //Ongoing effect only
        }
        
        public int AddOrSet(int originalVal, EffectOperatorInt oper, int addValue)
        {
            if (oper == EffectOperatorInt.Add)
                return originalVal + addValue;
            if (oper == EffectOperatorInt.Set)
                return addValue;
            return originalVal;
        }
    }
    
   
    
    public enum EffectOperatorInt
    {
        Add,
        Set,
    }
}