using System;
using Api;
using TMPro;
using UI;
using Unit;
using UnityEngine.UI;

namespace Menu
{
    /// <summary>
    /// Password recovery panel in the login menu
    /// Only available in API mode
    /// </summary>
    public class RecoveryPanel:UIPanel
    {
        public TMP_InputField resetEmail;
        public TextMeshProUGUI resetError;

        public UIPanel confirmPanel;
        public TMP_InputField confirmCode;
        public TMP_InputField confirmPassword;
        public TMP_InputField confirmPassConfirm;
        public TextMeshProUGUI confirmError;
        
        private static RecoveryPanel instance;

        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        public virtual void RefreshPanel()
        {
            confirmPanel.Hide(true);
            resetEmail.text = "";
            confirmCode.text = "";
            confirmPassword.text = "";
            confirmPassConfirm.text = "";
            resetError.text = "";
            confirmError.text = "";
        }

        public async void ResetPassword()
        {
            if (ApiClient.Get().IsBusy())
                return;

            resetError.text = "";

            if (resetEmail.text.Length == 0)
                return;

            ResetPasswordRequest req = new ResetPasswordRequest();
            req.email = resetEmail.text;

            string url = ApiClient.ServerURL + "/users/password/reset";
            string json = ApiTool.ToJson(req);
            WebResponse res = await ApiClient.Get().SendPostRequest(url, json);
            if (!res.success)
            {
                resetError.text = res.error;
            }
            else
            {
                confirmPanel.Show();
            }
        }

        public async void ResetPasswordConfirm()
        {
            if (ApiClient.Get().IsBusy())
                return;
            confirmError.text = "";
            if (confirmCode.text.Length == 0 || confirmPassword.text.Length == 0 || confirmPassConfirm.text.Length == 0)
                return;
            if (confirmPassword.text != confirmPassConfirm.text)
            {
                confirmError.text = "Passwords don't match";
                return;
            }
            ResetConfirmPasswordRequest req = new ResetConfirmPasswordRequest();
            req.email = resetEmail.text;
            req.code = confirmCode.text;
            req.password = confirmPassword.text;
            
            string url = ApiClient.ServerURL + "/users/password/reset/confirm";
            string json = ApiTool.ToJson(req);
            WebResponse res = await ApiClient.Get().SendPostRequest(url, json);
            if (!res.success)
            {
                confirmError.text = res.error;
            }
            else
            {
                LoginMenu.Get().loginUsername.text = req.email;
                LoginMenu.Get().loginPassword.text = "";
                Hide();
            }
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            confirmPanel.Hide();
        }
        
        public void OnClickReset()
        {
            ResetPassword();
        }

        public void OnClickResetConfirm()
        {
            ResetPasswordConfirm();
        }

        public void OnClickBack()
        {
            Hide();
        }
        
        public static RecoveryPanel Get()
        {
            return instance;
        }
    }
    
    [Serializable]
    public class ResetPasswordRequest
    {
        public string email;
    }

    [Serializable]
    public class ResetConfirmPasswordRequest
    {
        public string email;
        public string code;
        public string password;
    }
}