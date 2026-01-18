using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Condition that checks the card data matches
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionCardData", menuName = "Conditions/ConditionCardData")]
    public class ConditionCardData :ConditionData
    {
        [Header("Card is")]
        public CardData cardType;

        public ConditionOperatorBool oper;
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            return CompareBool(target.cardId == cardType.id, oper);
        }
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            return false; //Not a card
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
        {
            return false; //Not a card
        }
    }
}
