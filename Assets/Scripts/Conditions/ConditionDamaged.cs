using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Check if a card/player is damaged
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionDamaged", menuName = "Conditions/ConditionDamaged")]
    public class ConditionDamaged: ConditionData
    {
        [Header("Card is damaged")]
        public ConditionOperatorBool oper;
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            return CompareBool(target.damaged > 0, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            return CompareBool(target.hp < target.hpMax, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
        {
            return false;
        }
    }
}