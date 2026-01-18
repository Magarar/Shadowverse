using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace Data
{
    ///<总结>
    ///定义所有特征和统计数据
    ///</摘要>
    ///
    [CreateAssetMenu(fileName = "TraitData", menuName = "Data/TraitData",order = 1)]
    public class TraitData:ScriptableObject
    {
        public string id;
        public string title;
        public Sprite icon;
        
        public static List<TraitData> traitList = new List<TraitData>();
        
        public string GetTitle() { return title; }
        public Sprite GetIcon() { return icon; }

        public static void Load(string folder = "")
        {
            // if (traitList.Count == 0)
            //     traitList.AddRange(Resources.LoadAll<TraitData>(folder));

            if (traitList.Count == 0)
            {
                var package = YooAssets.GetPackage("DefaultPackage");
                var location = "士兵";
                var handle = package.LoadAllAssetsSync(location);
                foreach (var asset in handle.AllAssetObjects)
                {
                    TraitData traitData = asset as TraitData;
                    traitList.Add(traitData);
                }
                Debug.Log("traitList.Count:" + traitList.Count);
            }
        }

        public static TraitData Get(string id)
        {
            return traitList.Find(traitData => traitData.id == id);
        }

        public static List<TraitData> GetAll()
        {
            return traitList;
        }
    }



    [Serializable]
    public struct TraitStat
    {
        public TraitData trait;
        public int value;
    }
}