using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectChangeOwner", menuName = "Effects/EffectChangeOwner")]
    public class EffectChangeOwner: EffectData
    {
        public bool ownerOpponent; //Change to self or opponent?
        
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            Game game = logic.GetGameData();
            Player tplayer = ownerOpponent ? game.GetOpponentPlayer(caster.playerID) : game.GetPlayer(caster.playerID);
            logic.ChangeOwner(target, tplayer);
        }

    }
}