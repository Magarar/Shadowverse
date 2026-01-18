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
    /// Player panel appears when you click on your avatar in the menu
    /// it shows all stats related to your account, and let you change avatar/cardback
    /// </summary>
    public class PlayerPanel: UIPanel
    {
        [Header("Player")]
        public TextMeshProUGUI playerName;
        public TextMeshProUGUI playerLevel;
        public AvatarUI avatar;
        public CardBackUI cardBack;
        public TextMeshProUGUI elo;
        public TextMeshProUGUI winRate;
        public TextMeshProUGUI victories;
        
        [Header("Bottom bar")]
        public GameObject buttonsArea;

        
        // [Header("Edit Panel")]
        // public UIPanel editPanel;
        // public TMP_InputField userEmail;
        // public TMP_InputField userPasswordPrev;
        // public TMP_InputField userPasswordNew;
        // public TMP_InputField userPasswordConfirm;
        // public Button editChangeEmail;
        // public Button editChangePassword;
        // public Button resendButton;
        // public Button confirmButton;
        // public TextMeshProUGUI editError;
        //
        private string username;
        private UserData userData;

        private static PlayerPanel instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;


        }
        
        protected override void Update()
        {
            base.Update();

        }

        protected override void Start()
        {
            base.Start();
        }
        
        private async void LoadData()
        {
            if (IsYou())
                userData = Authenticator.Get().UserData;
            else
                userData = await ApiClient.Get().LoadUserData(username);

            RefreshPanel();
        }
        
        private void ClearPanel()
        {
            playerName.text = "";
            elo.text = "";
            winRate.text = "";
            playerLevel.text = "";
            avatar.Hide();
            cardBack.Hide();
        }

        private void RefreshPanel()
        {
            // avatarPanel.Hide();
            //cardback_panel.Hide();

            if (userData != null)
            {
                UserData user = userData;
                playerName.text = user.username;
                playerLevel.text = GamePlayData.Get().GetPlayerLevel(user.xp).ToString();

                AvatarData avatar = AvatarData.Get(user.avatar);
                this.avatar.SetAvatar(avatar);

                CardBackData cb = CardBackData.Get(user.cardback);
                this.cardBack.SetCardBack(cb);

                int winrateVal = user.matches > 0 ? Mathf.RoundToInt(user.victories * 100f / user.matches) : 0;
                winRate.text = winrateVal + "%";
                elo.text = user.elo.ToString();
                victories.text = user.victories.ToString();

                buttonsArea?.SetActive(IsYou());    //Buttons like logout only active if your account
            }
        }

        private void RefreshAvatarList()
        {
            // foreach (AvatarUI icon in avatars)
            //     icon.SetDefaultAvatar();
            //
            // int index = 0;
            // foreach (AvatarData adata in AvatarData.GetAll())
            // {
            //     if (index < avatars.Length)
            //     {
            //         AvatarUI line = avatars[index];
            //         if (adata != null)
            //         {
            //             line.SetAvatar(adata);
            //             index++;
            //         }
            //     }
            // }
        }
        
        private void RefreshCardBackList()
        {
            // foreach (CardBackUI line in cardBacks)
            //     line.Hide();
            //
            // int index = 0;
            // foreach (CardBackData cbdata in CardBackData.GetAll())
            // {
            //     if (index < cardBacks.Length)
            //     {
            //         CardBackUI line = cardBacks[index];
            //         if (cbdata != null)
            //         {
            //             line.SetCardBack(cbdata);
            //             index++;
            //         }
            //     }
            // }
        }
        
        private void OnClickAvatar(AvatarData avatar)
        {
            userData = Authenticator.Get().UserData;
            if (avatar != null && userData != null && IsYou())
            {
                userData.avatar = avatar.id;
                RefreshPanel();
                SaveUserAvatar(avatar);
                // avatarPanel.Hide();
            }
        }
        
        private void OnClickCardback(CardBackData cb)
        {
            userData = Authenticator.Get().UserData;
            if (cb != null && userData != null && IsYou())
            {
                userData.cardback = cb.id;
                RefreshPanel();
                SaveUserCardback(cb);
                // cardbackPanel.Hide();
            }
        }
        
        private async void SaveUserAvatar(AvatarData avatar)
        {
            if (ApiClient.Get().IsConnected())
            {
                string url = ApiClient.ServerURL + "/users/edit/" + ApiClient.Get().UserID;
                EditUserRequest req = new EditUserRequest();
                req.avatar = avatar.id;
                string json_data = ApiTool.ToJson(req);
                await ApiClient.Get().SendRequest(url, "POST", json_data);
            }
            await Authenticator.Get().SaveUserData();
            MainMenu.Get().RefreshUserData();
            RefreshPanel();
        }
        
        private async void SaveUserCardback(CardBackData cardback)
        {
            if (ApiClient.Get().IsConnected())
            {
                string url = ApiClient.ServerURL + "/users/edit/" + ApiClient.Get().UserID;
                EditUserRequest req = new EditUserRequest();
                req.cardback = cardback.id;
                string json_data = ApiTool.ToJson(req);
                await ApiClient.Get().SendRequest(url, "POST", json_data);
            }
            await Authenticator.Get().SaveUserData();
            MainMenu.Get().RefreshUserData();
            RefreshPanel();
        }
        
        public void OnClickAvatar()
        {
            if (!IsYou())
                return;

            RefreshAvatarList();
            // avatarPanel.Show();
        }
        
        public void OnClickCardBack()
        {
            if (!IsYou())
                return;

            RefreshCardBackList();
            // cardbackPanel.Show();
        }
        
        public void OnClickFriends()
        {
            FriendPanel.Get().Show();
        }

        public void OnClickDuplicates()
        {
            SellDuplicatePanel.Get().Show();
        }
        
        public void OnClickEdit()
        {
            // userEmail.readOnly = true;
            // userPasswordPrev.readOnly = true;
            // userPasswordNew.readOnly = true;
            // userPasswordConfirm.readOnly = true;
            // userPasswordNew.gameObject.SetActive(false);
            // userPasswordConfirm.gameObject.SetActive(false);
            //
            // UserData udata = Authenticator.Get().UserData;
            // userEmail.text = udata.email;
            // userPasswordConfirm.text = "password";
            // userPasswordNew.text = "password";
            // userPasswordConfirm.text = "password";
            // editChangeEmail.gameObject.SetActive(true);
            // editChangePassword.gameObject.SetActive(true);
            // resendButton.gameObject.SetActive(udata.validationLevel == 0);
            // confirmButton.gameObject.SetActive(false);
            // editError.text = "";
            // editPanel.Show();
        }

        public void OnClickChangePass()
        {
            OnClickEdit();
            // userPasswordPrev.readOnly = false;
            // userPasswordNew.readOnly = false;
            // userPasswordConfirm.readOnly = false;
            // userPasswordPrev.text = "";
            // userPasswordNew.text = "";
            // userPasswordConfirm.text = "";
            // userPasswordNew.gameObject.SetActive(true);
            // userPasswordConfirm.gameObject.SetActive(true);
            // editChangeEmail.gameObject.SetActive(false);
            // editChangePassword.gameObject.SetActive(false);
            // resendButton.gameObject.SetActive(false);
            // confirmButton.gameObject.SetActive(true);
            // userPasswordPrev.Select();
        }
        
        public void OnClickChangeEmail()
        {
            OnClickEdit();
            // userEmail.readOnly = false;
            // editChangeEmail.gameObject.SetActive(false);
            // editChangePassword.gameObject.SetActive(false);
            // resendButton.gameObject.SetActive(false);
            // confirmButton.gameObject.SetActive(true);
            // userEmail.Select();
        }
        
        public async void OnClickResendConfirm()
        {
            // editError.text = "";
            // string url = ApiClient.ServerURL + "/users/email/resend";
            // WebResponse res = await ApiClient.Get().SendPostRequest(url, "");
            // if (res.success)
            // {
            //     editPanel.Hide();
            // }
            // else
            // {
            //     editError.text = res.error;
            // }
        }
        
        public async void OnClickEditConfirm()
        {
            // editError.text = "";
            //
            // if (!userEmail.readOnly && userEmail.text.Length > 0)
            // {
            //     EditEmailRequest req = new EditEmailRequest();
            //     req.email = userEmail.text;
            //     string url = ApiClient.ServerURL + "/users/email/edit/";
            //     string json = ApiTool.ToJson(req);
            //     WebResponse res = await ApiClient.Get().SendPostRequest(url, json);
            //     if (res.success)
            //     {
            //         editPanel.Hide();
            //         MainMenu.Get().RefreshUserData();
            //     }
            //     else
            //     {
            //         editError.text = res.error;
            //     }
            // }
            // else if (!userPasswordConfirm.readOnly && userPasswordNew.text.Length > 0)
            // {
            //     if (userPasswordNew.text == userPasswordConfirm.text)
            //     {
            //         EditPasswordRequest req = new EditPasswordRequest();
            //         req.password_previous = userPasswordPrev.text;
            //         req.password_new = userPasswordNew.text;
            //         string url = ApiClient.ServerURL + "/users/password/edit/";
            //         string json = ApiTool.ToJson(req);
            //         WebResponse res = await ApiClient.Get().SendPostRequest(url, json);
            //         if (res.success)
            //         {
            //             editPanel.Hide();
            //         }
            //         else
            //         {
            //             editError.text = res.error;
            //         }
            //     }
            // }
        }

        
        private bool IsYou()
        {
            return username == ApiClient.Get().Username;
        }
        
        public void ShowPlayer()
        {
            string user = ApiClient.Get().Username;
            ShowPlayer(user);
        }
        
        public void ShowPlayer(string user)
        {
            if (username != user)
                ClearPanel();
            username = user;
            LoadData();
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            ShowPlayer();
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            // editPanel.Hide();
        }

        public static PlayerPanel Get()
        {
            return instance;
        }

    

       
    }
}