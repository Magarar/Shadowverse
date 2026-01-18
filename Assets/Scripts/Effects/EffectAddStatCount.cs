using System.Collections.Generic;
using Conditions;
using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    /// <summary>
    /// Effect that sets stats equal to a dynamic calculated value from a pile (number of cards on board/hand/deck)
    /// </summary>

    [CreateAssetMenu(fileName = "New EffectAddStatCount", menuName = "Effects/EffectAddStatCount")]
    public class EffectAddStatCount:EffectData
    {
        public EffectStatType type;
        public PileType pile;

        [Header("Count Traits")]
        public CardType has_type;
        public TeamData has_team;
        public TraitData has_trait;
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            int val = GetCount(logic.GetGameData(), caster) * ability.value;
            if (type == EffectStatType.HP)
            {
                target.hp += val;
                target.hpMax += ability.value;
            }

            if (type == EffectStatType.Mana)
            {
                target.mana += val;
                target.manaMax += val;
                target.mana = Mathf.Max(target.mana, 0);
                target.manaMax = Mathf.Clamp(target.manaMax, 0, GamePlayData.Get().manaMax);
            }
        }
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            int val = GetCount(logic.GetGameData(), caster) * ability.value;
            if (type == EffectStatType.Attack)
                target.attack += val;
            if (type == EffectStatType.HP)
                target.hp += val;
            if (type == EffectStatType.Mana)
                target.mana += val;
        }

        public override void DoOngoingEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            int val = GetCount(logic.GetGameData(), caster) * ability.value;
            if (type == EffectStatType.Attack)
                target.attackOngoing += val;
            if (type == EffectStatType.HP)
                target.hpOngoing += val;
            if (type == EffectStatType.Mana)
                target.manaOngoing += val;
        }
        
        private int GetCount(Game data, Card caster)
        {
            Player player = data.GetPlayer(caster.playerID);
            return CountPile(player, pile);
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
}