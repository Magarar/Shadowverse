using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Checks if a slot contains a card or not
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionSlotEmpty", menuName = "Conditions/ConditionSlotEmpty")]
    public class ConditionSlotEmpty: ConditionData
    {
        [Header("Slot Is Empty")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            return CompareBool(false, oper); //Target is not empty slot
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            return CompareBool(false, oper); //Target is not empty slot
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
        { 
            Card slotCard = data.GetSlotCard(target);
            return CompareBool(slotCard == null, oper);
        }
    }
}