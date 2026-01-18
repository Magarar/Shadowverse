using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using YooAsset;

namespace Data
{
    [CreateAssetMenu(fileName = "New Level", menuName = "Data/Level")]
    public class LevelData:ScriptableObject
    {
        public string id;
        public int level;
        
        [Header("Display")]
        public string title;

        [Header("Gameplay")] 
        public string scene;
        public DeckData playerDeck;
        public DeckData aiDeck;
        public int aiLevel;
        public LevelFirst firstPlayer;
        public GameObject tutoPrefab;
        public bool mulligan = false;
        
        [Header("Rewards")]
        public int rewardXp = 100;
        public int rewardCoins = 100;
        public PackData[] rewardPacks;
        public CardData[] rewardCards;
        public DeckData[] rewardDecks;
        
        public static List<LevelData> levelList = new List<LevelData>();

        public static void Load(string folder = "")
        {
            // if (levelList.Count == 0)
            // {
            //     levelList.AddRange(Resources.LoadAll<LevelData>(folder));
            //     levelList.Sort((a, b) => a.level.CompareTo(b.level));
            // }
            if (levelList.Count == 0)
            {
                var package = YooAssets.GetPackage("DefaultPackage");
                var location = "standard";
                var handle = package.LoadAllAssetsSync(location);
                
                if (handle.Status == EOperationStatus.Failed)
                {
                    Debug.LogWarning("Load level failed");
                    return;
                }
                
                foreach (var asset in handle.AllAssetObjects)
                {
                    LevelData levelData = asset as LevelData;
                    levelList.Add(levelData);
                }
                
                levelList.Sort((a, b) => a.level.CompareTo(b.level));
                Debug.Log("levelList.Count:" + levelList.Count);
            }
        }
        
        public string GetTitle() => title;

        public static LevelData Get(string id)
        {
            return levelList.Find(x => x.id == id);
        }

        public static List<LevelData> GetAll()
        {
            return levelList;
        }
    }
    
    public enum LevelFirst
    {
        Random = 0,
        Player = 10,
        AI = 20,
    }

   
}