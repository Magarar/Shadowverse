using System.Threading.Tasks;
using UnityEngine;
using Data;
using Unit;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace Network
{
    ///<总结>
    ///测试身份验证器只生成一个随机ID用作用户ID
    ///这对于在多人游戏中测试游戏非常有用，无需每次登录
    ///Unity Services功能在测试模式下无法工作（中继、云保存…）
    ///使用匿名模式测试这些功能（在服务窗口中连接项目ID后）
    ///</摘要>
    public class AuthenticatorLocal: Authenticator
    {
        private UserData userData;

        public override async Task<bool> Login(string username)
        {
            userId = username;//用户用户名作为ID，用于测试时保存文件的一致性
            this.username = username;
            loggedIn = true;
            await Task.Yield();
            PlayerPrefs.SetString("tcg_user", username);//保存最后用户名
            return true;
        }
        
        public override async Task<bool> RefreshLogin()
        {
            string username = PlayerPrefs.GetString("tcg_user", "");
            Debug.Log("Load user: " + username);
            if (!string.IsNullOrEmpty(username))
            {
                bool success = await Login(username);
                return success;
            }
            return false;
        }

        public override async Task<UserData> LoadUserData()
        {
            string user = PlayerPrefs.GetString("tcg_user", "");
            string file = SaveTool.CombineFilename(username, "user");
            if (!string.IsNullOrEmpty(file) && SaveTool.DoesFileExist(file))
            {
                userData = SaveTool.LoadFile<UserData>(file);
            }

            if (userData == null)
            {
                userData = new UserData();
                userData.username = username;
                userData.id = username;
            }
            
            await Task.Yield();
            return userData;
        }

        public override async Task<bool> SaveUserData()
        {
            if (userData != null && SaveTool.IsValidFilename(userData.username))
            {
                string file = SaveTool.CombineFilename(username, "user");
                SaveTool.SaveFile<UserData>(file, userData);
                await Task.Yield();
                return true;
            }
            return false;
        }

        public override void Logout()
        {
            base.Logout();
            userData = null;
            PlayerPrefs.DeleteKey("tcg_user");
        }
        
        public override UserData GetUserData() => userData;
    }
}