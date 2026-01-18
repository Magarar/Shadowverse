using Data;
using GameLogic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectTransform", menuName = "Effects/EffectTransform")]
    public class EffectTransform: EffectData
    {
        public CardData transformTo;

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            logic.TransformCard(target, transformTo);
        }
    }
}