using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Compares basic player stats such as attack/hp/Mana
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionPlayerStat", menuName = "Conditions/ConditionPlayerStat")]
    public class ConditionPlayerStat: ConditionData
    {
        [Header("Card stat is")]
        public ConditionStatType type;
        public ConditionOperatorInt oper;
        public int value;
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            Player ptarget = data.GetPlayer(target.playerID);
            return IsTargetConditionMet(data, ability, caster, ptarget);
        }
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            if (type == ConditionStatType.HP)
            {
                return CompareInt(target.hp, oper, value);
            }

            if (type == ConditionStatType.Mana)
            {
                return CompareInt(target.Mana, oper, value);
            }

            return false;
        }
    }
    
    public enum ConditionStatType
    {
        None = 0,
        Attack = 10,
        HP = 20,
        Mana = 30,
    }
}