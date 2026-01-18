using Data;
using GameClient;
using GameLogic;
using TMPro;

namespace UI
{
    /// <summary>
    /// Box that appears when using the SelectTarget ability target
    /// </summary>
    public class SelectTargetUI:SelectorPanel
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI desc;
        
        private static SelectTargetUI instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Update()
        {
            base.Update();

            Game game = Gameclient.Get().GetGameData();
            if (game != null && game.selector == SelectorType.None)
                Hide();
        }

        public override void Show(AbilityData ability, Card caster)
        {
            title.text = ability.title;
            Show();
        }
        
        public void OnClickClose()
        {
            Gameclient.Get().CancelSelection();
        }
        
        public override bool ShouldShow()
        {
            Game data = Gameclient.Get().GetGameData();
            int playerID = Gameclient.Get().GetPlayerID();
            return data.selector == SelectorType.SelectTarget && data.selectorPlayerId == playerID;
        }
        
        public static SelectTargetUI Get()
        {
            return instance;
        }
    }
}