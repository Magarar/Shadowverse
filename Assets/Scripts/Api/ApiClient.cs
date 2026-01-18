using System.Threading.Tasks;
using Data;
using GameLogic;
using Network;
using Unit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Api
{
    ///<总结>
    ///API客户端与NodeJS web API通信
    ///可以发送请求并接收响应
    ///</摘要>
    public class ApiClient : MonoBehaviour
    {
        private static ApiClient instance;

        public bool isServer;
        
        public UnityAction<RegisterResponse> onRegister; //Triggered after register, even if failed
        public UnityAction<LoginResponse> onLogin; //Triggered after login, even if failed
        public UnityAction<LoginResponse> onRefresh; //Triggered after login refresh, even if failed 
        public UnityAction onLogout; //Triggered after logout

        private string userId = "";
        private string username = "";
        private string accessToken = "";
        private string refreshToken = "";
        private string apiVersion = "";

        private bool loggedIn = false;
        private bool expired = false;
        
        private UserData userData = null;

        private int sending = 0;
        private string lastError = "";
        private float refreshTimer = 0f;
        private float onlineTimer = 0f;
        private long expirationTimestamp = 0;
        
        private const float OnlineDuration = 60f * 5f; //5 min
        
        public void Awake()
        {
            //API客户端应在OnDestroyOnLoad上
            //如果已经分配，请不要在此处分配，因为新的分配将在TheNetwork Awake中被销毁
            if(instance== null)
                instance = this;
            LoadTokens();
        }

        private void Update()
        {
            //刷新访问令牌或联机状态
            Refresh();
        }

        private void LoadTokens()
        {
            if (!isServer && string.IsNullOrEmpty(userId))
            {
                accessToken = PlayerPrefs.GetString("tcg_access_token");
                refreshToken = PlayerPrefs.GetString("tcg_refresh_token");
            }
        }

        private void SaveTokens()
        {
            if (!isServer)
            {
                PlayerPrefs.SetString("tcg_access_token", accessToken);
                PlayerPrefs.SetString("tcg_refresh_token", refreshToken);
            }
        }
        
        private async void Refresh()
        {
            if(!loggedIn)
                return;
            //Check expiration
            if (!expired)
            {
                long current = GetTimestamp();
                expired = current > (expirationTimestamp - 10);
            }
            
            //Refresh access token when expired
            refreshTimer += Time.deltaTime;
            if (expired && refreshTimer > 5f)
            {
                refreshTimer = 0f;
                await RefreshLogin();
            }
            
            //Refresh online status
            onlineTimer += Time.deltaTime;
            if (!expired && onlineTimer > OnlineDuration)
            {
                onlineTimer = 0f;
                await KeepOnline();
            }
        }
        
        public async Task<RegisterResponse> Register(string email, string user, string password)
        {
            RegisterRequest data = new RegisterRequest()
            {
                email = email,
                username = user,
                password = password,
                avatar = ""
            };
            return await Register(data);
        }

        private async Task<RegisterResponse> Register(RegisterRequest data)
        {
            Logout();
            
            string url = ServerURL + "/users/register";
            string json = ApiTool.ToJson(data);
            
            WebResponse res = await SendPostRequest(url, json);
            RegisterResponse registRes = ApiTool.JsonToObject<RegisterResponse>(res.data);
            registRes.success = res.success;
            registRes.error = res.error;
            onRegister?.Invoke(registRes);
            
            
            return registRes;
        }
        
        public async Task<LoginResponse> Login(string user, string password)
        {
            Logout(); //Disconnect

            LoginRequest data = new LoginRequest();
            data.password = password;
            if (user.Contains("@"))
                data.email = user;
            else
                data.username = user;
            
            string url = ServerURL + "/auth";
            string json = ApiTool.ToJson(data);
            WebResponse res = await SendPostRequest(url, json);
            LoginResponse loginRes = GetLoginRes(res);
            AfterLogin(loginRes);
            
            return loginRes;
        }
        
        public async Task<LoginResponse> RefreshLogin()
        {
            string url = ServerURL + "/auth/refresh";
            AutoLoginRequest data = new AutoLoginRequest();
            data.refresh_token = refreshToken;
            string json = ApiTool.ToJson(data);

            WebResponse res = await SendPostRequest(url, json);
            LoginResponse loginRes = GetLoginRes(res);
            AfterLogin(loginRes);

            onRefresh?.Invoke(loginRes);
            return loginRes;
        }

        private LoginResponse GetLoginRes(WebResponse res)
        {
            LoginResponse loginRes = ApiTool.JsonToObject<LoginResponse>(res.data);
            loginRes.success = res.success;
            loginRes.error = res.error;
            
            //不承诺强制使用与api相同的客户端版本
            /*if (!is_server && !IsVersionValid())
            {
                login_res.error = "Invalid Version";
                login_res.success = false;
            }*/
            return loginRes;
        }
        
        private void AfterLogin(LoginResponse loginRes)
        {
            lastError = loginRes.error;
            if (loginRes.success)
            {
                userId = loginRes.id;
                username = loginRes.username;
                accessToken = loginRes.accessToken;
                refreshToken = loginRes.refreshToken;
                apiVersion = loginRes.version;
                expirationTimestamp = GetTimestamp() + loginRes.duration;
                refreshTimer = 0f;
                expired = false;
                onlineTimer = 0f;
                loggedIn = true;
                SaveTokens();
            }
        }
        
        public async Task<UserData> LoadUserData()
        {
            userData = await LoadUserData(username);
            return userData;
        }
        
        public async Task<UserData> LoadUserData(string username)
        {
            if (!IsConnected())
                return null;

            string url = ServerURL + "/users/" + username;
            WebResponse res = await SendGetRequest(url);

            UserData udata = null;
            if (res.success)
            {
                udata = ApiTool.JsonToObject<UserData>(res.data);
            }

            return udata;
        }
        
        public async Task<bool> KeepOnline()
        {
            if (!IsConnected())
                return false;

            //Keep player online
            string url = ServerURL + "/auth/keep";
            WebResponse res = await SendGetRequest(url);
            expired = !res.success;
            return res.success;
        }
        
        public async Task<bool> Validate()
        {
            if (!IsConnected())
                return false;

            //Check if connection is still valid
            string url = ServerURL + "/auth/validate";
            WebResponse res = await SendGetRequest(url);
            expired = !res.success;
            return res.success;
        }
        
        public void Logout()
        {
            userId = "";
            username = "";
            accessToken = "";
            refreshToken = "";
            apiVersion = "";
            lastError = "";
            loggedIn = false;
            onLogout?.Invoke();
            SaveTokens();
        }
        
        public async void CreateMatch(Game gameData)
        {
            if (gameData.settings.gameType != GameType.Multiplayer)
                return;
        
            AddMatchRequest req = new AddMatchRequest();
            req.players = new string[2];
            req.players[0] = gameData.players[0].username;
            req.players[1] = gameData.players[1].username;
            req.tid = gameData.gameUID;
            req.ranked = gameData.settings.IsRanked();
            req.mode = gameData.settings.GetGameModeId();
        
            string url = ServerURL + "/matches/add";
            string json = ApiTool.ToJson(req);
            WebResponse res = await SendPostRequest(url, json);
            Debug.Log("Match Started! " + res.success);
        }
        
        public async void EndMatch(Game gameData, int winnerID)
        {
            if (gameData.settings.gameType != GameType.Multiplayer)
                return;
        
            Player player = gameData.GetPlayer(winnerID);
            CompleteMatchRequest req = new CompleteMatchRequest();
            req.tid = gameData.gameUID;
            req.winner = player != null ? player.username : "";
        
            string url = ServerURL + "/matches/complete";
            string json = ApiTool.ToJson(req);
            WebResponse res = await SendPostRequest(url, json);
            Debug.Log("Match Completed! " + res.success);
        }
        
        public async Task<string> SendGetVersion()
        {
            string url = ServerURL + "/version";
            WebResponse res = await SendGetRequest(url);

            if (res.success)
            {
                VersionResponse versionData = ApiTool.JsonToObject<VersionResponse>(res.data);
                apiVersion = versionData.version;
                return apiVersion;
            }

            return null;
        }

        public async Task<WebResponse> SendGetRequest(string url)
        {
            return await SendRequest(url, WebRequest.MethodGet);
        }
        
        public async Task<WebResponse> SendPostRequest(string url, string json)
        {
            return await SendRequest(url, WebRequest.MethodPost, json);
        }

        public async Task<WebResponse> SendRequest(string url, string method,string json = null)
        {
            UnityWebRequest request = WebRequest.Create(url, method, json, accessToken);
            return await SendRequest(request);
        }

        public async Task<WebResponse> SendRequest(UnityWebRequest request)
        {
            int wait = 0;
            int waitMax = request.timeout * 1000;
            request.timeout += 1; //Add offset to make sure it aborts first
            sending++;

            var asyncOper = request.SendWebRequest();
            while (!asyncOper.isDone)
            {
                await TimeTool.Delay(200);
                wait += 200;
                if (wait >= waitMax)
                    request.Abort(); //Abort to avoid unity errors on timeout
            }

            WebResponse response = WebRequest.GetResponse(request);
            response.error = GetError(response);
            lastError = response.error;
            request.Dispose();
            sending--;

            return response;
        }

        private string GetError(WebResponse res)
        {
            if (res.success)
                return "";

            ErrorResponse err = ApiTool.JsonToObject<ErrorResponse>(res.data);
            if (err != null)
                return err.error;
            else
                return res.error;
        }
        
        

        public bool IsConnected()
        {
            return loggedIn && !expired;
        }

        public bool IsExpired()
        {
            return expired;
        }

        public bool IsLoggedIn()
        {
            return loggedIn;
        }
        
        public bool IsBusy()
        {
            return sending > 0;
        }
        
        private long GetTimestamp()
        {
            return System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public string GetLastError()
        {
            return lastError;
        }
        
        // public string GetLastRequest()
        // {
        //     return G;
        // }
        
        //如果你想阻止玩家使用过时的客户端登录，请使用此功能
        //在设置api_version后，在login和loginrefresh函数中调用它，如果无效，则返回错误
        public bool IsVersionValid()
        {
            return ClientVersion == ServerVersion; 
        }
        
        public UserData UserData => userData;

        public string UserID { 
            get => userId;
            set => userId = value;
        }
        public string Username { 
            get => username;
            set => username = value;
        }
        public string AccessToken { 
            get => accessToken;
            set => accessToken = value;
        }
        public string RefreshToken { 
            get => refreshToken;
            set => refreshToken = value;
        }
        
        public string ServerVersion => apiVersion;
        public string ClientVersion => Application.version;

        public static string ServerURL
        {
            get
            {
                NetworkData data = NetworkData.Get();
                string protocol = data.apiHttps ? "https://" : "http://";
                return protocol + data.apiURL;
            }
        }
        
        public static ApiClient Get()
        {
            if (instance == null)
                instance = FindObjectOfType<ApiClient>();
            return instance;
        }

    }
}
