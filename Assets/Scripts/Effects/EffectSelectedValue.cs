using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectSelectedValue", menuName = "Effects/EffectSelectedValue")]
    public class EffectSelectedValue: EffectData
    {
        public EffectOperatorInt oper;
        public int value;

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            logic.GameData.selectedValue = AddOrSet(logic.GameData.selectedValue, oper, value);
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            logic.GameData.selectedValue = AddOrSet(logic.GameData.selectedValue, oper, value);
        }
    }
}