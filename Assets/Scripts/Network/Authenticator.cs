using System.Threading.Tasks;
using Data;

namespace Network
{
    /// <summary>
    /// 必须继承所有身份验证器的基类
    /// </summary>
    public abstract class Authenticator
    {
        protected string userId = null;
        protected string username = null;
        protected bool loggedIn = false;
        protected bool inited = false;
        
        public virtual async Task Initialize()
        {
            inited = true;
            await Task.Yield(); //Do nothing
        }
        
        public virtual async Task<bool> Login(string username)
        {
            await Task.Yield(); //Do nothing
            return false;
        }
        
        public virtual async Task<bool> Login(string username, string token)
        {
            return await Login(username); //某些身份验证器未定义此函数
        }

        public virtual async Task<bool> RefreshLogin()
        {
            return await Login(username); //Same as Login if not defined
        }
        
        //只需为测试分配自己的值，即可绕过登录系统
        public virtual void LoginTest(string username)
        {
            this.userId = username;
            this.username = username;
            loggedIn = true;
        }
        
        public virtual async Task<bool> Register(string username, string email, string token)
        {
            return await Login(username, token); //某些身份验证器未定义此函数
        }
        
        public virtual async Task<UserData> LoadUserData()
        {
            await Task.Yield(); //Do nothing
            return null;
        }

        public virtual async Task<bool> SaveUserData()
        {
            await Task.Yield(); //Do nothing
            return false;
        }
        
        public virtual void Logout()
        {
            loggedIn = false;
            userId = null;
            username = null;
        }
        
        public virtual bool IsInited()
        {
            return inited;
        }
        
        public virtual bool IsConnected()
        {
            return IsSignedIn() && !IsExpired();
        }
        
        public virtual bool IsSignedIn()
        {
            return loggedIn; //如果登录过期，IsSignedIn仍将为true
        }

        public virtual bool IsExpired()
        {
            return false;
        }

        public virtual string GetUserId()
        {
            return userId;
        }
        
        public virtual string GetUsername()
        {
            return username;
        }
        
        public virtual int GetPermission()
        {
            return loggedIn ? 1 : 0;
        }
        
        public virtual UserData GetUserData()
        {
            return null;
        }
        
        public virtual string GetError()
        {
            return ""; //Should return the latest error
        }
        
        public bool IsTest()
        {
            return NetworkData.Get().authenticatorType == AuthenticatorType.LocalSave;
        }
        
        public bool IsApi()
        {
            return NetworkData.Get().authenticatorType == AuthenticatorType.Api;
        }
        
        public string UserID => GetUserId();
        public string Username => GetUsername();
        public UserData UserData => GetUserData();
        
        public static Authenticator Create(AuthenticatorType type)
        {
            if (type == AuthenticatorType.Api)
                return new AuthenticatorApi();
            else
                return new AuthenticatorLocal();
        }
        
        public static Authenticator Get()
        {
            return TcgNetwork.Get().Auth; //Access authenticator
        }
        
        
    }
    
    public enum AuthenticatorType
    {
        LocalSave = 0,   //测试模式，虚假登录，无需每次登录即可快速测试
        Api = 10,        //实际在线登录
    }
}
