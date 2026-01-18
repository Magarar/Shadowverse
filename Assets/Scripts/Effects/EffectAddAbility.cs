using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    /// <summary>
    /// Effect that adds an ability to a card
    /// </summary>
    [CreateAssetMenu(fileName = "New EffectAddAbility", menuName = "Effects/EffectAddAbility")]
    public class EffectAddAbility:EffectData
    {
        public AbilityData gainAbility;
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            target.AddAbility(gainAbility);
        }

        public override void DoOngoingEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            target.AddOngoingAbility(gainAbility);
        }
    }
}