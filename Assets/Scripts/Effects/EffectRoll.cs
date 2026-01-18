using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectRoll", menuName = "Effects/EffectRoll")]
    public class EffectRoll: EffectData
    {
        public int dice = 6;

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            logic.RollRandomValue(dice);
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            logic.RollRandomValue(dice);
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Slot target)
        {
            logic.RollRandomValue(dice);
        }
    }
}