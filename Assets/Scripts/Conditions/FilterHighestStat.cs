using System.Collections.Generic;
using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    //Pick all targets with the highest stat

    [CreateAssetMenu(fileName = "New FilterHighestStat", menuName = "Filters/FilterHighestStat")]
    public class FilterHighestStat: FilterData
    {
        public ConditionStatType stat;
        
        public override List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
        {
            //Find highest
            int highest = -999;
            foreach (Card card in source)
            {
                int stat = GetStat(card);
                if (stat > highest)
                    highest = stat;
            }

            //Add all highest
            foreach (Card card in source)
            {
                int stat = GetStat(card);
                if (stat == highest)
                    dest.Add(card);
            }

            return dest;
        }
        
        private int GetStat(Card card)
        {
            if (stat == ConditionStatType.Attack)
            {
                return card.GetAttack();
            }
            if (stat == ConditionStatType.HP)
            {
                return card.GetHp();
            }
            if (stat == ConditionStatType.Mana)
            {
                return card.GetMana();
            }
            return 0;
        }
    }
}