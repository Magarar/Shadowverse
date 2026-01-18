using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Compares cards or players custom stats
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionTrait", menuName = "Conditions/ConditionTrait")]
    public class ConditionTrait: ConditionData
    {
        [Header("Card stat is")]
        public TraitData trait;
        public ConditionOperatorInt oper;
        public int value;

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            return CompareInt(target.GetTraitValue(trait.id), oper, value);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            return CompareInt(target.GetTraitValue(trait.id), oper, value);
        }
    }
}