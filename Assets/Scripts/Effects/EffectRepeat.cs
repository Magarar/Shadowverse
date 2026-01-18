using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    /// <summary>
    /// Repeat an ability X times
    /// </summary>

    [CreateAssetMenu(fileName = "New EffectRemoveTrait", menuName = "Effects/EffectRemoveTrait")]
    public class EffectRepeat: EffectData
    {
        public AbilityData ability;
        public EffectRepeatType type;
        
        public override void DoEffect(Gamelogic logic, AbilityData iability, Card caster)
        {
            int count = GetRepeatCount(logic.GameData, iability);
            for (int i = 0; i < count; i++)
            {
                Card triggerer = logic.GameData.GetCard(logic.GameData.abilityTrigger);
                logic.TriggerAbilityDelayed(this.ability, caster, triggerer);
            }
        }

        public override void DoEffect(Gamelogic logic, AbilityData iability, Card caster, Player target)
        {
            int count = GetRepeatCount(logic.GameData, iability);
            for (int i = 0; i < count; i++)
            {
                Card triggerer = logic.GameData.GetCard(logic.GameData.abilityTrigger);
                logic.TriggerAbilityDelayed(this.ability, caster, triggerer);
            }
        }

        public override void DoEffect(Gamelogic logic, AbilityData iability, Card caster, Card target)
        {
            int count = GetRepeatCount(logic.GameData, iability);
            for (int i = 0; i < count; i++)
            {
                Card triggerer = logic.GameData.GetCard(logic.GameData.abilityTrigger);
                logic.TriggerAbilityDelayed(this.ability, caster, triggerer);
            }
        }
        
        public int GetRepeatCount(Game game, AbilityData iability)
        {
            if (type == EffectRepeatType.SelectedValue)
                return game.selectedValue;
            if (type == EffectRepeatType.FixedValue)
                return iability.value;
            return 0;
        }
    }
    
    public enum EffectRepeatType
    {
        FixedValue,
        SelectedValue
    }
}