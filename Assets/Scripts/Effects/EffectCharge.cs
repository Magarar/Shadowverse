using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectCharge", menuName = "Effects/EffectCharge")]
    public class EffectCharge: EffectData
    {
        public bool exhausted;
        public bool canAttackPlayer;
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            if (!logic.gameData.cardsAttacked.Contains(target.uid))
                target.exhausted = exhausted;
            if(!target.canAttackPlayer)
                target.canAttackPlayer = canAttackPlayer;
        }
    }
}