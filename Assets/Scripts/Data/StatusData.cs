using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace Data
{
    ///<总结>
    ///定义所有状态效果数据
    ///状态是可以通过能力获得或失去的效果，这将影响游戏玩法
    ///状态可以有一个持续时间
    ///</摘要>
    [CreateAssetMenu(fileName = "New Status", menuName = "Data/Status")]
    public class StatusData:ScriptableObject
    {
        public StatusType effect;
        
        [Header("Display")]
        public string title;
        public Sprite icon;
        
        [TextArea(3, 5)]
        public string desc;
        
        [Header("FX")]
        public GameObject statusFX;
        
        [Header("AI")]
        public int hValue;
        
        public static List<StatusData> statusList = new List<StatusData>();

        public static void Load(string folder = "")
        {
            // if (statusList.Count == 0)
            //     statusList.AddRange(Resources.LoadAll<StatusData>(folder));

            if (statusList.Count == 0)
            {
                var package = YooAssets.GetPackage("DefaultPackage");
                var location = "Shell";
                var handle = package.LoadAllAssetsSync(location);
                foreach (var asset in handle.AllAssetObjects)
                {
                    StatusData statusData = asset as StatusData;
                    statusList.Add(statusData);
                }
                Debug.Log("statusList.Count:" + statusList.Count);
            }
        }
        
        public string GetTitle() => title;
        
        public string GetDesc() => GetDesc(1);
        
        public string GetDesc(int value)
        {
            string des = desc.Replace("<value>", value.ToString());
            return des;
        }

        public static StatusData Get(StatusType effect)
        {
            return statusList.Find(x => x.effect== effect);
        }

        public static List<StatusData> GetAll()
        {
            return statusList;
        }
    }

    
    
    public enum StatusType
    {
        None = 0,

        AddAttack = 4,      //Attack status can be used for attack boost limited for X turns 
        AddHP = 5,          //HP status can be used for hp boost limited for X turns 
        AddManaCost = 6,    //Mana Cost status can be used for Mana cost increase/reduction limited for X turns 

        Stealth = 10,       //Cant be attacked until do action
        Invincibility = 12, //Cant be attacked for X turns
        Shell = 13,         //Receives no damaged the first time
        Protection = 14,    //Taunt, gives Protected to other cards
        Protected = 15,     //Cards that are protected by taunt
        Armor = 16,         //Receives less damaged
        SpellImmunity = 18, //Cant be targeted/damaged by spells

        Deathtouch = 20,    //Kills when attacking a character
        Fury = 22,          //Can attack twice per turn
        Intimidate = 23,    //Target doesnt counter when attacking
        Flying = 24,         //Can ignore taunt
        Trample = 26,         //Extra damaged is assigned to player
        LifeSteal = 28,      //Heal player when fighting

        Silenced = 30,      //All abilities canceled
        Paralysed = 32,     //Cant do any actions for X turns
        Poisoned = 34,     //Lose hp each start of turn
        Sleep = 36,         //Doesnt untap at the start of turn
        Evolved = 38,
        SuperEvolved = 40,


    }
}