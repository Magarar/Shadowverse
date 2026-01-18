using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Network
{
    /// <summary>
    /// 所有网络相关内容的主配置文件
    /// 服务器API密码不在此文件中（而是在服务器场景中），以防止将其暴露给客户端生成
    /// </summary>
    [CreateAssetMenu(fileName = "NetworkData", menuName = "Shadowverse/NetworkData")]
    public class NetworkData : ScriptableObject
    {
        [Header("Game Server")] 
        public string url; //Url of your Game Server
        public ushort port;//Port to connect/listen on your game server
        
        [Header("Api")]
        public string apiURL;                  //Url of your Nodejs API (can be same as Game Server)
        public bool apiHttps;                  //Http or Https ?   Http will use port 80,  https will use port 443
        
        [Header("Setting")]
        public SoloType soloType;
        public AuthenticatorType authenticatorType;

        public static NetworkData Get()
        {
            return TcgNetwork.Get().data;
        }
        
        
    }

    public enum SoloType
    {
        UseNetcode = 0,     //在单人游戏中使用Netcode网络消息，在多人游戏和单人游戏中都有更相似的行为，建议在多人/单人游戏之间保持一致
        Offline = 10        //使单人游戏完全离线（无网络代码），但可能与WebGL所需的多人游戏不同，因为StartHost在WebGL上不起作用。
    }
}
