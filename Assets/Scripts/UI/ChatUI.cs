using System.Collections.Generic;
using System.Linq;
using GameClient;
using GameLogic;
using TMPro;
using Unit;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Chat area in the UI, 
    /// its where you can write chat msg and will display chat message received from the server
    /// </summary>
    public class ChatUI:MonoBehaviour
    {
        public bool isOpponent;
        
        [Header("Display Box")]
        public ChatBubble chatBubble;
        public AudioClip chatAudio;
        
        [Header("Write Box")]
        public UIPanel chatFieldArea;
        public TMP_InputField chatField;
        
        private string chatMsg;
        private float chatTimer = 0f;
        
        private static List<ChatUI> uiList = new List<ChatUI>();

        private void Awake()
        {
            uiList.Add(this);
        }
        
        private void OnDestroy()
        {
            uiList.Remove(this);
        }

        private void Start()
        {
            Gameclient.Get().onChatMsg += OnChat;
            RefreshChat();
        }
        
        private void Update()
        {
            if(!Gameclient.Get().IsReady())
                return;
            
            int playerID = isOpponent ? Gameclient.Get().GetOpponentPlayerID() : Gameclient.Get().GetPlayerID();
            Game data = Gameclient.Get().GetGameData();
            Player player = data.GetPlayer(playerID);

            if (player != null)
            {
                //Chat
                if (chatFieldArea != null && !isOpponent && Input.GetKeyDown(KeyCode.Return))
                {
                    if (chatFieldArea.IsVisible())
                    {
                        if(!string.IsNullOrWhiteSpace(chatField.text))
                            SendChat(chatField.text);
                        chatField.text = "";
                        chatFieldArea.Hide();
                        GUI.FocusControl(null);
                    }
                    else
                    {
                        chatFieldArea.Show();
                    }
                }
            }
            //Chat remove
            chatTimer += Time.deltaTime;
            if (chatTimer > 5f)
                chatMsg = null;
            
        }

        private void SendChat(string msg)
        {
            Gameclient.Get().SendChatMsg(msg);
        }

        private void RefreshChat()
        {
            chatBubble.Hide();
            if(!string.IsNullOrWhiteSpace(chatMsg))
                chatBubble.SetLine(chatMsg, 5f);
        }

        private void OnChat(int chatPlayerID, string msg)
        {
            int playerID = isOpponent ? Gameclient.Get().GetOpponentPlayerID() : Gameclient.Get().GetPlayerID();
            if (playerID == chatPlayerID)
            {
                chatMsg = msg;
                chatTimer = 0f;
                AudioTool.Get().PlaySFX("chat", chatAudio);
                RefreshChat();
            }
        }

        public void OnClickSend()
        {
            if (chatFieldArea != null && !string.IsNullOrWhiteSpace(chatField.text))
            {
                SendChat(chatField.text);
                chatField.text = "";
                chatFieldArea.Hide();
                GUI.FocusControl(null);
            }
        }

        public static ChatUI Get(bool opponent)
        {
            return uiList.FirstOrDefault(ui => ui.isOpponent == opponent);
        }

      

       

    }
}