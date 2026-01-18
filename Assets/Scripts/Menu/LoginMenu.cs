using System.Collections;
using Network;
using TMPro;
using UI;
using Unit;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Menu
{
    public class LoginMenu:MonoBehaviour
    {
        [Header("Login")]
        public UIPanel loginPanel;
        public TMP_InputField loginUsername;
        public TMP_InputField loginPassword;
        public Button loginButton;
        public GameObject loginBottom;
        public TextMeshProUGUI errorMsg;
        
        [Header("Register")]
        public UIPanel registerPanel;
        public TMP_InputField registerUsername;
        public TMP_InputField registerPassword;
        public TMP_InputField registerPasswordConfirm;
        public TMP_InputField registerEmail;
        public Button registerButton;
        
        [Header("Other")]
        public GameObject testArea;
        
        [Header("Music")]
        public AudioClip music;

        private bool clicked = false;

        private static LoginMenu instance;
        
        void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            AudioTool.Get().PlayMusic("music", music);
            BlackPanel.Get().Show(true);
            errorMsg.text = "";
            //testArea.SetActive(Authenticator.Get().IsTest());
            
            string user = PlayerPrefs.GetString("tcg_last_user", "");
            loginUsername.text = user;
            
            if (Authenticator.Get().IsTest())
            {
                loginPassword.gameObject.SetActive(false);
                loginBottom.SetActive(false);
            }
            else if (!string.IsNullOrEmpty(user))
            {
                SelectField(loginPassword);
            }

            RefreshLogin();
        }

        void Update()
        {
            loginButton.interactable = !clicked&&!string.IsNullOrWhiteSpace(loginUsername.text);
            registerButton.interactable = !clicked&&!string.IsNullOrWhiteSpace(registerUsername.text)&&
                                          !string.IsNullOrWhiteSpace(registerPassword.text)&&
                                          !string.IsNullOrWhiteSpace(registerEmail.text)&&
                                          registerPassword.text == registerPasswordConfirm.text;

            if (loginPanel.IsVisible())
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    SelectField(loginUsername.isFocused ? loginPassword : loginUsername);
                }
                
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (loginButton.interactable)
                        OnClickLogin();
                }
            }

            if (registerPanel.IsVisible())
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    if (registerUsername.isFocused)
                        SelectField(registerEmail);
                    else if (registerEmail.isFocused)
                        SelectField(registerPassword);
                    else if (registerPassword.isFocused)
                        SelectField(registerPasswordConfirm);
                    else
                        SelectField(registerUsername);
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (registerButton.interactable)
                        OnClickRegister();
                }
            }
        }

        private async void RefreshLogin()
        {
            bool success = await Authenticator.Get().RefreshLogin();
            if (success)
            {
                SceneNav.GoTo("Menu");
            }
            else
            {
                loginPanel.Show();
                BlackPanel.Get().Hide();
            }
        }

        private async void Login(string user, string password)
        {
            clicked = true;
            errorMsg.text = "";
            
            bool success = await Authenticator.Get().Login(user, password);
            if (success)
            {
                PlayerPrefs.SetString("tcg_last_user", loginUsername.text);
                FadeToScene("Menu");
            }else
            {
                clicked = false;
                errorMsg.text = Authenticator.Get().GetError();
            }
        }

        private async void Register(string email, string username, string password)
        {
            clicked = true;
            errorMsg.text = "";
            bool success = await Authenticator.Get().Register(email, username, password);
            if (success)
            {
                loginUsername.text = username;
                loginPassword.text = password;
                loginPanel.Show();
                registerPanel.Hide();
            }
            else
            {
                errorMsg.text = Authenticator.Get().GetError();
            }
            clicked = false;
        }

        public void OnClickLogin()
        {
            if (string.IsNullOrEmpty(loginUsername.text))
                return;
            if(clicked)
                return;
            
            Login(loginUsername.text, loginPassword.text);
        }

        public void OnClickRegister()
        {
            if (string.IsNullOrEmpty(registerUsername.text))
                return;
            if(string.IsNullOrEmpty(registerPassword.text))
                return;
            
            if(registerPassword.text != registerPasswordConfirm.text)
                return;
            if(clicked)
                return;
            
            Register(registerEmail.text, registerUsername.text, registerPassword.text);
        }

        public void OnClickSwitchLogin()
        {
            loginPanel.Show();
            registerPanel.Hide();
            loginUsername.text = "";
            loginPassword.text = "";
            errorMsg.text = "";
            SelectField(loginUsername);
        }

        public void OnClickSwitchRegister()
        {
            loginPanel.Hide();
            registerPanel.Show();
            errorMsg.text = "";
            SelectField(registerUsername);
        }
        
        public void OnClickSwitchReset()
        {
            RecoveryPanel.Get().Show();
        }
        
        public void OnClickGo()
        {
            FadeToScene("Menu");
        }
        
        public void OnClickQuit()
        {
            Application.Quit();
        }
        
        private void SelectField(TMP_InputField field)
        {
            if (!GameTool.IsMobile())
                field.Select();
        }
        
        public void FadeToScene(string scene)
        {
            StartCoroutine(FadeToRun(scene));
        }

        private IEnumerator FadeToRun(string scene)
        {
            BlackPanel.Get().Show();
            AudioTool.Get().FadeOutMusic("music");
            yield return new WaitForSeconds(1f);
            SceneNav.GoTo(scene);
        }

        public static LoginMenu Get()
        {
            return instance;
        }

        
    }
}