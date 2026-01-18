using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    /// <summary>
    /// Effect to make a card attack a target
    /// </summary>
    [CreateAssetMenu(fileName = "New EffectAttack", menuName = "Effects/EffectAttack")]
    public class EffectAttack : EffectData
    {
        public EffectAttackerType attackerType;
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            Card attacker = GetAttacker(logic.GetGameData(), caster);
            if (attacker != null)
            {
                logic.AttackPlayer(attacker, target, true);
            }
        }
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            Card attack = GetAttacker(logic.GetGameData(), caster);
            if (attack != null)
            {
                logic.AttackTarget(attack, target, true);
            }
        }
        
        public Card GetAttacker(Game gdata, Card caster)
        {
            if (attackerType == EffectAttackerType.Self)
                return caster;
            if (attackerType == EffectAttackerType.AbilityTriggerer)
                return gdata.GetCard(gdata.abilityTrigger);
            if (attackerType == EffectAttackerType.LastPlayed)
                return gdata.GetCard(gdata.lastPlayed);
            if (attackerType == EffectAttackerType.LastTargeted)
                return gdata.GetCard(gdata.lastTarget);
            return null;
        }
    }
    
    public enum EffectAttackerType
    {
        Self = 1,                  
        AbilityTriggerer = 25, 
        LastPlayed = 70,  
        LastTargeted = 72, 
    }
}