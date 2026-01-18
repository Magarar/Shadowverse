using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    /// <summary>
    /// Effect that adds or removes basic card/player stats such as hp, attack, Mana, by the value of the dice roll
    /// </summary>
    [CreateAssetMenu(fileName = "New EffectAddStatRoll", menuName = "Effects/EffectAddStatRoll")]
    public class EffectAddStatRoll : EffectData
    {
        public EffectStatType type;
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            Game data = logic.GetGameData();

            if (type == EffectStatType.HP)
            {
                target.hp += data.rolledValue;
                target.hpMax += data.rolledValue;
            }

            if (type == EffectStatType.Mana)
            {
                target.mana += data.rolledValue;
                target.manaMax += data.rolledValue;
                target.mana = Mathf.Max(target.mana, 0);
                target.manaMax = Mathf.Clamp(target.manaMax, 0, GamePlayData.Get().manaMax);
            }
        }
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            Game data = logic.GetGameData();

            if (type == EffectStatType.Attack)
                target.attack += data.rolledValue;
            if (type == EffectStatType.HP)
                target.hp += data.rolledValue;
            if (type == EffectStatType.Mana)
                target.mana += data.rolledValue;
        }
    }
}