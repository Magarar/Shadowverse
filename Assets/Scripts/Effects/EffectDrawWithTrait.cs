using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectDrawWithTrait", menuName = "Effects/EffectDrawWithTrait")]
    public class EffectDrawWithTrait: EffectData
    {
        public TraitData trait;
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            logic.DrawCardWithTrait(target, trait,ability.value);
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            Player player = logic.GameData.GetPlayer(target.playerID);
            logic.DrawCardWithTrait(player, trait,ability.value);
        }
    }
}