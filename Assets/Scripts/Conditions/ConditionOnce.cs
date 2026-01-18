using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Add this to an ability to prevent it from being cast more than once per turn
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionOnce", menuName = "Conditions/ConditionOnce")]
    public class ConditionOnce: ConditionData
    {
        public override bool IsTriggerConditionMet(Game data, AbilityData ability, Card caster)
        {
            return !data.abilityPlayed.Contains(ability.id);
        }
    }
}