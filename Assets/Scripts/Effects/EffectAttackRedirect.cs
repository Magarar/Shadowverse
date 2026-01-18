using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{ 
    /// <summary>
    /// Effect to redirect an attack (usually triggered with OnBeforeAttack or OnBeforeDefend)
    /// </summary>
    [CreateAssetMenu(fileName = "New EffectAttackRedirect", menuName = "Effects/EffectAttackRedirect")]
    public class EffectAttackRedirect: EffectData
    {
        public EffectAttackerType attackerType;
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            Card attacker = GetAttacker(logic.GetGameData(), caster);
            if (attacker != null)
            {
                logic.RedirectAttack(attacker, target);
            }
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            Card attacker = GetAttacker(logic.GetGameData(), caster);
            if (attacker != null)
            {
                logic.RedirectAttack(attacker, target);
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
}