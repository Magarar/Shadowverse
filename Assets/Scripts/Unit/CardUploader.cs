using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unit
{
    /// <summary>
    /// Use this tool to upload your cards, packs and rewards to the Mongo Database (it will overwrite existing data)
    /// </summary>
    public class CardUploader:MonoBehaviour
    {
        public string username = "admin";
        
        [Header("References")]
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public TextMeshProUGUI msgText;
        
        [Header("Uploader")]
        public bool uploadCards = true;
        public bool uploadPacks = true;
        public bool uploadRewards = true;
        public bool uploadDecks = true;
        public bool uploadVariants = true;

        void Start()
        {
            usernameInput.text = username;
            msgText.text = "";
        }
        
        private async void Login()
        {
            LoginResponse res = await ApiClient.Get().Login(usernameInput.text, passwordInput.text);
            if (res.success && res.permissionLevel >= 10)
            {
                UploadAll();
            }
            else
            {
                ShowText("Admin Login Failed");
            }
        }

        private async void UploadAll()
        {
            ShowText("Deleting previous data...");
            
            if(uploadPacks)
                await DeleteAllPacks();
            if(uploadCards)
                await DeleteAllCards();
            if(uploadDecks)
                await DeleteAllDecks();
            if(uploadRewards)
                await DeleteAllRewards();

            if (uploadCards)
            {
                List<PackData> packs = PackData.GetAll();
                foreach (var pack in packs)
                {
                    if (pack.available)
                    {
                        ShowText("Uploading Packs: " + pack.id);
                        UploadPack(pack);
                        await TimeTool.Delay(100);
                    }
                }
            }

            if (uploadCards)
            {
                List<CardData> cards = CardData.GetAll();
                for (int i = 0; i < cards.Count; i += 100)
                {
                    List<CardData> list = GetCardGroup(cards, i, 100);
                    ShowText("Uploading Cards: " + i + "-" + (i + 100 - 1));
                    UploadCardList(list);
                    await TimeTool.Delay(200);
                }
            }
            
            if (uploadVariants)
            {
                List<VariantData> variants = VariantData.GetAll();
                foreach (var variant in variants)
                {
                    ShowText("Uploading Variant: " + variant.id);
                    UploadVariant(variant);
                    await TimeTool.Delay(100);
                }
            }

            if (uploadDecks)
            {
                DeckData[] decks = GamePlayData.Get().starterDecks;
                foreach (var deck in decks)
                {
                    ShowText("Uploading Deck: " + deck.id);
                    UploadDeck(deck);
                    UploadDeckReward(deck);
                    await TimeTool.Delay(100);
                }
            }
            
            if (uploadRewards)
            {
                List<LevelData> levels = LevelData.GetAll();
                for (int i = 0; i < levels.Count; i++)
                {
                    LevelData level = levels[i];
                    ShowText("Uploading Reward: " + level.id);
                    UploadLevelReward(level);

                    if (level.rewardDecks != null)
                    {
                        foreach (DeckData deck in level.rewardDecks)
                            UploadDeck(deck);
                    }

                    await TimeTool.Delay(100);
                }
            }

            //Custom rewards
            if (uploadRewards)
            {
                List<RewardData> rewards = RewardData.GetAll();
                for (int i = 0; i < rewards.Count; i++)
                {
                    RewardData reward = rewards[i];
                    ShowText("Uploading Reward: " + reward.id);
                    UploadReward(reward);

                    foreach (DeckData deck in reward.decks)
                        UploadDeck(deck);

                    await TimeTool.Delay(100);
                }
            }
            
            ShowText("Completed!");
            ApiClient.Get().Logout();
        }

        private async Task DeleteAllPacks()
        {
            string url = ApiClient.ServerURL + "/packs";
            await ApiClient.Get().SendRequest(url, WebRequest.MethodDelete);
        }

        private async Task DeleteAllCards()
        {
            string url = ApiClient.ServerURL + "/cards";
            await ApiClient.Get().SendRequest(url, WebRequest.MethodDelete);
        }

        private async Task DeleteAllRewards()
        {
            string url = ApiClient.ServerURL + "/rewards";
            await ApiClient.Get().SendRequest(url, WebRequest.MethodDelete);
        }

        private async Task DeleteAllDecks()
        {
            string url = ApiClient.ServerURL + "/decks";
            await ApiClient.Get().SendRequest(url, WebRequest.MethodDelete);
        }

        private async Task DeleteAllVariants()
        {
            string url = ApiClient.ServerURL + "/variants";
            await ApiClient.Get().SendRequest(url, WebRequest.MethodDelete);
        }

        private async void UploadPack(PackData pack)
        {
            PackAddRequest req = new PackAddRequest()
            {
                tid = pack.id,
                cards = pack.cards,
                cost = pack.cost,
                random = pack.type == PackType.Random,
                rarities_1st = new PackAddProbability[pack.rarities1St.Length],
                rarities = new PackAddProbability[pack.rarities.Length],
                variants = new PackAddProbability[pack.variants.Length]
            };
            for (int i = 0; i < req.rarities_1st.Length; i++)
                req.rarities_1st[i] = AddPackRarity(pack.rarities1St[i]);
            for (int i = 0; i < req.rarities.Length; i++)
                req.rarities[i] = AddPackRarity(pack.rarities[i]);
            
            string url = ApiClient.ServerURL + "/packs/add";
            string json = ApiTool.ToJson(req);
            await ApiClient.Get().SendPostRequest(url, json);
        }

        private PackAddProbability AddPackRarity(PackRarity rarity)
        {
            PackAddProbability add = new PackAddProbability();
            add.tid = rarity.rarity.id;
            add.value = rarity.probability;
            return add;
        }
        
        private PackAddProbability AddPackVariant(PackVariant rarity)
        {
            PackAddProbability add = new PackAddProbability();
            add.tid = rarity.variant.id;
            add.value = rarity.probability;
            return add;
        }
        
        private async void UploadCard(CardData card)
        {
            CardAddRequest req = new CardAddRequest
            {
                tid = card.id,
                type = card.GetTypeID(),
                team = card.team.id,
                rarity = card.rarity.id,
                mana = card.mana,
                attack = card.attack,
                hp = card.hp,
                cost = card.cost,
                packs = new string[card.packs.Length]
            };

            for (int i = 0; i < req.packs.Length; i++)
            {
                req.packs[i] = card.packs[i].id;
            }

            string url = ApiClient.ServerURL + "/cards/add";
            string json = ApiTool.ToJson(req);
            await ApiClient.Get().SendPostRequest(url, json);
        }
        
        private async void UploadCardList(List<CardData> cards)
        {
            CardAddListRequest req = new CardAddListRequest
            {
                cards = new CardAddRequest[cards.Count]
            };
            for(int i=0; i<cards.Count; i++)
            {
                CardData card = cards[i];
                CardAddRequest rcard = new CardAddRequest();
                rcard.tid = card.id;
                rcard.type = card.GetTypeID();
                rcard.team = card.team.id;
                rcard.rarity = card.rarity.id;
                rcard.mana = card.mana;
                rcard.attack = card.attack;
                rcard.hp = card.hp;
                rcard.cost = card.cost;
                rcard.packs = new string[card.packs.Length];
                for (int p = 0; p < card.packs.Length; p++)
                {
                    rcard.packs[p] = card.packs[p].id;
                }
                req.cards[i] = rcard;
            }

            string url = ApiClient.ServerURL + "/cards/add/list";
            string json = ApiTool.ToJson(req);
            await ApiClient.Get().SendPostRequest(url, json);
        }
        
        private async void UploadVariant(VariantData variant)
        {
            VariantAddRequest req = new VariantAddRequest
            {
                tid = variant.id,
                costFactor = variant.costFactor,
                isDefault = variant.isDefault
            };

            string url = ApiClient.ServerURL + "/variants/add";
            string json = ApiTool.ToJson(req);
            await ApiClient.Get().SendPostRequest(url, json);
        }
        
        private async void UploadDeckReward(DeckData deck)
        {
            RewardAddRequest req = new RewardAddRequest
            {
                tid = deck.id,
                group = "starter_deck",
                decks = new string[1] { deck.id }
            };

            string url = ApiClient.ServerURL + "/rewards/add";
            string json = ApiTool.ToJson(req);
            await ApiClient.Get().SendPostRequest(url, json);
        }
        
        private async void UploadDeck(DeckData deck)
        {
            UserDeckData req = new UserDeckData(deck);
             string url = ApiClient.ServerURL + "/decks/add";
             string json = ApiTool.ToJson(req);
             await ApiClient.Get().SendPostRequest(url, json);
        }
        
        private async void UploadReward(RewardData reward)
        {
            RewardAddRequest req = new RewardAddRequest
            {
                tid = reward.id,
                group = "",
                coins = reward.coins,
                xp = reward.xp,
                repeat = reward.repeat
            };

            if (reward.cards != null)
            {
                req.cards = new string[reward.cards.Length];
                for (int i = 0; i < reward.cards.Length; i++)
                {
                    req.cards[i] = reward.cards[i].id;
                }
            }

            if (reward.decks != null)
            {
                req.decks = new string[reward.decks.Length];
                for (int i = 0; i < reward.decks.Length; i++)
                {
                    req.decks[i] = reward.decks[i].id;
                }
            }

            if (reward.packs != null)
            {
                req.packs = new string[reward.packs.Length];
                for (int i = 0; i < reward.packs.Length; i++)
                {
                    req.packs[i] = reward.packs[i].id;
                }
            }

            string url = ApiClient.ServerURL + "/rewards/add";
            string json = ApiTool.ToJson(req);
            await ApiClient.Get().SendPostRequest(url, json);
        }
        
        private async void UploadLevelReward(LevelData level)
        {
            RewardAddRequest req = new RewardAddRequest();
            req.tid = level.id;
            req.group = "";
            req.coins = level.rewardCoins;
            req.xp = level.rewardXp;
            req.repeat = false;

            if (level.rewardCards != null)
            {
                req.cards = new string[level.rewardCards.Length];
                for (int i = 0; i < level.rewardCards.Length; i++)
                {
                    req.cards[i] = level.rewardCards[i].id;
                }
            }

            if (level.rewardPacks != null)
            {
                req.packs = new string[level.rewardPacks.Length];
                for (int i = 0; i < level.rewardPacks.Length; i++)
                {
                    req.packs[i] = level.rewardPacks[i].id;
                }
            }

            if (level.rewardDecks != null)
            {
                req.decks = new string[level.rewardDecks.Length];
                for (int i = 0; i < level.rewardDecks.Length; i++)
                {
                    req.decks[i] = level.rewardDecks[i].id;
                }
            }

            string url = ApiClient.ServerURL + "/rewards/add";
            string json = ApiTool.ToJson(req);
            await ApiClient.Get().SendPostRequest(url, json);
        }
        
        private List<CardData> GetCardGroup(List<CardData> allCards, int start, int count)
        {
            List<CardData> list = new List<CardData>();
            for (int i = 0; i < count; i++)
            {
                int index = start + i;
                if (index < allCards.Count)
                {
                    CardData card = allCards[index];
                    if (card.deckbuilding)
                    {
                        list.Add(card);
                    }
                }
            }
            return list;
        }
        
        public void OnClickStart()
        {
            msgText.text = "";
            Login();
        }

        private void ShowText(string txt)
        {
            Debug.Log(txt);
        }
    }
    
    [Serializable]
    public class CardAddListRequest
    {
        public CardAddRequest[] cards;
    }
    
    [Serializable]
    public class CardAddRequest
    {
        public string tid;
        public string type;
        public string team;
        public string rarity;
        public int mana;
        public int attack;
        public int hp;
        public int cost;
        public string[] packs;
    }
    
    [Serializable]
    public class PackAddRequest
    {
        public string tid;
        public int cards;
        public int cost;
        public bool random;
        public PackAddProbability[] rarities_1st;
        public PackAddProbability[] rarities;
        public PackAddProbability[] variants;
    }
    
    [Serializable]
    public class PackAddProbability
    {
        public string tid;
        public int value;
    }
    
    [Serializable]
    public class VariantAddRequest
    {
        public string tid;
        public int costFactor;
        public bool isDefault;
    }

    [Serializable]
    public class RewardAddRequest
    {
        public string tid;
        public string group;
        public int coins;
        public int xp;
        public string[] packs;
        public string[] cards;
        public string[] decks;
        public bool repeat;
    }
}