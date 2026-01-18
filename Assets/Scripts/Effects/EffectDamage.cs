using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    /// <summary>
    /// Effect that damages a card or a player (lose hp)
    /// </summary>
    [CreateAssetMenu(fileName = "New EffectDamage", menuName = "Effects/EffectDamage")]
    public class EffectDamage:EffectData
    {
        public TraitData bonusDamage;
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            int damage = GetDamage(logic.GameData, caster, ability.value);
            logic.DamagePlayer(caster, target, damage);
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            int damage = GetDamage(logic.GameData, caster, ability.value);
            logic.DamageCard(caster, target, damage, true);
        }

        private int GetDamage(Game data, Card caster, int value)
        {
            Player player = data.GetPlayer(caster.playerID);
            int damage = value + caster.GetTraitValue(bonusDamage) + player.GetTraitValue(bonusDamage);
            return damage;
        }
    }
}