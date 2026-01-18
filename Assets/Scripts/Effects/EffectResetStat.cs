using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectResetStat", menuName = "Effects/EffectResetStat")]
    public class EffectResetStat : EffectData
    {
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            target.SetCard(target.CardData, target.VariantData);
        }
    }
}