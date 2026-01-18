using Conditions;
using Data;
using GameLogic;

namespace Effects
{
    /// <summary>
    /// Effects that creates a new card from a CardData
    /// Use for discover effects
    /// Unlike EffectSummon, this effect targets the card data you want to create, and goes into the pile selected here
    /// </summary>
    public class EffectCreate: EffectData
    {
        public PileType createPile;   //Better to not select Board here, for placing a card on board or in secret area, would suggest instead EffectSummon, or EffectPlay as chain after Create
        public bool createOpponent;       //Add to opponent?

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, CardData target)
        {
            Player player = logic.GameData.GetPlayer(caster.playerID);
            if(createOpponent)
                player = logic.GameData.GetOpponentPlayer(caster.playerID);
            
            Card card = Card.Create(target, caster.VariantData, player);
            logic.GameData.lastSummoned = card.uid;
            
            if (createPile == PileType.Deck)
                player.cardsDeck.Add(card);

            if (createPile == PileType.Discard)
                player.cardsDiscard.Add(card);

            if (createPile == PileType.Hand)
                player.cardsHand.Add(card);

            if (createPile == PileType.Temp)
                player.cardsTemp.Add(card);
        }
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            DoEffect(logic, ability, caster, target.CardData); //Create a copy
        }
    }
}