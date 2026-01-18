using Api;
using Data;
using Network;
using TMPro;
using UI;
using Unit;
using UnityEngine;

namespace Menu
{
    /// <summary>
    /// When you click on a pack in the PackPanel, a box will appear to show more information about it
    /// You can also buy pack in this panel
    /// </summary>
    public class PackZoomPanel:UIPanel
    {
        public PackUI packUI;
        public TextMeshProUGUI desc;
        
        public GameObject buyArea;
        public TMP_InputField buyQuantity;
        public TextMeshProUGUI buyCost;
        public TextMeshProUGUI buyError;
        
        private PackData pack;

        private static PackZoomPanel instance;

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

            if (pack != null)
            {
                int quantity = GetBuyQuantity();
                buyCost.text = (pack.cost * quantity).ToString();
            }
        }

        public void ShowPack(PackData pack)
        {
            this.pack = pack;

            UserData udata = Authenticator.Get().UserData;
            int quantity = udata.GetPackQuantity(pack.id);
            packUI.SetPack(pack, quantity);
            desc.text = pack.GetDesc();
            buyQuantity.text = "1";
            buyError.text = "";
            buyArea?.SetActive(pack.available);

            Show();
        }
        
        private async void BuyPackTest()
        {
            int quantity = GetBuyQuantity();
            int cost = (quantity * pack.cost);
            if (quantity <= 0)
                return;

            UserData udata = Authenticator.Get().UserData;
            if (udata.coins < cost)
                return;

            udata.AddPack(pack.id, quantity);
            udata.coins -= cost;
            await Authenticator.Get().SaveUserData();
            PackPanel.Get().ReloadUserPack();
            Hide();
        }

        private async void BuyPackApi()
        {
            BuyPackRequest req = new BuyPackRequest();
            req.pack = pack.id;
            req.quantity = GetBuyQuantity();
            
            
            if (req.quantity <= 0)
                return;

            string url = ApiClient.ServerURL + "/users/packs/buy/";
            string jdata = ApiTool.ToJson(req);
            buyError.text = "";

            WebResponse res = await ApiClient.Get().SendPostRequest(url, jdata);
            if (res.success)
            {
                PackPanel.Get().ReloadUserPack();
                Hide();
            }
            else
            {
                buyError.text = res.error;
            }
        }
        
        public void OnClickBuy()
        {
            if (Authenticator.Get().IsTest())
            {
                BuyPackTest();
            }
            if (Authenticator.Get().IsApi())
            {
                BuyPackApi();
            }
        }
        
        private void OnClickTab(UI.TabButton btn)
        {
            if (btn.group == "menu")
                Hide();
        }
        
        public int GetBuyQuantity()
        {
            bool success = int.TryParse(buyQuantity.text, out int quantity);
            if (success)
                return quantity;
            return 0;
        }
        
        public PackData GetPack()
        {
            return pack;
        }

        public static PackZoomPanel Get()
        {
            return instance;
        }

        
    }
}