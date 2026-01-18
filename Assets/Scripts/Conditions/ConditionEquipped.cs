using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Condition that check if the card is equipped with an equipment card
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionEquipped", menuName = "Conditions/ConditionEquipped")]
    public class ConditionEquipped : ConditionData
    {
        [Header("Target is equipped")]
        public ConditionOperatorBool oper;
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            return CompareBool(target.equippedUid != null, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            return false;
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
        {
            return false;
        }
    }
}