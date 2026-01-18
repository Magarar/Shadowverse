using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using YooAsset;

namespace Data
{
    [CreateAssetMenu(fileName = "New Variant", menuName = "Data/Variant")]
    public class VariantData:ScriptableObject
    {
        public string id;
        public string title;
        public Sprite frame;
        public Sprite frameBoard;
        public Color color = Color.white;
        public int costFactor = 1;
        public bool isDefault;
        
        public static List<VariantData> variantList = new List<VariantData>();

        public static void Load(string folder = "")
        {
            // if (variantList.Count == 0)
            // {
            //     variantList.AddRange(Resources.LoadAll<VariantData>(folder));
            // }

            if (variantList.Count == 0)
            {
                var package = YooAssets.GetPackage("DefaultPackage");
                var location = "brone";
                var handle = package.LoadAllAssetsSync(location);
                foreach (var asset in handle.AllAssetObjects)
                {
                    VariantData variantData = asset as VariantData;
                    variantList.Add(variantData);
                }
                Debug.Log("variantList.Count:" + variantList.Count);
            }
        }

        public static VariantData Get(string id)
        {
            return variantList.Find(x => x.id == id);
        }

        public static VariantData GetDefault()
        {
            return GetAll().FirstOrDefault(variant => variant.isDefault);
        }
        
        public static VariantData GetSpecial()
        {
            return GetAll().FirstOrDefault(variant => !variant.isDefault);
        }

        public static List<VariantData> GetAll()
        {
            return variantList;
        }
    }

 

}