using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectSetTrait", menuName = "Effects/EffectSetTrait")]
    public class EffectSetTrait: EffectData
    {
        public TraitData trait;

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            target.SetTrait(trait.id, ability.value);
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            target.SetTrait(trait.id, ability.value);
        }

        public override void DoOngoingEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            target.SetTrait(trait.id, ability.value);
        }

        public override void DoOngoingEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            target.SetTrait(trait.id, ability.value);
        }
    }
}