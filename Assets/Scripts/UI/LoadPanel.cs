using GameClient;
using TMPro;

namespace UI
{
    /// <summary>
    /// Loading panel that appears at the begining of a match, waiting for players to connect
    /// </summary>

    public class LoadPanel:UIPanel
    {
        public TextMeshProUGUI loadText;
        
        private static LoadPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            if(loadText.text != null)
                loadText.text = "";
        }
        
        protected override void Start()
        {
            base.Start();

            Gameclient.Get().onConnectGame += OnConnect;
            Gameclient.Get().onPlayerReady += OnReady;
            Gameclient.Get().onGameStart += OnStart;

            SetLoadText("Connecting to server...");
        }
        
        private void OnConnect()
        {
            SetLoadText("Sending player data...");
        }

        private void OnStart()
        {
            SetLoadText("");
        }
        
        private void OnReady(int playerID)
        {
            if (playerID == Gameclient.Get().GetPlayerID())
            {
                SetLoadText("Waiting for other player...");
            }
        }
        
        private void SetLoadText(string text)
        {
            if (IsOnline())
            {
                if (loadText != null)
                    loadText.text = text;
            }
        }
        
        public bool IsOnline()
        {
            return Gameclient.gameSetting.IsOnline();
        }
        
        public static LoadPanel Get()
        {
            return instance;
        }
    }
}