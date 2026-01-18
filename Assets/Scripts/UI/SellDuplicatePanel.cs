using Api;
using Menu;
using TMPro;
using Unit;

namespace UI
{
    public class SellDuplicatePanel: UIPanel
    {
        public TextMeshProUGUI error;
        
        private static SellDuplicatePanel instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        public async void OnClickSell()
        {
            SellDuplicateRequest req = new SellDuplicateRequest();
            req.keep = 3;
            
            string url = ApiClient.ServerURL + "/users/cards/sell/duplicate";
            string jdata = ApiTool.ToJson(req);
            error.text = "";
            
            WebResponse res = await ApiClient.Get().SendPostRequest(url, jdata);
            if (res.success)
            {
                CollectionPanel.Get().ReloadUser();
                Hide();
            }
            else
            {
                error.text = res.error;
            }
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            error.text = "";
        }
        
        public static SellDuplicatePanel Get()
        {
            return instance;
        }
        
    }
}