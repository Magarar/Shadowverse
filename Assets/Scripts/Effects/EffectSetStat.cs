using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectSetStat", menuName = "Effects/EffectSetStat")]
    public class EffectSetStat: EffectData
    {
        public EffectStatType type;

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            if (type == EffectStatType.HP)
            {
                target.hp = ability.value;
            }

            if (type == EffectStatType.Mana)
            {
                target.mana = ability.value;
                target.mana = Mathf.Max(target.mana, 0);
            }
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            if (type == EffectStatType.Attack)
                target.attack = ability.value;
            if (type == EffectStatType.Mana)
                target.mana = ability.value;
            if (type == EffectStatType.HP)
            {
                target.hp = ability.value;
                target.damaged = 0;
            }
        }

        public override void DoOngoingEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            if (type == EffectStatType.Attack)
                target.attack = ability.value;
            if (type == EffectStatType.HP)
                target.hp = ability.value;
            if (type == EffectStatType.Mana)
                target.mana = ability.value;
        }
    }
}