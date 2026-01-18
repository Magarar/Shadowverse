using Data;
using GameClient;
using GameLogic;
using Network;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class LevelUI: MonoBehaviour
    {
        [Header("Level")]
        public LevelData level;

        [Header("UI")] 
        public TextMeshProUGUI title;
        public TextMeshProUGUI subtitle;
        public DeckDisplay deck;
        public GameObject completed;

        void Start()
        {
            Button btn = GetComponent<Button>();
            btn.onClick.AddListener(OnClick);
            completed.SetActive(false);

            if (level != null)
                SetLevel(level);
            else
                Hide();
        }
        
        public void SetLevel(LevelData level)
        {
            this.level = level;
            RefreshLevel();
        }

        public void RefreshLevel()
        {
            if (level != null)
            {
                title.text = level.title;
                subtitle.text = "LEVEL " + level.level;
                deck.SetDeck(level.playerDeck);
                gameObject.SetActive(true);

                UserData udata = Authenticator.Get().GetUserData();
                if(udata != null)
                    completed.SetActive(udata.HasReward(level.id));
            }
        }
        
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnClick()
        {
            if (level != null)
            {
                Gameclient.gameSetting.level = level.id;
                Gameclient.gameSetting.scene = level.scene;
                Gameclient.playerSettings.deck = new UserDeckData(level.playerDeck);
                Gameclient.aiSettings.deck = new UserDeckData(level.aiDeck);
                Gameclient.aiSettings.aiLevel = level.aiLevel;
                MainMenu.Get().StartGame(GameType.Adventure, GameMode.Casual);
            }
        }

       
    }
}