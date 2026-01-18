using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace Data
{
    /// <summary>
    /// Defines all packs data
    /// </summary>
    ///
    [CreateAssetMenu(fileName = "New Pack", menuName = "Data/Pack")]
    public class PackData:ScriptableObject
    {
        public string id;
        
        [Header("Content")]
        public PackType type;
        public int cards = 8;
        public PackRarity[] rarities1St;//第一张卡的稀有概率
        public PackRarity[] rarities;//其他卡片每种稀有性的概率
        public PackVariant[] variants;

        [Header("Display")]
        public string title;
        public Sprite packImg;
        public Sprite cardBackImg;
        [TextArea(5, 10)]
        public string desc;
        public int sortOrder;
        
        [Header("Availability")]
        public bool available = true;
        public int cost = 100;  //Cost to buy
        
        public static List<PackData> packList = new List<PackData>();

        public static void Load(string folder = "")
        {
            // if (packList.Count == 0)
            // {
            //     packList.AddRange(Resources.LoadAll<PackData>(folder));
            //     packList.Sort((PackData a, PackData b) => {
            //         if (a.sortOrder == b.sortOrder)
            //             return a.id.CompareTo(b.id);
            //         else
            //             return a.sortOrder.CompareTo(b.sortOrder);
            //     });
            // }
            if (packList.Count == 0)
            {
                var package = YooAssets.GetPackage("DefaultPackage");
                var location = "standard";
                var handle = package.LoadAllAssetsSync(location);
                foreach (var asset in handle.AllAssetObjects)
                {
                    PackData packData = asset as PackData;
                    packList.Add(packData);
                }
                packList.Sort((PackData a, PackData b) => {
                        if (a.sortOrder == b.sortOrder)
                            return a.id.CompareTo(b.id);
                        else
                            return a.sortOrder.CompareTo(b.sortOrder);
                });
                Debug.Log("packList.Count:" + packList.Count);
            }
        }
        
        public string GetTitle()
        {
            return title;
        }

        public string GetDesc()
        {
            return desc;
        }

        public static PackData Get(string id)
        {
            return packList.Find(pack => pack.id == id);
        }

        public static List<PackData> GetAvailable()
        {
            return GetAll().Where(apack => apack.available).ToList();
        }

        public static List<PackData> GetAll()
        {
            return packList;
        }
        
    }
    
    public enum PackType
    {
        Random = 0,
        Fixed = 10,
    }

    [Serializable]
    public struct PackRarity
    {
        public RarityData rarity;
        public int probability;
    }

    [Serializable]
    public struct PackVariant
    {
        public VariantData variant;
        public int probability;
    }

    
}