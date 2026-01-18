using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    [CreateAssetMenu(fileName = "New ConditionRolled", menuName = "Conditions/ConditionRolled")]
    public class ConditionRolled: ConditionData
    {
        [Header("Value Rolled is")]
        public ConditionOperatorInt oper;
        public int value;
        
        public override bool IsTriggerConditionMet(Game data, AbilityData ability, Card caster)
        {
            return CompareInt(data.rolledValue, oper, value);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            return CompareInt(data.rolledValue, oper, value);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            return CompareInt(data.rolledValue, oper, value);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
        {
            return CompareInt(data.rolledValue, oper, value);
        }
    }
}