using GameClient;
using Unit;

namespace UI
{
    /// <summary>
    /// Panel that appears when internet connection is lost
    /// </summary>

    public class ConnectionPanel:UIPanel
    {
        private static ConnectionPanel instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }
        
        public void OnClickQuit()
        {
            Gameclient.Get()?.Disconnect();
            SceneNav.GoTo("LoginMenu");
        }
        
        public static ConnectionPanel Get()
        {
            return instance;
        }
    }
}