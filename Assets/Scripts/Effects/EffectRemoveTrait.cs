using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectRemoveTrait", menuName = "Effects/EffectRemoveTrait")]
    public class EffectRemoveTrait : EffectData
    {
        public TraitData trait;

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            target.RemoveTrait(trait.id);
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            target.RemoveTrait(trait.id);
        }
    }
}