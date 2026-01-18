using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Condition that checks in which pile a card is (deck/discard/hand/board/secrets)
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionCardPile", menuName = "Conditions/ConditionCardPile")]
    public class ConditionCardPile: ConditionData
    {
        [Header("Card is in pile")]
        public PileType type;
        public ConditionOperatorBool oper;
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
        {
            if (target == null)
                return false;

            if (type == PileType.Hand)
            {
                return CompareBool(data.IsInHand(target), oper);
            }

            if (type == PileType.Board)
            {
                return CompareBool(data.IsOnBoard(target), oper);
            }

            if (type == PileType.Equipped)
            {
                return CompareBool(data.IsEquipped(target), oper);
            }

            if (type == PileType.Deck)
            {
                return CompareBool(data.IsInDeck(target), oper);
            }

            if (type == PileType.Discard)
            {
                return CompareBool(data.IsInDiscard(target), oper);
            }

            if (type == PileType.Secret)
            {
                return CompareBool(data.IsInSecret(target), oper);
            }

            if (type == PileType.Temp)
            {
                return CompareBool(data.IsInTemp(target), oper);
            }

            return false;
        }
        
        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
        {
            return false; //Player cannot be in a pile
        }

        public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
        {
            return type == PileType.Board && target != Slot.None; //Slot is always on board
        }
    }
    
    public enum PileType
    {
        None = 0,
        Board = 10,
        Hand = 20,
        Deck = 30,
        Discard = 40,
        Secret = 50,
        Equipped = 60,
        Temp = 90,
    }
}