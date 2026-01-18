using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    //Sends the target card to a pile of your choice (deck/discard/hand)
    //Dont use to send to board since it needs a slot, use EffectPlay instead to send to board
    //Also dont send to discard from the board because it wont trigger OnKill effects, use EffectDestroy instead
    [CreateAssetMenu(fileName = "New EffectSendPile", menuName = "Effects/EffectSendPile")]
    public class EffectSendPile: EffectData
    {
        public PileType pile;
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            Game data = logic.GetGameData();
            Player player = data.GetPlayer(target.playerID);

            if (pile == PileType.Deck)
            {
                player.RemoveCardFromAllGroups(target);
                player.cardsDeck.Add(target);
                target.Clear();
            }

            if (pile == PileType.Hand)
            {
                player.RemoveCardFromAllGroups(target);
                player.cardsHand.Add(target);
                target.Clear();
            }

            if (pile == PileType.Discard)
            {
                player.RemoveCardFromAllGroups(target);
                player.cardsDiscard.Add(target);
                target.Clear();
            }

            if (pile == PileType.Temp)
            {
                player.RemoveCardFromAllGroups(target);
                player.cardsTemp.Add(target);
                target.Clear();
            }
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