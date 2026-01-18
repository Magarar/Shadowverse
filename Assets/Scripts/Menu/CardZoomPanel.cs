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
    /// <summary>
    /// When clicking on a card in menu, a box will appear with additional game info
    /// You can also buy cards in this panel
    /// </summary>
    public class CardZoomPanel:UIPanel
    {
        public CardUI cardUI;
        public TextMeshProUGUI desc;
        public Image quantityBar;
        public TextMeshProUGUI quantityText;

        public GameObject tradeArea;
        public TMP_InputField tradeQuantity;
        public TextMeshProUGUI buyCost;
        public TextMeshProUGUI sellCost;
        public TextMeshProUGUI tradeError;

        private CardData card;
        private VariantData variant;
        
        private static CardZoomPanel instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;

            UI.TabButton.onClickAny += OnClickTab;
        }
        
        private void OnDestroy()
        {
            UI.TabButton.onClickAny -= OnClickTab;
        }

        protected override void Update()
        {
            base.Update();
            if (card != null)
            {
                int quantity = GetBuyQuantity();
                int cost = quantity * card.cost * variant.costFactor;
                buyCost.text = cost.ToString();
                sellCost.text = Mathf.RoundToInt(cost * GamePlayData.Get().sellRatio).ToString();
            }
        }

        public void ShowCard(CardData card, VariantData variant)
        {
            this.card = card;
            this.variant = variant;
            
            UserData udata = Authenticator.Get().GetUserData();
            int quantity = udata.GetCardQuantity(card, variant);
            quantityText.text = quantity.ToString();
            quantityText.enabled = quantity > 0;
            quantityBar.enabled = quantity > 0;
            tradeQuantity.text = "1";
            tradeError.text = "";
            tradeArea.SetActive(card.deckbuilding&&card.cost>0);
            
            cardUI.SetCard(card, variant);
            string desc = card.GetDesc();
            string adesc = card.GetAbilitiesDesc();
            if(!string.IsNullOrWhiteSpace(desc))
                this.desc.text = desc + "\n\n" + adesc;
            else
                this.desc.text = adesc;
            
            Show();

        }
        
        public void RefreshCard()
        {
            ShowCard(card, variant);
        }

        private async void BuyCardTest()
        {
            int quantity = GetBuyQuantity();
            int cost = (quantity * card.cost * variant.costFactor);
            if (quantity <= 0)
                return;
            UserData udata = Authenticator.Get().UserData;
            if (udata.coins < cost)
                return;
            udata.AddCard(card.id, variant.id, quantity);
            udata.coins -= cost;
            await Authenticator.Get().SaveUserData();
            CollectionPanel.Get().ReloadUser();
            Hide();
        }

        private async void BuyCardApi()
        {
            BuyCardRequest req = new BuyCardRequest();
            req.card = card.id;
            req.variant = variant.id;
            req.quantity = GetBuyQuantity();
            
            if (req.quantity <= 0)
                return;
            
            string url = ApiClient.ServerURL + "/users/cards/buy/";
            string jdata = ApiTool.ToJson(req);
            tradeError.text = "";
            WebResponse res = await ApiClient.Get().SendPostRequest(url, jdata);
            if (res.success)
            {
                CollectionPanel.Get().ReloadUser();
                Hide();
            }
            else
            {
                tradeError.text = res.error;
            }
        }

        private async void SellCardTest()
        {
            int quantity = GetBuyQuantity();
            int cost = (quantity * card.cost * variant.costFactor);
            if (quantity <= 0)
                return;
            
            UserData udata = Authenticator.Get().UserData;
            udata.AddCard(card.id, variant.id, -quantity);
            udata.coins += cost;
            await Authenticator.Get().SaveUserData();
            CollectionPanel.Get().ReloadUser();
            MainMenu.Get().RefreshDeckList();
            Hide();
        }

        private async void SellCardApi()
        {
            BuyCardRequest req = new BuyCardRequest();
            req.card = card.id;
            req.variant = variant.id;
            req.quantity = GetBuyQuantity();
            if (req.quantity <= 0)
                return;
            
            string url = ApiClient.ServerURL + "/users/cards/sell/";
            string jdata = ApiTool.ToJson(req);
            tradeError.text = "";
            WebResponse res = await ApiClient.Get().SendPostRequest(url, jdata);
            if (res.success)
            {
                CollectionPanel.Get().ReloadUser();
                Hide();
            }
            else
            {
                tradeError.text = res.error;
            }
        }

        public void OnClickBuy()
        {
            if (Authenticator.Get().IsTest())
            {
                BuyCardTest();
            }
            if (Authenticator.Get().IsApi())
            {
                BuyCardApi();
            }
        }

        public void OnClickSell()
        {
            if (Authenticator.Get().IsTest())
            {
                SellCardTest();
            }

            if (Authenticator.Get().IsApi())
            {
                SellCardApi();
            }
        }
        
        public void OnClickTab(UI.TabButton btn)
        {
            if (btn.group == "menu")
                Hide();
        }

        private int GetBuyQuantity()
        {
            bool success = int.TryParse(tradeQuantity.text, out int quantity);
            if (success)
                return quantity;
            return 0;
        }
        
        public CardData GetCard()
        {
            return card;
        }

        public string GetCardId()
        {
            return card.id;
        }

        public string GetCardVariant()
        {
            return variant.id;
        }

        public static CardZoomPanel Get()
        {
            return instance;
        }

        
    }
}