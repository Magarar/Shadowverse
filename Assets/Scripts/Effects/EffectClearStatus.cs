using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectClearStatus", menuName = "Effects/EffectClearStatus")]
    public class EffectClearStatus: EffectData
    {
        public StatusData status;
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            if (status != null)
                target.RemoveStatus(status.effect);
            else
                target.status.Clear();
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            if (status != null)
                target.RemoveStatus(status.effect);
            else
                target.status.Clear();
        }

    }
}