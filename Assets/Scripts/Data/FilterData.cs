using System;
using System.Collections.Generic;
using System.Reflection;
using GameLogic;
using UnityEngine;

namespace Data
{
    ///<总结>
    ///目标筛选器的基类
    ///让您在目标已经由条件选择后但在应用效果之前对其进行过滤
    ///</摘要>
    [CreateAssetMenu(fileName = "New Filter", menuName = "Data/Filter")]
    public class FilterData:ScriptableObject
    {
        public virtual List<Card> FilterTargets(Game data, AbilityData abilityData, Card caster, List<Card> source, List<Card> dest)
        {
            return source;
        }

        public virtual List<Player> FilterTargets(Game data, AbilityData abilityData, Card caster, List<Player> source, List<Player> dest)
        {
            return source;
        }

        public virtual List<Slot> FilterTargets(Game data, AbilityData abilityData, Card caster, List<Slot> source, List<Slot> dest)
        {
            return source;
        }

        public virtual List<CardData> FilterTargets(Game data, AbilityData abilityData, Card caster, List<CardData> source, List<CardData> dest)
        {
            return source;
        }
        
        //Articles
        public virtual List<Card> FilterTargets(Game data, AbilityData abilityData, Article caster, List<Card> source, List<Card> dest)
        {
            return source;
        }

        public virtual List<Player> FilterTargets(Game data, AbilityData abilityData, Article caster, List<Player> source, List<Player> dest)
        {
            return source;
        }

        public virtual List<Slot> FilterTargets(Game data, AbilityData abilityData, Article caster, List<Slot> source, List<Slot> dest)
        {
            return source;
        }

        public virtual List<CardData> FilterTargets(Game data, AbilityData abilityData, Article caster, List<CardData> source, List<CardData> dest)
        {
            return source;
        }
    }
    
   
}