using Data;
using GameLogic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Conditions
{
    /// <summary>
    /// Condition that checks the type, team and traits of a card
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionCardType", menuName = "Conditions/ConditionCardType")]
    public class ConditionCardType: ConditionData
    {
        [Header("Card is of type")]
        public CardType hasType;
        public TeamData hasTeam;
        public TraitData hasTrait;

        public ConditionOperatorBool oper;

        public ConditionCardType(CardType hasType)
        {
            this.hasType = hasType;
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            return CompareBool(IsTrait(target), oper);
        }
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            return false; //Not a card
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
        {
            return false; //Not a card
        }
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, CardData target)
        {
            bool isType = target.type == hasType || hasType == CardType.None;
            bool isTeam = target.team == hasTeam || hasTeam == null;
            bool isTrait = target.HasTrait(hasTrait) || hasTrait == null;
            return (isType && isTeam && isTrait);
        }
        
        private bool IsTrait(Card card)
        {
            bool isType = card.CardData.type == hasType || hasType == CardType.None;
            bool isTeam = card.CardData.team == hasTeam || hasTeam == null;
            bool isTrait = card.HasTrait(hasTrait) || hasTrait == null;
            return (isType && isTeam && isTrait);
        }
    }
}