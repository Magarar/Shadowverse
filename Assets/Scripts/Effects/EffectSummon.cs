using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    //Effect to Summon an entirely new card (not in anyones deck)
    //And places it on the board (if target slot) or hand (if target player)
    //Unlike EffectCreate, this effect targets where the card goes, and the carddata is selected on the effect
    [CreateAssetMenu(fileName = "New EffectSummon", menuName = "Effects/EffectSummon")]
    public class EffectSummon: EffectData
    {
        public CardData summon;
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Player target)
        {
            logic.SummonCardHand(target, summon, caster.VariantData); //Summon in hand instead of board when target a player
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            Player player = logic.GameData.GetPlayer(caster.playerID);
            Debug.Log("Summoning " + summon.GetTitle()+ " to " + target.slot);
            logic.SummonCard(player, summon, caster.VariantData, target.slot); //Assumes the target has just been killed, so the slot is empty
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Slot target)
        {
            Player player = logic.GameData.GetPlayer(caster.playerID);
            logic.SummonCard(player, summon, caster.VariantData, target);
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, CardData target)
        {
            Player player = logic.GameData.GetPlayer(caster.playerID);
            logic.SummonCardHand(player, target, caster.VariantData);   //Summon in hand instead of board when target a carddata
        }
    }
}