using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Condition that check if the card is exhausted or not
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionExhaust", menuName = "Conditions/ConditionExhaust")]
    public class ConditionExhaust: ConditionData
    {
        [Header("Target is exhausted")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            return CompareBool(target.exhausted, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            return CompareBool(false, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
        {
            return CompareBool(false, oper);
        }
    }
}