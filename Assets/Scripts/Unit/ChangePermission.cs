using System;
using System.Threading.Tasks;
using Api;
using Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unit
{
    /// <summary>
    /// Use this tool to upload your cards and packs to the Mongo Database (it will overwrite existing data)
    /// </summary>
    public class ChangePermission:MonoBehaviour
    {
        public string username = "admin";
        
        [Header("Login")]
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        
        [Header("Change Permission")]
        public UIPanel permissionPanel;
        public TMP_InputField targetUserTxt;
        public TMP_InputField targetPermTxt;
        public TextMeshProUGUI error;

        private string loggedUser;

        private void Start()
        {
            usernameInput.text = username;
            error.text = "";
            
        }

        private async void Login(string user, string pass)
        {
            LoginResponse response = await ApiClient.Get().Login(user, pass);

            if (response.success&& response.permissionLevel >= 10)
            {
                loggedUser = response.username;
                permissionPanel.Show();
            }
            else if (response.success)
            {
                error.text = "Not an admin user";
            }
            else
            {
                error.text = response.error;
            }
            
        }
        
        private async Task<string> GetUserID(string tuser)
        {
            string url = ApiClient.ServerURL + "/users/" + tuser;
            WebResponse res = await ApiClient.Get().SendGetRequest(url);
            UserData udata = ApiTool.JsonToObject<UserData>(res.data);
            if (!res.success)
                error.text = res.error;

            return res.success ? udata.id : null;
        }
        
        private async void SetPermission(string tuser, int permission)
        {
            string userID = await GetUserID(tuser);
            if (userID == null)
                return;

            ChangePermissionRequest req = new ChangePermissionRequest();
            req.permissionLevel = permission;

            string url = ApiClient.ServerURL + "/users/permission/edit/" + userID;
            string json = ApiTool.ToJson(req);
            WebResponse res = await ApiClient.Get().SendPostRequest(url, json);

            if (!res.success)
                error.text = res.error;

            if (res.success)
            {
                error.text = "Success!";
                error.color = Color.green;
            }
        }
        
        public void OnClickLogin()
        {
            if (string.IsNullOrEmpty(usernameInput.text))
                return;

            if (string.IsNullOrEmpty(passwordInput.text))
                return;

            error.text = "";
            error.color = Color.red;
            Login(usernameInput.text, passwordInput.text);
        }
        
        public void OnClickUpdate()
        {
            if (string.IsNullOrEmpty(targetUserTxt.text))
                return;

            bool success = int.TryParse(targetPermTxt.text, out int perm);
            if (!success)
                return;

            if (loggedUser == targetPermTxt.text)
                return; //Prevent changing yourself

            error.text = "";
            error.color = Color.red;
            SetPermission(targetUserTxt.text, perm);
        }
    }
    
    [Serializable]
    public class ChangePermissionRequest
    {
        public int permissionLevel;
    }
}