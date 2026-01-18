using Data;
using GameClient;
using GameLogic;
using Network;
using TMPro;
using UI;
using UnityEngine.Serialization;

namespace Menu
{
    public class SoloPanel:UIPanel
    {
        public TextMeshProUGUI username;
        public DeckSelector selectorPlayer;
        public DeckSelector selectorAI;

        public DeckDisplay displayPlayer;
        public DeckDisplay displayAI;

        private static SoloPanel instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }
        
        protected override void Start()
        {
            base.Start();

            selectorPlayer.onChange += OnChangeDeck;
            selectorAI.onChange += OnChangeDeck;
        }
        
        private void RefreshDecks()
        {
            if(username != null)
                username.text = Authenticator.Get().Username;

            string selected_id = MainMenu.Get().deckSelector.GetDeckID();
            selectorPlayer.SetupUserDeckList();
            selectorPlayer.SelectDeck(selected_id);
            selectorAI.SetupAIDeckList();
            selectorAI.SelectDeck(0);

            RefreshDeckDisplay();
        }
        
        private void RefreshDeckDisplay()
        {
            displayPlayer.SetDeck(selectorPlayer.GetDeckID());
            displayAI.SetDeck(selectorAI.GetDeckID());
        }
        
        private void OnChangeDeck(string id)
        {
            RefreshDeckDisplay();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshDecks();
        }
        
        public void OnClickPlay()
        {
            UserDeckData deck = selectorPlayer.GetDeck();
            if (deck == null || !deck.IsValid())
                return;

            UserDeckData aideck = selectorAI.GetDeck();
            if (aideck == null || !aideck.IsValid())
                return;

            Gameclient.playerSettings.deck = deck;
            Gameclient.aiSettings.deck = aideck;
            Gameclient.aiSettings.aiLevel = GamePlayData.Get().aiLevel;
            Gameclient.gameSetting.scene = GamePlayData.Get().GetRandomArena();

            MainMenu.Get().StartGame(GameType.Solo, GameMode.Casual);
        }
        
        

        public static SoloPanel Get()
        {
            return instance;
        }
    }
}