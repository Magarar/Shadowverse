using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using YooAsset;

namespace Data
{
    [CreateAssetMenu(fileName = "RewardData", menuName = "Data/RewardData")]
    public class RewardData:ScriptableObject
    {
        public string id;
        public string group;
        [FormerlySerializedAs("coin")] public int coins;
        public int xp;
        
        public PackData[] packs;
        public CardData[] cards;
        public DeckData[] decks;

        public bool repeat = true;
        
        public static List<RewardData> rewardList = new List<RewardData>();

        public static void Load(string folder = "")
        {
            // if (rewardList.Count == 0)
            // {
            //     rewardList.AddRange(Resources.LoadAll<RewardData>(folder));
            // }

            if (rewardList.Count == 0)
            {
                var package = YooAssets.GetPackage("DefaultPackage");
                var location = "1";
                var handle = package.LoadAllAssetsSync(location);
                
                if (handle.Status == EOperationStatus.Failed)
                {
                    Debug.LogWarning("Load level failed");
                    return;
                }
                
                foreach (var asset in handle.AllAssetObjects)
                {
                    rewardList.Add(asset as RewardData);
                }
            }
        }

        public static List<RewardData> GetAll()
        {
            return rewardList;
        }

        public static RewardData Get(string id)
        {
            return rewardList.Find(x => x.id == id);
        }
    }

    
}