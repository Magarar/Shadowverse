using Data;
using GameClient;
using GameLogic;
using Network;
using TMPro;
using UI;
using Unit;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Menu
{
    /// <summary>
    /// Main script for the P2P menu scene
    /// </summary>
    public class TestP2P: MonoBehaviour
    {
        public UIPanel deckPanel;
        public UIPanel joinPanel;
        public InputField username;
        public InputField password;
        public DeckSelector deckSelector;
        public DeckDisplay deckPreview;
        public TMP_InputField joinIP;
        public TextMeshProUGUI error;
        
        private bool starting = false;
        
        void Start()
        {
            Gameclient.gameSetting = GameSetting.Default;
            Gameclient.playerSettings = PlayerSettings.Default;
            Gameclient.gameSetting.gameUid = "test_p2p";

            deckSelector.onChange += OnChangeDeck;
            error.text = "";
        }
        
        private async void Login()
        {
            error.text = "";
            bool success = await Authenticator.Get().Login(username.text, password.text);
            if (success)
            {
                UserData udata = await Authenticator.Get().LoadUserData();
                Gameclient.playerSettings.avatar = udata.GetAvatar();
                Gameclient.playerSettings.cardback = udata.GetCardback();
                deckPanel.Show();
                RefreshDeckList();
            }
            else
            {
                error.text = Authenticator.Get().GetError();
            }
        }
        
        public void StartGame()
        {
            if (!starting)
            {
                starting = true;
                SceneNav.GoTo(Gameclient.gameSetting.GetScene());
            }
        }
        
        public void RefreshDeckList()
        {
            deckSelector.SetupUserDeckList();
            deckSelector.SelectDeck(Gameclient.playerSettings.deck.tid);
            RefreshDeck(deckSelector.GetDeckID());
        }
        
        private void RefreshDeck(string tid)
        {
            UserData user = Authenticator.Get().UserData;
            UserDeckData udeck = user.GetDeck(tid);
            DeckData ddeck = DeckData.Get(tid);
            if (udeck != null)
                deckPreview.SetDeck(udeck);
            else if (ddeck != null)
                deckPreview.SetDeck(ddeck);
            else
                deckPreview.Clear();
        }
        
        public void OnChangeDeck(string tid)
        {
            Gameclient.playerSettings.deck = deckSelector.GetDeck();
            PlayerPrefs.SetString("tcg_deck", tid);
            RefreshDeck(tid);
        }
        
        public void OnClickLogin()
        {
            if (username.text.Length == 0)
                return;

            Login();
        }

        public void OnClickHost()
        {
            Gameclient.gameSetting.gameType = GameType.HostP2P;
            Gameclient.gameSetting.serverUrl = "127.0.0.1";
            Gameclient.playerSettings.deck = deckSelector.GetDeck();
            StartGame();
        }
        
        public void OnClickGoJoin()
        {
            Gameclient.playerSettings.deck = deckSelector.GetDeck();
            deckPanel.Hide();
            joinPanel.Show();
        }
        
        public void OnClickJoin()
        {
            if (joinIP.text.Length == 0)
                return;

            Gameclient.gameSetting.gameType = GameType.Multiplayer;
            Gameclient.gameSetting.serverUrl = joinIP.text;
            StartGame();
        }

        public void OnClickBack()
        {
            if (joinPanel.IsVisible())
            {
                joinPanel.Hide();
                deckPanel.Show();
            }
            else
            {
                deckPanel.Hide();
            }
        }
    }
}