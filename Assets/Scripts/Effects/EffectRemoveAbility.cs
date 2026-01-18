using Data;
using GameLogic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Effects
{
    /// <summary>
    /// Effect that removes an ability from a card
    /// </summary>
    [CreateAssetMenu(fileName = "New EffectRemoveAbility", menuName = "Effects/EffectRemoveAbility")]
    public class EffectRemoveAbility: EffectData
    {
        public AbilityData removeAbility;

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            target.RemoveAbility(removeAbility);
        }
    }
}