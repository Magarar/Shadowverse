using GameClient;
using TMPro;
using UI;

namespace Menu
{
    /// <summary>
    /// Matchmaking panel is just a loading panel that displays how many players are found yet
    /// </summary>
    public class MatchmakingPanel:UIPanel
    {
        public TextMeshProUGUI text;
        public TextMeshProUGUI playerText;
        public TextMeshProUGUI codeText;
        
        private static MatchmakingPanel instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }
        
        protected override void Start()
        {
            base.Start();
            codeText.text = "";
        }

        protected override void Update()
        {
            base.Update();
            if (GameClientMatchmaker.Get().IsConnected())
                text.text = "Finding Opponent...";
            else
                text.text = "Connecting to server...";

            codeText.text = "";
            
            string group = GameClientMatchmaker.Get().GetGroup();
            if (group != null && group.StartsWith("code_"))
                codeText.text = group.Replace("code_", "");
        }
        
        public void SetCount(int players)
        {
            if(playerText!=null)
                playerText.text = players.ToString() + "/" + GameClientMatchmaker.Get().GetNbPlayers();
        }
        
        public void OnClickCancel()
        {
            GameClientMatchmaker.Get().StopMatchmaking();
            Hide();
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            if (playerText != null)
                playerText.text = "";
        }
        public static MatchmakingPanel Get()
        {
            return instance;
        }

      
    }
}