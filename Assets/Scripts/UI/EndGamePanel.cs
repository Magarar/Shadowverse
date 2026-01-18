using Api;
using Data;
using GameClient;
using GameLogic;
using TMPro;
using Unit;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Endgame panel is shown when a game end
    /// Showing winner and rewards obtained
    /// </summary>

    public class EndGamePanel:UIPanel
    {
        public TextMeshProUGUI winnerText;
        public Image winnerGlow;

        // public TextMeshProUGUI playerName;
        // public TextMeshProUGUI otherName;
        public Image playerAvatar;
        //public Image otherAvatar;

        //public TextMeshProUGUI coinsText;
        //public TextMeshProUGUI xpText;

        private bool rewardLoaded = false;
        private float timer = 0f;

        // private int targetCoins = 0;
        // private int targetXp = 0;
        // private float coins = 0;
        // private float xp = 0;

        private static EndGamePanel instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }
        
        protected override void Start()
        {
            base.Start();

            // coinsText.text = "";
            // xpText.text = "";

        }

        protected override void Update()
        {
            base.Update();
            if (!rewardLoaded && IsVisible())
            {
                timer += Time.deltaTime;
                if (timer > 1f)
                {
                    timer = 0f;
                    RefreshRewards();
                }
            }

            if (rewardLoaded)
            {
                // coins = Mathf.MoveTowards(coins, targetCoins, 2000f * Time.deltaTime);
                // xp = Mathf.MoveTowards(xp, targetXp, 500f * Time.deltaTime);
                //
                // coinsText.text = "+ " + Mathf.RoundToInt(coins) + " coins";
                // xpText.text = "+ " + Mathf.RoundToInt(xp) + " xp";
                //
                // if (Mathf.RoundToInt(coins) == 0)
                //     coinsText.text = "";
                // if (Mathf.RoundToInt(xp) == 0)
                //     xpText.text = "";
            }
        }

        private void RefreshPanel(int winner)
        {
            Game data = Gameclient.Get().GetGameData();
            Player pwinner = data.GetPlayer(winner);
            Player player = Gameclient.Get().GetPlayer();
            Player oplayer = Gameclient.Get().GetOpponentPlayer();

            // playerName.text = player.username;
            // otherName.text = oplayer.username;

            Sprite avat1 = player.hero.CardData.artFull;
            //AvatarData avat2 = AvatarData.Get(oplayer.avatar);
            if(avat1 != null)
                playerAvatar.sprite = avat1;
            // if (avat2 != null)
            //     otherAvatar.sprite = avat2.avatar;

            if (pwinner != null && pwinner == player)
                winnerText.text = "Victory";
            else if (pwinner != null)
                winnerText.text = "Defeat";
            else
                winnerText.text = "Tie";

            if (pwinner == player)
                winnerGlow.rectTransform.anchoredPosition = playerAvatar.rectTransform.anchoredPosition;
            // if (pwinner == oplayer)
            //     winnerGlow.rectTransform.anchoredPosition = otherAvatar.rectTransform.anchoredPosition;
            winnerGlow.gameObject.SetActive(pwinner != null);
        }

        private async void RefreshRewards()
        {
            //Online rewards
            if (Gameclient.gameSetting.IsOnline())
            {
                string url = ApiClient.ServerURL + "/matches/" + Gameclient.gameSetting.gameUid;
                WebResponse res = await ApiClient.Get().SendGetRequest(url);
                if (res.success)
                {
                    rewardLoaded = true;
                    MatchResponse match = ApiTool.JsonToObject<MatchResponse>(res.data);
                    string username = ApiClient.Get().Username.ToLower();
                    foreach (MatchDataResponse data in match.udata)
                    {
                        if (data.username.ToLower() == username)
                        {
                            // targetCoins = data.reward.coins;
                            // targetXp = data.reward.xp;
                        }
                    }
                }
            }

            //Adventure Rewards
            if (Gameclient.gameSetting.gameType == GameType.Adventure)
            {
                LevelData lvl = LevelData.Get(Gameclient.gameSetting.level);
                if (lvl != null && RewardManager.Get().IsRewardGained())
                {
                    // targetCoins = lvl.rewardCoins;
                    // targetXp = lvl.rewardXp;
                    rewardLoaded = true;
                }
            }
        }

        public void ShowEnd(int winner)
        {
            rewardLoaded = false;
            RefreshPanel(winner);
            RefreshRewards();
            Show();
        }

        public void OnClickQuit()
        {
            GameUI.Get().OnClickQuit();
        }

        public static EndGamePanel Get()
        {
            return instance;
        }
    }
}