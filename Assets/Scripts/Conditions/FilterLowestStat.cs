using System.Collections.Generic;
using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    [CreateAssetMenu(fileName = "New FilterLowestStat", menuName = "Filters/FilterLowestStat")]
    public class FilterLowestStat: FilterData
    {
        public ConditionStatType stat;
        
        public override List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
        {
            //Find lowest
            int lowest = 99999;
            foreach (Card card in source)
            {
                int stat = GetStat(card);
                if (stat < lowest)
                    lowest = stat;
            }

            //Add all lowest
            foreach (Card card in source)
            {
                int stat = GetStat(card);
                if (stat == lowest)
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