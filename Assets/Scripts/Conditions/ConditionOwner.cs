using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{  
    /// <summary>
    /// Condition that check the owner of the target match the owner of the caster
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionOwner", menuName = "Conditions/ConditionOwner")]
    public class ConditionOwner : ConditionData
    {
        [Header("Target owner is caster owner")]
        public ConditionOperatorBool oper;
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            bool sameOwner = caster.playerID == target.playerID;
            return CompareBool(sameOwner, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            bool sameOwner = caster.playerID == target.id;
            return CompareBool(sameOwner, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
        {
            bool sameOwner = Slot.GetP(caster.playerID) == target.p;
            return CompareBool(sameOwner, oper);
        }
    }
}