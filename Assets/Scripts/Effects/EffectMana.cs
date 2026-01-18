using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    /// <summary>
    /// Effect to gain/lose Mana (player)
    /// </summary>
    [CreateAssetMenu(fileName = "New EffectMana", menuName = "Effects/EffectMana")]
    public class EffectMana: EffectData
    {
        public bool increaseValue;
        public bool increaseMax;
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            if (increaseMax)
            {
                target.manaMax += ability.value;
                target.manaMax = Mathf.Clamp(target.manaMax, 0, GamePlayData.Get().manaMax);
            }
            
            if(increaseValue)
            {
                target.mana += ability.value;
                target.mana = Mathf.Max(target.mana, 0);
            }
        }
    }
}