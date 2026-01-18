using System.Collections;
using Data;
using GameClient;
using GameLogic;
using Network;
using TMPro;
using UI;
using Unit;
using UnityEngine;

namespace Menu
{
    /// <summary>
    /// Main script for the main menu scene
    /// </summary>
    public class MainMenu:MonoBehaviour
    {
        public AudioClip music;
        public AudioClip ambience;
        
        [Header("Player UI")]
        public TextMeshProUGUI usernameText;
        public TextMeshProUGUI creditsText;
        public AvatarUI avatar;
        public GameObject loader;
        
        [Header("UI")]
        public TextMeshProUGUI versionText;
        public DeckSelector deckSelector;
        public DeckDisplay deckPreview;
        
        private bool starting = false;

        private static MainMenu instance;
        
        void Awake()
        {
            instance = this;

            //Set default settings
            Application.targetFrameRate = 120;
            Gameclient.gameSetting = GameSetting.Default;
        }

        private void Start()
        {
            BlackPanel.Get().Show(true);
            AudioTool.Get().PlayMusic("music", music);
            AudioTool.Get().PlaySFX("ambience", ambience, 0.5f, true, true);

            usernameText.text = "";
            creditsText.text ="";
            versionText.text = "Version " + Application.version;
            deckSelector.onChange += OnChangeDeck;
            
            if (Authenticator.Get().IsConnected())
                AfterLogin();
            else
                RefreshLogin();
        }

        void Update()
        {
            UserData udata = Authenticator.Get().UserData;
            if (udata != null)
            {
                creditsText.text = GameUI.FormatNumber(udata.coins);
            }
            bool matchmaking = GameClientMatchmaker.Get().IsMatchmaking();
            if (loader.activeSelf != matchmaking)
                loader.SetActive(matchmaking);
            if (MatchmakingPanel.Get().IsVisible() != matchmaking)
                MatchmakingPanel.Get().SetVisible(matchmaking);
        }

        private async void RefreshLogin()
        {
            bool success = await Authenticator.Get().RefreshLogin();
            if (success)
                AfterLogin();
            else
                SceneNav.GoTo("LoginMenu");
        }

        private void AfterLogin()
        {
            BlackPanel.Get().Hide();
            
            //Events
            GameClientMatchmaker matchmaker = GameClientMatchmaker.Get();
            matchmaker.onMatchmaking += OnMatchmakingDone;
            matchmaker.onMatchList += OnReceiveObserver;
            
            //Deck
            Gameclient.playerSettings.deck.tid = PlayerPrefs.GetString("tcg_deck_" + Authenticator.Get().Username, "");
            
            //UserData
            RefreshUserData();
        }

        public async void RefreshUserData()
        {
            UserData user = await Authenticator.Get().LoadUserData();
            if (user != null)
            {
                usernameText.text = user.username;
                creditsText.text = GameUI.FormatNumber(user.coins);
                
                AvatarData avatar = AvatarData.Get(user.avatar);
                this.avatar.SetAvatar(avatar);

                //Decks
                RefreshDeckList();
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
            if (deckPreview != null)
            {
                deckPreview.SetDeck(tid);
            }
        }
        
        private void OnChangeDeck(string tid)
        {
            Gameclient.playerSettings.deck = deckSelector.GetDeck();
            PlayerPrefs.SetString("tcg_deck_" + Authenticator.Get().Username, tid);
            RefreshDeck(tid);
        }

        private void OnMatchmakingDone(MatchmakingResult result)
        {
            if(result==null)
                return;
            
            if (result.success)
            {
                Debug.Log("Matchmaking found: " + result.success + " " + result.serverURL + "/" + result.gameUID);
                StartGame(GameType.Multiplayer, result.gameUID, result.serverURL);
            }
            else
            {
                MatchmakingPanel.Get().SetCount(result.players);
            }
        }

        private void OnReceiveObserver(MatchList list)
        {
            MatchListItem target = null;
            foreach (MatchListItem item in list.items)
            {
                if (item.username == Gameclient.observerUser)
                    target = item;
            }

            if (target != null)
            {
                StartGame(GameType.Observer, target.gameUID, target.gameURL);
            }
        }
        
        public void StartGame(GameType type, GameMode mode)
        {
            string uid = GameTool.GenerateRandomID();
            Gameclient.gameSetting.gameType = type;
            Gameclient.gameSetting.gameMode = mode;
            StartGame(uid); 
        }
        
        public void StartGame(GameType type, string gameUID, string serverURL = "")
        {
            Gameclient.gameSetting.gameType = type;
            StartGame(gameUID, serverURL);
        }


        public void StartGame(string gameUID, string serverURL = "")
        {
            if (!starting)
            {
                starting = true;
                Gameclient.gameSetting.serverUrl = serverURL;
                Gameclient.gameSetting.gameUid = gameUID;
                GameClientMatchmaker.Get().Disconnect();
                FadeToScene(Gameclient.gameSetting.GetScene());
            }
        }
        
        public void StartObserve(string user)
        {
            Gameclient.observerUser = user;
            GameClientMatchmaker.Get().StopMatchmaking();
            GameClientMatchmaker.Get().RefreshMatchList(user);
        }

        public void StartChallenge(string user)
        {
            string self = Authenticator.Get().Username;
            if (self == user)
                return; //Cant challenge self
            
            string key;
            if (self.CompareTo(user) > 0)
                key = self + "-" + user;
            else
                key = user + "-" + self;

            StartMatchmaking(GameMode.Casual, key);
        }

        public void StartMatchmaking(GameMode mode, string group)
        {
            UserDeckData deck = deckSelector.GetDeck();
            if (deck != null)
            {
                Gameclient.gameSetting.gameType = GameType.Multiplayer;
                Gameclient.gameSetting.gameMode = mode;
                Gameclient.playerSettings.deck = deck;
                Gameclient.gameSetting.scene = GamePlayData.Get().GetRandomArena();
                GameClientMatchmaker.Get().StartMatchmaking(group, Gameclient.gameSetting.nbPlayers);
            }
        }
        
        public void OnClickSolo()
        {
            if (!Authenticator.Get().IsConnected())
            {
                FadeToScene("LoginMenu");
                return;
            }

            SoloPanel.Get().Show();
        }

        public void OnClickPvP()
        {
            if (!Authenticator.Get().IsConnected())
            {
                FadeToScene("LoginMenu");
                return;
            }

            UserDeckData deck = deckSelector.GetDeck();
            if (deck == null || !deck.IsValid())
                return;

            StartMatchmaking(GameMode.Ranked, "");
        }
        
        public void OnClickAdventure()
        {
            AdventurePanel.Get().Show();
        }
        
        public void OnClickPlayCode()
        {
            JoinCodePanel.Get().Show();
        }
        
        public void OnClickCancelMatch()
        {
            GameClientMatchmaker.Get().StopMatchmaking();
        }
        
        public void FadeToScene(string scene)
        {
            StartCoroutine(FadeToRun(scene));
        }

        private IEnumerator FadeToRun(string scene)
        {
            BlackPanel.Get().Show();
            AudioTool.Get().FadeOutMusic("music");
            yield return new WaitForSeconds(1f);
            SceneNav.GoTo(scene);
        }
        
        public void OnClickLogout()
        {
            TcgNetwork.Get().Disconnect();
            Authenticator.Get().Logout();
            FadeToScene("LoginMenu");
        }
        
        public void OnClickQuit()
        {
            Application.Quit();
        }
        
        public void OnClickSettings()
        {
            SettingsPanel.Get().Show();
        }
        
        public static MainMenu Get()
        {
            return instance;
        }
    }
}