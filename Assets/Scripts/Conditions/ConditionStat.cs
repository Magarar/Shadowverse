using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Compares basic card or player stats such as attack/hp/Mana
    /// </summary>
    ///
    [CreateAssetMenu(fileName = "New ConditionStat", menuName = "Conditions/ConditionStat")]

    public class ConditionStat: ConditionData
    {
        
        [Header("Card stat is")]
        public ConditionStatType type;
        public ConditionOperatorInt oper;
        public int value;
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            if (type == ConditionStatType.Attack)
            {
                return CompareInt(target.GetAttack(), oper, value);
            }

            if (type == ConditionStatType.HP)
            {
                return CompareInt(target.GetHp(), oper, value);
            }

            if (type == ConditionStatType.Mana)
            {
                return CompareInt(target.GetMana(), oper, value);
            }

            return false;
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            if (type == ConditionStatType.HP)
            {
                return CompareInt(target.hp, oper, value);
            }

            if (type == ConditionStatType.Mana)
            {
                return CompareInt(target.Mana, oper, value);
            }

            return false;
        }
    }
}