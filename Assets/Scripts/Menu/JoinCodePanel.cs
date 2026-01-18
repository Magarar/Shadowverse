using GameLogic;
using TMPro;
using UI;
using Unit;

namespace Menu
{
    public class JoinCodePanel: UIPanel
    {
        public TMP_InputField codeField;
        private string gameCode = "";

        private static JoinCodePanel instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Update()
        {
            base.Update();
        }
        
        public void OnClickRandomize()
        {
            gameCode = GameTool.GenerateRandomID(4,6).ToUpper();
            codeField.text = gameCode;
        }
        
        public void OnClickJoinCode()
        {
            if (codeField.text.Length < 3)
                return;

            gameCode = codeField.text.ToUpper();
            MainMenu.Get().StartMatchmaking(GameMode.Casual, "code_" + gameCode);
            Hide();
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            codeField.text = "";
        }

        public string GetCode()
        {
            return gameCode;
        }

        public static JoinCodePanel Get()
        {
            return instance;
        }

        
        
    }
}