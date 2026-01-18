using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    /// <summary>
    /// Effects that heals a card or player (hp)
    /// It cannot restore more than the original hp, use AddStats to go beyond original
    /// </summary>
    [CreateAssetMenu(fileName = "New EffectHeal", menuName = "Effects/EffectHeal")]

    public class EffectHeal: EffectData
    {
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            logic.HealPlayer(target, ability.value);
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            logic.HealCard(target, ability.value);
        }
    }
}