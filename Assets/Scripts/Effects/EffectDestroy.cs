using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectDestroy", menuName = "Effects/EffectDestroy")]
    public class EffectDestroy: EffectData
    {
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            if (logic.GameData.IsOnBoard(target))
                logic.KillCard(caster, target);
            else
                logic.DiscardCard(target);
        }
    }
}