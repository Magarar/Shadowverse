using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Condition that is checked only by the AI. 
    /// Prevents the AI from targeting itself with bad spells even though you want to give real players the flexibility to do it
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionOwnerAI", menuName = "Conditions/ConditionOwnerAI")]
    public class ConditionOwnerAI: ConditionData
    { 
        [Header("AI Only: Target owner is caster owner")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            if (!IsAIPlayer(data, caster))
                return true; //Condition always true for human players

            bool sameOwner = caster.playerID== target.playerID;
            return CompareBool(sameOwner, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            if (!IsAIPlayer(data, caster))
                return true; //Condition always true for human players

            bool sameOwner = caster.playerID == target.id;
            return CompareBool(sameOwner, oper);
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
        {
            if (!IsAIPlayer(data, caster))
                return true; //Condition always true for human players

            bool sameOwner = Slot.GetP(caster.playerID) == target.p;
            return CompareBool(sameOwner, oper);
        }

        private bool IsAIPlayer(Game data, Card caster)
        {
            Player player = data.GetPlayer(caster.playerID);
            return player.isAI;
        }
        
    }
}