using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectDraw", menuName = "Effects/EffectDraw")]
    public class EffectDraw: EffectData
    {
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            logic.DrawCard(target, ability.value);
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            Player player = logic.GameData.GetPlayer(target.playerID);
            logic.DrawCard(player, ability.value);
        }
    }
}