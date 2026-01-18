using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectShuffle", menuName = "Effects/EffectShuffle")]
    public class EffectShuffle: EffectData
    {
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            logic.ShuffleDeck(target.cardsDeck);
        }
    }
}