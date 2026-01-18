using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    [CreateAssetMenu(fileName = "New ConditionRising", menuName = "Conditions/ConditionRising")]
    public class ConditionRising:ConditionData
    {
        public ConditionOperatorBool oper;
        
        public override bool IsTriggerConditionMet(Game data, AbilityData abilityData, Card caster)
        {
            Player player = data.GetPlayer(caster.playerID);
            return CompareBool(player.manaMax >= 7, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData abilityData, Card caster, Card target)
        {
            Player player = data.GetPlayer(caster.playerID);
            return CompareBool(player.manaMax >= 7, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData abilityData, Card caster, Player target)
        {
            Player player = data.GetPlayer(caster.playerID);
            return CompareBool(player.manaMax >= 7, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData abilityData, Card caster, Slot target)
        {
            Player player = data.GetPlayer(caster.playerID);
            return CompareBool(player.manaMax >= 7, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData abilityData, Card caster, CardData target)
        {
            Player player = data.GetPlayer(caster.playerID);
            return CompareBool(player.manaMax >= 7, oper);
        }
    }
}