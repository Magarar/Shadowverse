using System.Net;
using System.Threading.Tasks;
using Api;
using Data;
using GameLogic;
using Network;
using UnityEngine;

namespace GameClient
{
    //Grants rewards for adventure solo mode
    public class RewardManager:MonoBehaviour
    {
        private bool rewardGained = false;

        private static RewardManager instance;
        
        void Awake()
        {
            instance = this;
        }
        
        private void Start()
        {
            Gameclient.Get().onGameEnd += OnGameEnd;
        }

        private void OnGameEnd(int winner)
        {
            int playerID = Gameclient.Get().GetPlayerID();

            if (Gameclient.gameSetting.gameType == GameType.Adventure && winner == playerID)
            {
                UserData udata = Authenticator.Get().UserData;
                LevelData level = LevelData.Get(Gameclient.gameSetting.level);
                if (level != null && !udata.HasReward(level.id) && !rewardGained)
                {
                    if (Authenticator.Get().IsTest())
                        GainRewardTest(level);
                    if (Authenticator.Get().IsApi())
                        GainRewardAPI(level);
                }
            }

        }

        private async void GainRewardTest(LevelData level)
        {
            UserData udata = Authenticator.Get().UserData;
            udata.coins += level.rewardCoins;
            udata.xp += level.rewardXp;
            udata.AddReward(level.id);
            
            foreach (CardData card in level.rewardCards)
            {
                udata.AddCard(card.id, card.GetVariant().id, 1);
            }
            
            foreach (PackData pack in level.rewardPacks)
            {
                udata.AddPack(pack.id, 1);
            }
            rewardGained = true;
            await Authenticator.Get().SaveUserData();
        }

        private async void GainRewardAPI(LevelData level)
        {
            bool success = await GainRewardAPI(level.id);
            rewardGained = success;
        }

        public async Task<bool> GainRewardAPI(string rewardID)
        {
            RewardGainRequest req = new RewardGainRequest()
            {
                reward = rewardID
            };
            
            string url = ApiClient.ServerURL + "/users/rewards/gain/" + ApiClient.Get().UserID;
            string json = ApiTool.ToJson(req);
            var res = await ApiClient.Get().SendPostRequest(url, json);
            Debug.Log("Gain Reward: " + rewardID + " " + res.success);
            return res.success;
        }

        public bool IsRewardGained()
        {
            return rewardGained;
        }
        
        public static RewardManager Get()
        {
            return instance;
        }
    }
}