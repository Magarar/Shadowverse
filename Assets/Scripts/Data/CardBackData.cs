using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace Data
{
    [CreateAssetMenu(fileName = "New CardBack", menuName = "Data/CardBack")]
    public class CardBackData:ScriptableObject
    {
        public string id;
        public Sprite cardBack;
        public Sprite deck;
        public int sortOrder;

        public static List<CardBackData> cardBackList = new List<CardBackData>();

        public static void Load(string folder = "")
        {
            // if(cardBackList.Count == 0)
            //     cardBackList.AddRange(Resources.LoadAll<CardBackData>(folder));

            if (cardBackList.Count == 0)
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
                    cardBackList.Add(asset as CardBackData);
                }
            }
            
            cardBackList.Sort((a, b) =>
            {
                if(a.sortOrder == b.sortOrder)
                    return a.id.CompareTo(b.id);
                else
                {
                    return a.sortOrder.CompareTo(b.sortOrder);
                }
            });
        }

        public static List<CardBackData> GetAll()
        {
            return cardBackList;
        }

        public static CardBackData Get(string id)
        {
            return cardBackList.Find(x => x.id == id);
        }
    }
    
   
}