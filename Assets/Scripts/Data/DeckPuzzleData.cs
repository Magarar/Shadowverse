using System;
using System.Collections.Generic;
using System.Reflection;
using GameLogic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Deck with more fields for having specific cards or starting board cards
    /// </summary>
    [CreateAssetMenu(fileName = "New DeckPuzzle", menuName = "Data/DeckPuzzle")]
    public class DeckPuzzleData:DeckData
    {
        public DeckCardSlot[] boardCards;
        public int startCards = 4;
        public int startMana = 2;
        public int startHp = 20;
        public bool dontShuffleDeck;
        
        public new static DeckPuzzleData Get(string id)
        {
            foreach (DeckData deck in GetAll())
            {
                if (deck.id == id && deck is DeckPuzzleData)
                    return (DeckPuzzleData) deck;
            }
            return null;
        }
    }
    
    [Serializable]
    public class DeckCardSlot
    {
        public CardData card;
        public SlotXY slot;
    }

    
}