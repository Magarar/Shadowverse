using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    //Checks if a player or card has a status effect
    [CreateAssetMenu(fileName = "New ConditionStatus", menuName = "Conditions/ConditionStatus")]
    public class ConditionStatus: ConditionData
    {
        [Header("Card has status")]
        public StatusType hasStatus;
        public int value = 0;
        public ConditionOperatorBool oper;
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            bool hstatus = target.HasStatus(hasStatus) && target.GetStatusValue(hasStatus) >= value;
            return CompareBool(hstatus, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            bool hstatus = target.HasStatus(hasStatus) && target.GetStatusValue(hasStatus) >= value;
            return CompareBool(hstatus, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
        {
            Card card = data.GetSlotCard(target);
            if (card != null)
                return IsTargetConditionMet(data, ability, caster, card);
            return false;
        }
    }
}