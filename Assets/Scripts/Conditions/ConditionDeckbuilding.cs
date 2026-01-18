using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
       /// <summary>
        /// Condition that check if the CardData is a valid deckbuilding card (not a summon token)
        /// </summary>
    [CreateAssetMenu(fileName = "New ConditionDeckbuilding", menuName = "Conditions/ConditionDeckbuilding")]
    public class ConditionDeckbuilding: ConditionData
    {
        [Header("Card is Deckbuilding")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            return CompareBool(target.CardData.deckbuilding, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, CardData target)
        {
            return CompareBool(target.deckbuilding, oper);
        }
    }
}