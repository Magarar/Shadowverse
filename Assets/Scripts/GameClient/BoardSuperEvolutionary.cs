using GameLogic;
using UnityEngine;

namespace GameClient
{
    public class BoardSuperEvolutionary:BoardEvolutionary
    {
        protected override void Start()
        {
             base.Start();
             type = EvolutionaryType.Super;
        }

        protected override void Update()
        {
            base.Update();
            if(!Gameclient.Get().IsReady())
                return;
            
            PlayerControls controls = PlayerControls.Get();
            Game data = Gameclient.Get().GetGameData();
            Player player = GetPlayer();

            if (player != null)
            {
                evolutionaryBar.value = player.superEvolutionPoint;
                evolutionaryBar.maxValue = player.superEvolutionPointMax;
                evolutionaryTurnTip.text = !player.enableSuperEvolution ? $"{player.enableSuperEvolutionPointTurn}" : $"{player.superEvolutionPoint}";
            }
        }
    }
}