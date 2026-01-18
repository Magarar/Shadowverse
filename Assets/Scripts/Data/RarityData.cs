using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace Data
{
    [CreateAssetMenu(fileName = "New Rarity", menuName = "Data/Rarity")]
    public class RarityData:ScriptableObject
    {
        public string id;
        public string title;
        public Sprite icon;
        public int rank; //稀有指数，应从1（普通）开始，然后依次增加
        
        public static List<RarityData> rarityList = new List<RarityData>();

        public static void Load(string folder = "")
        {
            // if (rarityList.Count == 0)
            // {
            //     rarityList.AddRange(Resources.LoadAll<RarityData>(folder));
            // }

            if (rarityList.Count == 0)
            {
                var package = YooAssets.GetPackage("DefaultPackage");
                var location = "1-brone";
                var handle = package.LoadAllAssetsSync(location);
                foreach (var asset in handle.AllAssetObjects)
                {
                    RarityData rarityData = asset as RarityData;
                    rarityList.Add(rarityData);
                }
                Debug.Log($"Loaded {rarityList.Count} rarities");
            }
        }

        public static RarityData GetFirst()
        {
            int lowest = 99999;
            RarityData first = null;
            foreach (var rarity in GetAll())
            {
                if (rarity.rank < lowest)
                {
                    lowest = rarity.rank;
                    first = rarity;
                }
            }
            return first;
        }

        public static RarityData Get(string id)
        {
            return GetAll().Find(x => x.id == id);
        }

        private static List<RarityData> GetAll()
        {
            return rarityList;
        }
    }

    
}