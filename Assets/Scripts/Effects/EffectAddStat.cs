using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    /// <summary>
    /// Effect that adds or removes basic card/player stats such as hp, attack, Mana
    /// </summary>
    [CreateAssetMenu(fileName = "New EffectAddStat", menuName = "Effects/EffectAddStat")]
    public class EffectAddStat: EffectData
    {
        public EffectStatType type;
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            if (type == EffectStatType.HP)
            {
                target.hp += ability.value;
                target.hpMax += ability.value;
            }

            if (type == EffectStatType.Mana)
            {
                target.mana += ability.value;
                target.manaMax += ability.value;
                target.mana = Mathf.Max(target.mana, 0);
                target.manaMax = Mathf.Clamp(target.manaMax, 0, GamePlayData.Get().manaMax);
            }
        }
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            if (type == EffectStatType.Attack)
                target.attack += ability.value;
            if (type == EffectStatType.HP)
                target.hp += ability.value;
            if (type == EffectStatType.Mana)
                target.mana += ability.value;
        }
        
        public override void DoOngoingEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            if (type == EffectStatType.Attack)
                target.attackOngoing += ability.value;
            if (type == EffectStatType.HP)
                target.hpOngoing += ability.value;
            if (type == EffectStatType.Mana)
                target.manaOngoing += ability.value;
        }
    }
    
    public enum EffectStatType
    {
        None = 0,
        Attack = 10,
        HP = 20,
        Mana = 30,
    }
}