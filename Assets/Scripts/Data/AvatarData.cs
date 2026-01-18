using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace Data
{
    [CreateAssetMenu(fileName = "New Avatar", menuName = "Data/Avatar")]
    public class AvatarData:ScriptableObject
    {
        public string id;
        public Sprite avatar;
        public int sortOrder;

        public static List<AvatarData> avatarList = new List<AvatarData>();

        public static void Load(string folder = "")
        {
            // if(avatarList.Count == 0)
            //     avatarList.AddRange(Resources.LoadAll<AvatarData>(folder));
            //
            // avatarList.Sort((a, b) =>
            // {
            //     if (a.sortOrder == b.sortOrder) 
            //         return a.id.CompareTo(b.id); 
            //     else 
            //         return a.sortOrder.CompareTo(b.sortOrder); 
            // });

            if (avatarList.Count == 0)
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
                    avatarList.Add(asset as AvatarData);
                }

                avatarList.Sort((a, b) =>
                {
                    if (a.sortOrder == b.sortOrder)
                        return a.id.CompareTo(b.id);
                    else
                        return a.sortOrder.CompareTo(b.sortOrder);
                });
                Debug.Log($"AvatarData loaded {avatarList.Count}");

            }
        }

        public static AvatarData Get(string id)
        {
            return avatarList.Find(a => a.id == id);
        }

        public static List<AvatarData> GetAll()
        {
            return avatarList;
        }
        
    }

  
}