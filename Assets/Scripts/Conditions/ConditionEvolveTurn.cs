using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    [CreateAssetMenu(fileName = "New ConditionEvolveTurn", menuName = "Conditions/ConditionEvolveTurn")]
    public class ConditionEvolveTurn: ConditionData
    {
        [Header("Evolve turn")] 
        public EvolveTurnType type;
        public ConditionOperatorBool oper;
        
        public override bool IsTriggerConditionMet(Game data, AbilityData abilityData, Card caster)
        {
            return isMet(data, caster);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData abilityData, Card caster, Card target)
        {
            return isMet(data, caster);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData abilityData, Card caster, Player target)
        {
            return isMet(data, caster);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData abilityData, Card caster, Slot target)
        {
            return isMet(data, caster);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData abilityData, Card caster, CardData target)
        {
            return isMet(data, caster);
        }

        public bool isMet(Game data, Card card)
        {
            Player player = data.GetPlayer(card.playerID);
            switch (type)
            {
                case EvolveTurnType.Common:
                    Debug.Log("Evolve turn: " + CompareBool(player.enableEvolution, oper));
                    return CompareBool(player.enableEvolution, oper);
                case EvolveTurnType.Super:
                    Debug.Log("Super Evolve turn: " + CompareBool(player.enableSuperEvolution, oper));
                    return CompareBool(player.enableSuperEvolution, oper);
            }
            return false;
        }
    }

    public enum EvolveTurnType
    {
        Common,
        Super
    }
}