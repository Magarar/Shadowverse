using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Check selected value for card cost
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionSelectedValue", menuName = "Conditions/ConditionSelectedValue")]
    public class ConditionSelectedValue : ConditionData
    {
        [Header("Selected Value is")]
        public ConditionOperatorInt oper;
        public int value;

        public override bool IsTriggerConditionMet(Game data, AbilityData ability, Card caster)
        {
            return CompareInt(data.selectedValue, oper, value);
        }
    }
}