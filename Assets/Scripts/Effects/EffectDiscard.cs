using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectDiscard", menuName = "Effects/EffectDiscard")]
    public class EffectDiscard: EffectData
    {
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            logic.DrawDiscardCard(target, ability.value); //Discard first card of deck
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            logic.DiscardCard(target);
        }
    }
}