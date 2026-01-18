using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Condition that check if the target is the same as the caster
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionSelf", menuName = "Conditions/ConditionSelf")]
    public class ConditionSelf: ConditionData
    {
        [Header("Target is caster")]
        public ConditionOperatorBool oper;
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            return CompareBool(caster == target, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            bool sameOwner = caster.playerID == target.id;
            return CompareBool(sameOwner, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
        {
            return CompareBool(caster.slot == target, oper);
        }
    }
}