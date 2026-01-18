using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class DataLoader : MonoBehaviour
    {
        public GamePlayData gamePlayData;
        public AssetData assetData;

        private HashSet<string> cardIds = new();
        private HashSet<string> abilityIds = new();
        private HashSet<string> deckIds = new();

        private static DataLoader instance;

        private void Awake()
        {
            instance = this;
            LoadData();
        }

        private void LoadData()
        {
            //To make loading faster, add a path inside each Load() function, relative to Resources folder
            //For example CardData.Load("Cards");  to only load data inside the Resources/Cards folder
            CardData.Load();
            TeamData.Load();
            RarityData.Load();
            TraitData.Load();
            VariantData.Load();
            PackData.Load();
            LevelData.Load();
            DeckData.Load();
            AbilityData.Load();
            StatusData.Load();
            //AvatarData.Load();
            //CardBackData.Load();
            //RewardData.Load();
            
            CheckCardData();
            CheckAbilityData();
            CheckDeckData();
            CheckVariantData();
        }

        private void CheckCardData()
        {
            cardIds.Clear();
            foreach (var cardData in CardData.GetAll())
            {
                if(string.IsNullOrEmpty(cardData.id))
                    Debug.LogError("CardData: " + cardData.name + " has no ID");
                if (cardIds.Contains(cardData.id))
                    Debug.LogError("CardData: " + cardData.name + " has duplicate ID");
                if(cardData.team==null)
                    Debug.LogError("CardData: " + cardData.name + " has no team");
                if(cardData.rarity==null)
                    Debug.LogError("CardData: " + cardData.name + " has no rarity");

                if (cardData.traitDatas != null)
                {
                    foreach (TraitData trait in cardData.traitDatas)
                    {
                        if (trait == null)  
                            Debug.LogError(cardData.id + " has null trait");
                    }
                }

                if (cardData.traitStats != null)
                {
                    foreach (TraitStat traitStat in cardData.traitStats)
                    {
                        if (traitStat.trait == null)
                            Debug.LogError(cardData.id + " has null trait");
                    }
                }

                if (cardData.abilityDatas != null)
                {
                    foreach (AbilityData abilityData in cardData.abilityDatas)
                    {
                        if (abilityData == null)
                            Debug.LogError(cardData.id + " has null ability");
                    }
                }
                
                cardIds.Add(cardData.id);
            }
        }

        private void CheckAbilityData()
        {
            abilityIds.Clear();
            foreach (var abilityData in AbilityData.GetAll())
            {
                if (string.IsNullOrEmpty(abilityData.id))
                    Debug.LogError("AbilityData: " + abilityData.name + " has no ID");
                if (abilityIds.Contains(abilityData.id))
                    Debug.LogError("AbilityData: " + abilityData.name + " has duplicate ID");

                foreach (var chain in abilityData.chainAbilities)
                {
                    if (chain == null)
                        Debug.LogError("AbilityData: " + abilityData.name + " has null chain ability");
                }
                abilityIds.Add(abilityData.id);
            }
        }

        private void CheckDeckData()
        {
            GamePlayData gameData = GamePlayData.Get();
            CheckDeckArray(gameData.aiDecks);
            CheckDeckArray(gameData.freeDecks);
            CheckDeckArray(gameData.starterDecks);

            if (gameData.testDeck == null || gameData.testDeckAi == null)
            {
                Debug.Log("Deck is null in Resources/GameplayData");
            }
            
            deckIds.Clear();
            foreach (var deckData in DeckData.GetAll())
            {
                if (string.IsNullOrEmpty(deckData.id))
                    Debug.LogError("DeckData: " + deckData.name + " has no ID");
                if (deckIds.Contains(deckData.id))
                    Debug.LogError("DeckData: " + deckData.name + " has duplicate ID");
                foreach (var cardData in deckData.cards)
                {
                    if (cardData == null)
                        Debug.LogError("DeckData: " + deckData.name + " has null card");
                }
                deckIds.Add(deckData.id);
            }
        }
        
        private void CheckDeckArray(DeckData[] decks)
        {
            foreach (DeckData deck in decks)
            {
                if (deck == null)
                    Debug.Log("Deck is null in Resources/GameplayData");
            }
        }

        private void CheckVariantData()
        {
            VariantData dvariant = VariantData.GetDefault();
            if(dvariant == null)
                Debug.LogError("No default variant data found, make sure you have a default VariantData");
        }
        
        public static DataLoader Get()
        {
            return instance;
        }
    }
}
