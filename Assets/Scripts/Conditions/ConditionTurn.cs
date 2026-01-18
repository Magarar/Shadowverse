using Data;
using GameLogic;

namespace Conditions
{
    /// <summary>
    /// Checks if its your turn
    /// </summary>
    public class ConditionTurn: ConditionData
    {
        public ConditionOperatorBool oper;

        public override bool IsTriggerConditionMet(Game data, AbilityData ability, Card caster)
        {
            bool yourturn = caster.playerID == data.currentPlayer;
            return CompareBool(yourturn, oper);
        }
    }
}