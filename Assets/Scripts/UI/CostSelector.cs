using Data;
using GameClient;
using GameLogic;

namespace UI
{
    /// <summary>
    /// The Cost selector is a box that appears when using an ability with ChoiceSelector as target
    /// it let you choose between different abilities
    /// </summary>
    public class CostSelector:SelectorPanel
    {
        public NumberSelector selector;

        private Card caster;

        private static CostSelector instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }
        
        protected override void Start()
        {
            base.Start();

        }
        
        protected override void Update()
        {
            base.Update();

            Game game = Gameclient.Get().GetGameData();
            if (game != null && game.selector == SelectorType.None)
                Hide();
        }
        
        public void RefreshPanel()
        {
            if (caster == null)
                return;

            Game game = Gameclient.Get().GetGameData();
            Player player = game.GetPlayer(caster.playerID);
            selector.SetMax(player.Mana);
            selector.SetValue(0);
        }
        
        public void OnClickOK()
        {
            Game data = Gameclient.Get().GetGameData();
            if (data.selector == SelectorType.SelectorCost)
            {
                Gameclient.Get().SelectCost(selector.value);
            }

            Hide();
        }
        
        public void OnClickCancel()
        {
            Gameclient.Get().CancelSelection();
            Hide();
        }
        
        public override void Show(AbilityData iability, Card caster)
        {
            this.caster = caster;
            Show();
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public override bool ShouldShow()
        {
            Game data = Gameclient.Get().GetGameData();
            int playerID = Gameclient.Get().GetPlayerID();
            return data.selector == SelectorType.SelectorCost && data.selectorPlayerId == playerID;
        }
        
        public static CostSelector Get()
        {
            return instance;
        }
    }
}