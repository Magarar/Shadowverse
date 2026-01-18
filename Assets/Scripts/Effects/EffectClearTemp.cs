using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    /// <summary>
    /// Clear temporary array of player's card
    /// </summary>
    [CreateAssetMenu(fileName = "New EffectClearTemp", menuName = "Effects/EffectClearTemp")]
    public class EffectClearTemp: EffectData
    {
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster)
        {
            Player player = logic.GameData.GetPlayer(caster.playerID);
            player.cardsTemp.Clear();
        }

        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            Player player = logic.GameData.GetPlayer(caster.playerID);
            player.cardsTemp.Clear();
        }
    }
}