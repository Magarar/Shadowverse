using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace Data
{
    [CreateAssetMenu(fileName = "New Team", menuName = "Data/Team")]
    public class TeamData:ScriptableObject
    {
        public string id;
        public string title;
        public Sprite icon;
        public Color color;
        
        public static List<TeamData> teamList = new List<TeamData>();
        
        public static void Load(string folder = "")
        {
            // if (teamList.Count == 0)
            //     teamList.AddRange(Resources.LoadAll<TeamData>(folder));
            if (teamList.Count == 0)
            {
                var package = YooAssets.GetPackage("DefaultPackage");
                var location = "Royal";
                var handle = package.LoadAllAssetsSync(location);
                foreach (var asset in handle.AllAssetObjects)
                {
                    TeamData teamData = asset as TeamData;
                    teamList.Add(teamData);
                }
                Debug.Log($"Loaded {teamList.Count} teams");
            }
        }

        public static TeamData Get(string id)
        {
            return teamList.Find(team => team.id == id);
        }

        public static List<TeamData> GetAll()
        {
            return teamList;
        }
    }

 
    
}