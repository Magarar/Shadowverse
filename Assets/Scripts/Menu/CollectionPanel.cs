using System.Collections.Generic;
using System.Linq;
using Api;
using Data;
using Network;
using TMPro;
using UI;
using Unit;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class CollectionPanel:UIPanel
    {
        [Header("Cards")]
        public ScrollRect scrollRect;
        public RectTransform scrollContent;
        public CardGrid gridContent;
        public GameObject cardPrefab;
        
        [Header("Left Side")]
        public IconButton[] teamFilters;
        public Toggle toggleOwned;
        public Toggle toggleNotOwned;
        
        public Toggle toggleCharacter;
        public Toggle toggleSpell;
        
        public Toggle toggleCommon;
        public Toggle toggleUncommon;
        public Toggle toggleRare;
        public Toggle toggleMythic;
        
        public Toggle toggleFoil;

        public TMP_Dropdown sortDropdown;
        public TMP_InputField search;
        
        [Header("Right Side")]
        public UIPanel deckListPanel;
        public UIPanel cardListPanel;
        public DeckLine[] deckLines;
        
        [Header("Deckbuilding")]
        public TMP_InputField deckTitle;
        public TextMeshProUGUI deckQuantity;
        public GameObject deckCardsPrefab;
        public RectTransform deckContent;
        public GridLayoutGroup deckGrid;
        public IconButton[] heroPowers;
        
        private TeamData filterTeam = null;
        private int filterDropdown = 0;
        private string filterSearch = "";
        
        private List<CollectionCard> cardList = new List<CollectionCard>();
        private List<CollectionCard> allList = new List<CollectionCard>();
        private List<DeckLine> deckCardLines = new List<DeckLine>();
        
        private string currentDeckTid;
        private bool editingDeck = false;
        private bool saving = false;
        private bool spawned = false;
        private bool updateGrid = false;
        private float updateGridTimer = 0f;

        private List<UserCardData> deckCards = new List<UserCardData>();

        private static CollectionPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            
            //Delete grid content
            for (int i = 0; i < gridContent.transform.childCount; i++)
            {
                Destroy(gridContent.transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < deckGrid.transform.childCount; i++)
            {
                Destroy(deckGrid.transform.GetChild(i).gameObject);
            }
            
            foreach (DeckLine line in deckLines)
                line.onClick += OnClickDeckLine;
            foreach (DeckLine line in deckLines)
                line.onClickDelete += OnClickDeckDelete;
            
            foreach (IconButton button in teamFilters)
                button.onClick += OnClickTeam;
        }

        protected override void Start()
        {
            base.Start();
            
            //Set power abilities hover text
            foreach (IconButton btn in heroPowers)
            {
                CardData icard = CardData.Get(btn.value);
                HoverTargetUI hover = btn.GetComponent<HoverTargetUI>();
                AbilityData iability = icard?.GetAbility(AbilityTrigger.Activate);
                if (icard != null && hover != null && iability != null)
                {
                    string color = ColorUtility.ToHtmlStringRGBA(icard.team.color);
                    hover.text = "<b><color=#" + color + ">Hero Power: </color>";
                    hover.text += icard.title + "</b>\n " + iability.GetDesc(icard);
                    if (iability.manaCost > 0)
                        hover.text += " <size=16>Mana: " + iability.manaCost + "</size>";
                }
            }
        }
        
        protected override void Update()
        {
            base.Update();

        }

        private void LateUpdate()
        {
            //Resize grid
            updateGridTimer += Time.deltaTime;
            if (updateGrid && updateGridTimer > 0.2f)
            {
                gridContent.GetColumnAndRow(out int rows, out int cols);
                if (cols > 0)
                {
                    float rowHeight = gridContent.GetGrid().cellSize.y + gridContent.GetGrid().spacing.y;
                    float height = rows * rowHeight;
                    scrollContent.sizeDelta = new Vector2(scrollContent.sizeDelta.x, height + 100);
                    updateGrid = false;
                }
            }
        }

        private void SpawnCards()
        {
            spawned = true;
            foreach (CollectionCard card in allList)
                Destroy(card.gameObject);
            allList.Clear();
            
            foreach (VariantData variant in VariantData.GetAll())
            {
                foreach (CardData card in CardData.GetAll())
                {
                    GameObject nCard = Instantiate(cardPrefab, gridContent.transform);
                    CollectionCard dCard = nCard.GetComponent<CollectionCard>();
                    dCard.SetCard(card, variant, 0);
                    dCard.onClick += OnClickCard;
                    dCard.onClickRight += OnClickCardRight;
                    allList.Add(dCard);
                    nCard.SetActive(false);
                }
            }
        }
        
        //----- Reload User Data ---------------
        public async void ReloadUser()
        {
            await Authenticator.Get().LoadUserData();
            MainMenu.Get().RefreshDeckList();
            RefreshCardsQuantities();
            if (!editingDeck)
                RefreshDeckList();
        }
        
        public async void ReloadUserCards()
        {
            await Authenticator.Get().LoadUserData();
            RefreshCardsQuantities();
        }

        public async void ReloadUserDecks()
        {
            await Authenticator.Get().LoadUserData();
            MainMenu.Get().RefreshDeckList();
            RefreshDeckList();
        }
        
        //----- Refresh UI -------
        private void RefreshAll()
        {
            RefreshFilters();
            RefreshCards();
            RefreshDeckList();
            RefreshStarterDeck();
        }

        private void RefreshFilters()
        {
            search.text = "";
            sortDropdown.value = 0;
            foreach (IconButton button in teamFilters)
                button.Deactivate();
            
            filterTeam = null;
            filterDropdown = 0;
            filterSearch = "";
        }
        
        private void ShowDeckList()
        {
            deckListPanel.Show();
            cardListPanel.Hide();
            editingDeck = false;
        }

        private void ShowDeckCards()
        {
            cardListPanel.Show();
            deckListPanel.Hide();
        }

        public void RefreshCards()
        {
            if(!spawned)
                SpawnCards();
            
            cardList.Clear();
            UserData udata = Authenticator.Get().UserData;
            if (udata == null)
                return;
            
           // VariantData variant = VariantData.GetDefault();
            VariantData special = VariantData.GetSpecial();
            // if (toggleFoil.isOn && special != null)
            //     variant = special;
            
            List<CardDataQ> allCards = new List<CardDataQ>();
            List<CardDataQ> shownCards = new List<CardDataQ>();
            
            foreach (CardData icard in CardData.GetAll())
            {
                CardDataQ card = new CardDataQ();
                card.card = icard;
                card.variant = icard.GetVariant();
                card.quantity = udata.GetCardQuantity(icard,  icard.GetVariant());
                allCards.Add(card);
            }
            if (filterDropdown == 0) //Name
                allCards.Sort((a, b) => a.card.title.CompareTo(b.card.title));
            if (filterDropdown == 1) //Attack
                allCards.Sort((a, b) => a.card.attack.CompareTo(b.card.attack));
            if (filterDropdown == 2) //HP
                allCards.Sort((a, b) => a.card.hp.CompareTo(b.card.hp));
            if (filterDropdown == 3) //Mana
                allCards.Sort((a, b) => a.card.mana.CompareTo(b.card.mana));

            foreach (CardDataQ card in allCards)
            {
                if (card.card.deckbuilding)
                {
                    CardData icard = card.card;
                    if (filterTeam == null || filterTeam == icard.team)
                    {
                        bool owned = card.quantity > 0;
                        RarityData rarity = icard.rarity;
                        CardType type = icard.type;
                        
                        bool ownedCheck =  (owned && toggleOwned.isOn)||
                                           (!owned && toggleNotOwned.isOn)||
                                           toggleOwned.isOn==toggleNotOwned.isOn;
                        
                        bool typeCheck = (type == CardType.Character && toggleCharacter.isOn)
                                          || (type == CardType.Spell && toggleSpell.isOn)
                                          || (!toggleCharacter.isOn && !toggleSpell.isOn);
                        
                        bool rarityCheck = (rarity.rank == 1 && toggleCommon.isOn)
                                            || (rarity.rank == 2 && toggleUncommon.isOn)
                                            || (rarity.rank == 3 && toggleRare.isOn)
                                            || (rarity.rank == 4 && toggleMythic.isOn)
                                            || (!toggleCommon.isOn && !toggleUncommon.isOn && !toggleRare.isOn && !toggleMythic.isOn);
                        
                        string search = filterSearch.ToLower();
                        bool searchCheck = string.IsNullOrWhiteSpace(search)
                                            || icard.id.Contains(search)
                                            || icard.title.ToLower().Contains(search)
                                            || icard.GetText().ToLower().Contains(search);
                        
                        if (ownedCheck && typeCheck && rarityCheck && searchCheck)
                        {
                            shownCards.Add(card);
                        }

                    }
                    
                    int index = 0;
                    foreach (CardDataQ qcard in shownCards)
                    {
                        if (index < allList.Count)
                        {
                            CollectionCard dcard = allList[index];
                            dcard.SetCard(qcard.card, qcard.variant, 0);
                            cardList.Add(dcard);
                            if (!dcard.gameObject.activeSelf)
                                dcard.gameObject.SetActive(true);
                            index++;
                        }
                    }
                    
                    for (int i = index; i < allList.Count; i++)
                        allList[i].gameObject.SetActive(false);

                    updateGrid = true;
                    updateGridTimer = 0f;
                    scrollRect.verticalNormalizedPosition = 1f;
                    RefreshCardsQuantities();
                }
            }
            
            
        }
        
        private void RefreshCardsQuantities()
        {
            UserData udata = Authenticator.Get().UserData;
            foreach (CollectionCard card in cardList)
            {
                CardData icard = card.GetCard();
                VariantData ivariant = card.GetVariant();
                bool owned = IsCardOwned(udata, icard, ivariant, 1);
                int quantity = udata.GetCardQuantity(icard, ivariant);
                card.SetQuantity(quantity);
                card.SetGrayscale(!owned);
            }
        }

        private void RefreshDeckList()
        {
            foreach (DeckLine line in deckLines)
                line.Hide();
            deckCards.Clear();
            editingDeck = false;
            saving = false;

            UserData udata = Authenticator.Get().UserData;
            if (udata == null)
                return;

            int index = 0;
            foreach (UserDeckData deck in udata.decks)
            {
                if (index < deckLines.Length)
                {
                    DeckLine line = deckLines[index];
                    line.SetLine(udata, deck);
                }
                index++;
            }

            if (index < deckLines.Length)
            {
                DeckLine line = deckLines[index];
                line.SetLine("+");
            }
            RefreshCardsQuantities();
        }

        private void RefreshDeck(UserDeckData deck)
        {
            deckTitle.text = "Deck Name";
            currentDeckTid = GameTool.GenerateRandomID(7);
            deckCards.Clear();
            saving = false;
            editingDeck = true;

            foreach (IconButton btn in heroPowers)
                btn.Deactivate();

            if (deck != null)
            {
                deckTitle.text = deck.title;
                currentDeckTid = deck.tid;

                foreach (IconButton btn in heroPowers)
                {
                    if (deck.hero != null && btn.value == deck.hero.tid)
                        btn.Activate();
                }
                
                for (int i = 0; i < deck.cards.Length; i++)
                {
                    CardData card = CardData.Get(deck.cards[i].tid);
                    VariantData variant = card.GetVariant();
                    if (card != null && variant != null)
                    {
                        AddDeckCard(card, variant, deck.cards[i].quantity);
                    }
                }
            }

            RefreshDeckCards();
        }

        private void RefreshDeckCards()
        {
            foreach (DeckLine line in deckCardLines)
                line.Hide();
            
            List<CardDataQ> list = new List<CardDataQ>();
            foreach (UserCardData card in deckCards)
            {
                CardDataQ acard = new CardDataQ();
                acard.card = CardData.Get(card.tid);
                acard.variant = VariantData.Get(card.variant);
                acard.quantity = card.quantity;
                list.Add(acard);
            }
            list.Sort((CardDataQ a, CardDataQ b) => { return a.card.title.CompareTo(b.card.title); });
            
            UserData udata = Authenticator.Get().UserData;
            int index = 0;
            int count = 0;
            foreach (CardDataQ card in list)
            {
                if (index >= deckCardLines.Count)
                    CreateDeckCard();

                if (index < deckCardLines.Count)
                {
                    DeckLine line = deckCardLines[index];
                    if (line != null)
                    {
                        line.SetLine(card.card, card.variant, card.quantity, !IsCardOwned(udata, card.card, card.variant, card.quantity));
                        count += card.quantity;
                    }
                }
                index++;
            }
            
            deckQuantity.text = count + "/" + GamePlayData.Get().deckSize;
            deckQuantity.color = count >= GamePlayData.Get().deckSize ? Color.white : Color.red;
            
            RefreshCardsQuantities();

        }

        private void RefreshStarterDeck()
        {
            UserData udata = Authenticator.Get().UserData;
            if (udata != null && (udata.cards.Length == 0 || udata.rewards.Length == 0))
            {
                if (GamePlayData.Get().starterDecks.Length > 0)
                {
                    StarterDeckPanel.Get().Show();
                }
            }
        }
        
        //-------- Deck editing actions

        private void CreateDeckCard()
        {
            GameObject deckLine = Instantiate(deckCardsPrefab, deckGrid.transform);
            DeckLine line = deckLine.GetComponent<DeckLine>();
            deckCardLines.Add(line);
            float height = deckCardLines.Count * 70f + 20f;
            deckContent.sizeDelta = new Vector2(deckContent.sizeDelta.x, height);
            line.onClick += OnClickCardLine;
            line.onClickRight += OnRightClickCardLine;
        }

        private void AddDeckCard(CardData card, VariantData variant, int quantity = 1)
        {
            AddDeckCard(card.id, variant.id, quantity);
        }
        
        private void RemoveDeckCard(CardData card, VariantData variant)
        {
            RemoveDeckCard(card.id, variant.id);
        }

        private void AddDeckCard(string tid, string variant, int quantity = 1)
        {
            UserCardData ucard = GetDeckCard(tid, variant);
            if (ucard != null)
            {
                ucard.quantity += quantity;
            }
            else
            {
                ucard = new UserCardData(tid, variant);
                ucard.quantity = quantity;
                deckCards.Add(ucard);
            }
        }

        private void RemoveDeckCard(string tid, string variant)
        {
            for (int i = deckCards.Count - 1; i >= 0; i--)
            {
                UserCardData ucard = deckCards[i];
                if (ucard.tid == tid && ucard.variant == variant)
                {
                    ucard.quantity--;

                    if(ucard.quantity <= 0)
                        deckCards.RemoveAt(i);
                }
            }
        }
        
        private UserCardData GetDeckCard(string tid, string variant)
        {
            return deckCards.FirstOrDefault(ucard => ucard.tid == tid && ucard.variant == variant);
        }

        private void SaveDeck()
        {
            UserData udata = Authenticator.Get().UserData;
            UserDeckData udeck = new UserDeckData();
            udeck.tid = currentDeckTid;
            udeck.title = deckTitle.text;
            udeck.hero = new UserCardData();
            Debug.Log("Selected hero: " + GetSelectedHeroId());
            udeck.hero.tid = GetSelectedHeroId();
            udeck.hero.variant = VariantData.GetDefault().id;
            udeck.cards = deckCards.ToArray();
            saving = true;
            
            if (Authenticator.Get().IsTest())
                SaveDeckTest(udata, udeck);

            if (Authenticator.Get().IsApi())
                SaveDeckAPI(udata, udeck);

            ShowDeckList();
        }
        
        private async void SaveDeckTest(UserData udata, UserDeckData udeck)
        {
            udata.SetDeck(udeck);
            await Authenticator.Get().SaveUserData();
            ReloadUserDecks();
        }
        
        private async void SaveDeckAPI(UserData udata, UserDeckData udeck)
        {
            string url = ApiClient.ServerURL + "/users/deck/" + udeck.tid;
            string jdata = ApiTool.ToJson(udeck);
            WebResponse res = await ApiClient.Get().SendPostRequest(url, jdata);
            UserDeckData[] decks = ApiTool.JsonToArray<UserDeckData>(res.data);
            saving = res.success;

            if (res.success && decks != null)
            {
                udata.decks = decks;
                await Authenticator.Get().SaveUserData();
                ReloadUserDecks();
            }
        }
        
        private async void DeleteDeck(string deckTid)
        {
            UserData udata = Authenticator.Get().UserData;
            UserDeckData udeck = udata.GetDeck(deckTid);
            List<UserDeckData> decks = new List<UserDeckData>(udata.decks);
            decks.Remove(udeck);
            udata.decks = decks.ToArray();

            if (Authenticator.Get().IsApi())
            {
                string url = ApiClient.ServerURL + "/users/deck/" + deckTid;
                await ApiClient.Get().SendRequest(url, "DELETE", "");
            }

            await Authenticator.Get().SaveUserData();
            ReloadUserDecks();
        }
        
        //---- Left Panel Filters Clicks -----------
        public void OnClickTeam(IconButton button)
        {
            filterTeam = null;
            if (button.IsActive())
            {
                foreach (TeamData team in TeamData.GetAll())
                {
                    if (button.value == team.id)
                        filterTeam = team;
                }
            }
            Debug.Log("Team: " + filterTeam);
            RefreshCards();
        }
        
        public void OnChangeToggle()
        {
            RefreshCards();
        }
        
        public void OnChangeDropdown()
        {
            filterDropdown = sortDropdown.value;
            RefreshCards();
        }
        
        public void OnChangeSearch()
        {
            filterSearch = search.text;
            RefreshCards();
        }
        
        //---- Card grid clicks ----------
        public void OnClickCard(CardUI card)
        {
            if (!editingDeck)
            {
                CardZoomPanel.Get().ShowCard(card.GetCard(), card.GetVariant());
                return;
            }
            
            CardData icard = card.GetCard();
            VariantData variant = card.GetVariant();
            if (icard != null)
            {
                int inDeck = CountDeckCards(icard, variant);
                int inDeckSame = CountDeckCards(icard);
                UserData udata = Authenticator.Get().UserData;

                bool owner = IsCardOwned(udata, card.GetCard(), card.GetVariant(), inDeck + 1);
                bool deckLimit = inDeckSame < GamePlayData.Get().deckDuplicateNax;

                if (owner && deckLimit)
                {
                    AddDeckCard(icard, variant);
                    RefreshDeckCards();
                }
            }
            
        }
        
        public void OnClickCardRight(CardUI card)
        {
            CardZoomPanel.Get().ShowCard(card.GetCard(), card.GetVariant());
        }
        
        //---- Right Panel Click -------
        public void OnClickDeckLine(DeckLine line)
        {
            if (line.IsHidden() || saving)
                return;
            UserDeckData deck = line.GetUserDeck();
            RefreshDeck(deck);
            ShowDeckCards();
        }
        
        private void OnClickCardLine(DeckLine line)
        {
            CardData card = line.GetCard();
            VariantData variant = line.GetVariant();
            if (card != null)
            {
                RemoveDeckCard(card, variant);
            }

            RefreshDeckCards();
        }
        
        private void OnRightClickCardLine(DeckLine line)
        {
            CardData icard = line.GetCard();
            if (icard != null)
                CardZoomPanel.Get().ShowCard(icard, line.GetVariant());
        }
        
        // ---- Deck editing Click -----
        public void OnClickSaveDeck()
        {
            if (!saving)
            {
                SaveDeck();
            }
        }
        
        public void OnClickDeckBack()
        {
            ShowDeckList();
        }

        public void OnClickDeleteDeck()
        {
            if (editingDeck && !string.IsNullOrEmpty(currentDeckTid))
            {
                DeleteDeck(currentDeckTid);
            }
        }

        public void OnClickDeckDelete(DeckLine line)
        {
            if (line.IsHidden())
                return;
            UserDeckData deck = line.GetUserDeck();
            if (deck != null)
            {
                DeleteDeck(deck.tid);
            }
        }
        
        // ---- Getters -----
        public int CountDeckCards(CardData card, VariantData cvariant)
        {
            int count = 0;
            foreach (UserCardData ucard in deckCards)
            {
                if (ucard.tid == card.id && ucard.variant == cvariant.id)
                    count += ucard.quantity;
            }
            return count;
        }
        
        public int CountDeckCards(CardData card)
        {
            int count = 0;
            foreach (UserCardData ucard in deckCards)
            {
                if (ucard.tid == card.id)
                    count += ucard.quantity;
            }
            return count;
        }
        
        private bool IsCardOwned(UserData udata, CardData card, VariantData variant, int quantity)
        {
            return udata.GetCardQuantity(card, variant) >= quantity;
        }

        private string GetSelectedHeroId()
        {
            foreach (IconButton btn in heroPowers)
            {
                if (btn.IsActive())
                    return btn.value;
            }
            return "";
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshAll();
            ShowDeckList();
        }

        public static CollectionPanel Get()
        {
            return instance;
        }
        
    }
    
    public struct CardDataQ
    {
        public CardData card;
        public VariantData variant;
        public int quantity;
    }
}