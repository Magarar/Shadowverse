using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectEvolve", menuName = "Effects/EffectEvolve")]
    public class EffectEvolve: EffectData
    {
        public override void DoEffect(Gamelogic logic, AbilityData abilityData, Card caster)
        {
            logic.EvolveCard(caster,false);
        }

        public override void DoEffect(Gamelogic logic, AbilityData abilityData, Card caster, Card target)
        {
            logic.EvolveCard(target,false);
        }
    }
}