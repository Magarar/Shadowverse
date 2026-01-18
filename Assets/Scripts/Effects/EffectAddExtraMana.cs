using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectAddExtraMana", menuName = "Effects/EffectAddExtraMana")]
    public class EffectAddExtraMana: EffectData
    {
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            target.manaOngoing += ability.value;
            if(target.manaOngoing<0)
                target.manaOngoing = 0;
        }
    }
}