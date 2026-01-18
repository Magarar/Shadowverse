using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectHalfMana", menuName = "Effects/EffectHalfMana")]
    public class EffectHalfMana: EffectData
    {
        public override void DoEffect(Gamelogic logic, AbilityData abilityData, Card caster, Card target)
        {
            base.DoEffect(logic, abilityData, caster, target);
            target.mana = target.mana / 2;
        }
    }
}