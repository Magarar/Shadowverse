using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace Data
{
    ///<总结>
    ///定义所有固定deck数据（对于用户自定义deck，请检查UserData.cs）
    ///</摘要>
    [CreateAssetMenu(fileName = "New Deck", menuName = "Data/Deck")]
    public class DeckData:ScriptableObject
    {
        public string id;
        
        [Header("title")]
        public string title;

        [Header("Cards")] 
        public CardData hero;
        public CardData[] cards;

        public static List<DeckData> deckList = new List<DeckData>();

        public static void Load(string folder = "")
        {
            // if(deckList.Count == 0)
            //     deckList.AddRange(Resources.LoadAll<DeckData>(folder));

            if (deckList.Count == 0)
            {
                var package = YooAssets.GetPackage("DefaultPackage");
                var location = "TestDeck_1";
                var handle = package.LoadAllAssetsSync(location);
                foreach (var asset in handle.AllAssetObjects)
                {
                    DeckData deckData = asset as DeckData;
                    deckList.Add(deckData);
                }
                Debug.Log("deckList.Count:" + deckList.Count);
            }
        }
        
        public int GetQuantity()=> cards.Length;
        
        public bool IsValid()=> cards.Length >= GamePlayData.Get().deckSize;

        public static DeckData Get(string id)
        {
            return deckList.Find(x => x.id == id);
        }

        public static List<DeckData> GetAll()
        {
            return deckList;
        }
    }

  
}