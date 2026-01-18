using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    /// <summary>
    /// Effect to play a card from your hand for free
    /// </summary>
    [CreateAssetMenu(fileName = "New EffectPlay", menuName = "Effects/EffectPlay")]
    public class EffectPlay: EffectData
    {
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            Game game = logic.GetGameData();
            Player player = game.GetPlayer(caster.playerID);
            Slot slot = player.GetRandomEmptySlot(logic.GetRandom());

            player.RemoveCardFromAllGroups(target);
            player.cardsHand.Add(target);

            if (slot != Slot.None)
            {
                logic.PlayCard(target, slot, true);
            }
        }
    }
}