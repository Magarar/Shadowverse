using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectExhaust", menuName = "Effects/Exhaust")]
    public class EffectExhaust: EffectData
    {
        public bool exhausted;

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            target.exhausted = exhausted;
        }
    }
}