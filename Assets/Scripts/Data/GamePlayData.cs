using System;
using System.Collections.Generic;
using System.Reflection;
using Ai;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Data
{
    ///<总结>
    ///通用游戏设置，如起始统计数据、牌组限制、场景和ai级别
    ///</摘要>
    [CreateAssetMenu(fileName = "GamePlayData", menuName = "Data/GamePlayData")]
    public class GamePlayData:ScriptableObject
    {
        [Header("GamePlay")]
        public int hpStart = 20;
        public int manaStart = 1;
        public int manaPerTurn = 1;
        public int manaMax = 10;
        public int cardsStart = 4;
        public int cardPerTurn = 1;
        public int cardMax = 9;
        public float turnDuration = 60f;
        public CardData secondBouns;
        public bool mulligan;

        [Header("Deckbuilding")] 
        public int deckSize = 30;
        public int deckDuplicateNax = 3;
        
        [Header("Buy/Sell")]
        public float sellRatio = 0.8f;
        
        [Header("AI")]
        public AIType aiType = AIType.MiniMax;
        public int aiLevel = 10; //AI level, 10=best, 1=weakest
        
        [Header("Decks")]
        public DeckData[] freeDecks;
        public DeckData[] starterDecks;
        public DeckData[] aiDecks;
        
        [Header("Scenes")]
        public string[] arenaList;  
        
        [Header("Test")]
        public DeckData testDeck; 
        public DeckData testDeckAi;
        public bool aiVsAi;
        
        public int GetPlayerLevel(int xp)
        {
            return Mathf.FloorToInt(xp / 1000f) + 1;
        }
        
        public string GetRandomArena()
        {
            if (arenaList.Length > 0)
                return arenaList[Random.Range(0, arenaList.Length)];
            return "Game";
        }

        public DeckData GetRandomFreeDeck()
        {
            if (freeDecks.Length > 0)
            {
                return freeDecks[Random.Range(0, freeDecks.Length)];
            }
            return null;
        }

        public DeckData GetRandomAIDeck()
        {
            if (aiDecks.Length > 0)
            {
                return aiDecks[Random.Range(0, aiDecks.Length)];
            }
            return null;
        }

        public static GamePlayData Get()
        {
            return DataLoader.Get().gamePlayData;   
        }
        
    }
    
    

    
}