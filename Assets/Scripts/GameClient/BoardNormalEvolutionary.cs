using GameLogic;

namespace GameClient
{
    public class BoardNormalEvolutionary:BoardEvolutionary
    {
        protected override void Start()
        {
            base.Start();
            type = EvolutionaryType.Normal;
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
                evolutionaryBar.value = player.evolutionPoint;
                evolutionaryBar.maxValue = player.evolutionPointMax;
                evolutionaryTurnTip.text = !player.enableEvolution ? $"{player.enableEvolutionPointTurn}" : $"{player.evolutionPoint}";
            }
        }
    }
}