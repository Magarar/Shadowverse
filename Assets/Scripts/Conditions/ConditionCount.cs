    using System.Collections.Generic;
using Data;
using GameLogic;
using UnityEngine;

namespace Conditions
{
    /// <summary>
    /// Trigger condition that count the amount of cards in pile of your choise (deck/discard/hand/board...)
    /// Can also only count cards of a specific type/team/trait
    /// </summary>
    [CreateAssetMenu(fileName = "New ConditionCount", menuName = "Conditions/ConditionCount")]
    public class ConditionCount: ConditionData
    {
        [Header("Count cards of type")]
        [Header("Count cards of type")]
        public ConditionPlayerType target;
        public PileType pile;
        public ConditionOperatorInt oper;
        public int value;
        
        [Header("Traits")]
        public CardType has_type;
        public TeamData has_team;
        public TraitData has_trait;
        
        public override bool IsTriggerConditionMet(Game data, AbilityData ability, Card caster)
        {
            int count = 0;
            if (target == ConditionPlayerType.Self || target == ConditionPlayerType.Both)
            {
                Player player =  data.GetPlayer(caster.playerID);
                count += CountPile(player, pile);
            }
            if (target == ConditionPlayerType.Opponent || target == ConditionPlayerType.Both)
            {
                Player player = data.GetOpponentPlayer(caster.playerID);
                count += CountPile(player, pile);
            }
            return CompareInt(count, oper, value);
        }
        
        private int CountPile(Player player, PileType pile)
        {
            List<Card> cardPile = null;

            if (pile == PileType.Hand)
                cardPile = player.cardsHand;

            if (pile == PileType.Board)
                cardPile = player.cardsBoard;

            if (pile == PileType.Equipped)
                cardPile = player.cardsEquip;

            if (pile == PileType.Deck)
                cardPile = player.cardsDeck;

            if (pile == PileType.Discard)
                cardPile = player.cardsDiscard;

            if (pile == PileType.Secret)
                cardPile = player.cardsSecret;

            if (pile == PileType.Temp)
                cardPile = player.cardsTemp;

            if (cardPile != null)
            {
                int count = 0;
                foreach (Card card in cardPile)
                {
                    if (IsTrait(card))
                        count++;
                }
                return count;
            }
            return 0;
        }
        
        private bool IsTrait(Card card)
        {
            bool is_type = card.CardData.type == has_type || has_type == CardType.None;
            bool is_team = card.CardData.team == has_team || has_team == null;
            bool is_trait = card.HasTrait(has_trait) || has_trait == null;
            return (is_type && is_team && is_trait);
        }
    }
    
    public enum ConditionPlayerType
    {
        Self = 0,
        Opponent = 1,
        Both = 2,
    }
}