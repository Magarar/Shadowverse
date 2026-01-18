using System.Collections.Generic;
using Data;
using Network;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    /// <summary>
    /// Deck selector is a dropdown that let the player select a deck before a match
    /// </summary>
    public class DeckSelector:MonoBehaviour
    {
        public DropdownValue deckDropdown;
        
        public UnityAction<string> onChange;
        
        void Start()
        {
            deckDropdown.onValueChanged += OnChange;
        }

        public void SetupUserDeckList()
        {
            deckDropdown.ClearOptions();
            
            deckDropdown.AddOption("random", "Random");
            
            //Add standard decks
            foreach (DeckData deck in GamePlayData.Get().freeDecks)
            {
                deckDropdown.AddOption(deck.id, deck.title);
            }
            
            UserData udata = Authenticator.Get().UserData;
            if (udata != null)
            {
                foreach (UserDeckData deck in udata.decks)
                {
                    if (udata.IsDeckValid(deck))
                    {
                        deckDropdown.AddOption(deck.tid, deck.title);
                    }
                }
            }
        }

        public void SetupAIDeckList()
        {
            deckDropdown.ClearOptions();
            deckDropdown.AddOption("random_ai", "Random");
            //Add standard decks
            foreach (DeckData deck in GamePlayData.Get().aiDecks)
            {
                deckDropdown.AddOption(deck.id, deck.title);
            }
            
            //Also add user made decks
            UserData udata = Authenticator.Get().UserData;
            if (udata != null)
            {
                foreach (UserDeckData deck in udata.decks)
                {
                    if (udata.IsDeckValid(deck))
                    {
                        deckDropdown.AddOption(deck.tid, deck.title);
                    }
                }
            }
        }
        
        private void SelectDeck(UserDeckData deck)
        {
            if (deck != null)
            {
                deckDropdown.SetValue(deck.tid);
            }
        }

        private void SelectDeck(DeckData deck)
        {
            if (deck != null)
            {
                deckDropdown.SetValue(deck.id);
            }
        }

        public void SelectDeck(string deck)
        {
            UserData udata = Authenticator.Get().UserData;
            UserDeckData udeck = udata.GetDeck(deck);

            if (udeck != null)
            {
                SelectDeck(udeck);
                return;
            }
            DeckData ddeck = DeckData.Get(deck);
            if (ddeck != null)
                SelectDeck(ddeck);
        }
        
        public void SelectDeck(int index)
        {
            deckDropdown.SetValue(index);
        }
        
        public void Lock()
        {
            deckDropdown.interactable = false;
        }
        
        public void Unlock()
        {
            deckDropdown.interactable = true;
        }
        
        public void SetLocked(bool locked)
        {
            deckDropdown.interactable = !locked;
        }
        
        private void OnChange(int i, string val)
        {
            string value = deckDropdown.GetSelectedValue();
            onChange?.Invoke(value);
        }
        
        public string GetDeckID()
        {
            return deckDropdown.GetSelectedValue();
        }
        
        public string GetDeckTitle()
        {
            return deckDropdown.GetSelectedText();
        }

        public UserDeckData GetDeckById(string deckID)
        {
            UserData user = Authenticator.Get().UserData;
            UserDeckData udeck = user.GetDeck(deckID); //Check for user custom deck
            DeckData deck = DeckData.Get(deckID);     //Check for deck presets

            //User custom deck
            if (udeck != null)
                return udeck;
            //Premade deck
            else if (deck != null)
                return new UserDeckData(deck);
            return null;
        }

        public UserDeckData GetDeck()
        {
            string deckID = GetDeckID();

            //Random Reck
            if (deckID == "random")
                return GetRandomDeck();
            if (deckID == "random_ai")
                return GetRandomDeckAI();

            return GetDeckById(deckID);
        }

        public UserDeckData GetRandomDeck()
        {
            List<UserDeckData> randomDecks = new List<UserDeckData>();
            foreach (DropdownValueItem item in deckDropdown.Items)
            {
                UserDeckData deck = GetDeckById(item.id);
                if (deck != null)
                    randomDecks.Add(deck);
            }

            if (randomDecks.Count > 0)
                return randomDecks[Random.Range(0, randomDecks.Count)];
            return null;
        }
        
        public UserDeckData GetRandomDeckAI()
        {
            return new UserDeckData(GamePlayData.Get().GetRandomAIDeck());
        }
    }
}