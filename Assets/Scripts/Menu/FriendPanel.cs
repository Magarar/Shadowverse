using System;
using System.Collections.Generic;
using Api;
using Network;
using TMPro;
using UI;
using Unit;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    /// <summary>
    /// Friend panel is the panel that contain all your list of friend, and where you can invite new ones
    /// </summary>
    public class FriendPanel: UIPanel
    {
        public ScrollRect friendScroll;
        public RectTransform friendContent;
        public FriendLine linePrefab;
        public TMP_InputField friendInput;
        public UI.TabButton friendsTab;
        public UI.TabButton requestsTab;
        public int onlineDuration = 10; //In minutes
        public TextMeshProUGUI testMsg;
        public TextMeshProUGUI error;
        
        private List<FriendLine> friendLines = new List<FriendLine>();

        private static FriendPanel instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
            InitLines();
        }
        
        protected override void Start()
        {
            base.Start();

            friendsTab.onClick += RefreshPanel;
            requestsTab.onClick += RefreshPanel;
        }
        
        private void InitLines()
        {
            int nlines = 100;
            for (int i = 0; i < nlines; i++)
            {
                FriendLine line = AddLine(linePrefab, i);
                line.Hide();
                friendLines.Add(line);
            }

            friendScroll.verticalNormalizedPosition = 1f;
        }
        
        private FriendLine AddLine(FriendLine template, int index)
        {
            GameObject line = Instantiate(template.gameObject, friendContent);
            RectTransform rtrans = line.GetComponent<RectTransform>();
            FriendLine rline = line.GetComponent<FriendLine>();
            rline.onClick += OnClickFriendLine;
            rline.onClickAccept += OnClickFriendAccept;
            rline.onClickReject += OnClickFriendReject;
            rline.onClickWatch += OnClickFriendWatch;
            rline.onClickChallenge += OnClickFriendChallenge;
            return rline;
        }
        
        private async void RefreshPanel()
        {
            foreach (FriendLine line in friendLines)
                line.Hide();

            if(testMsg != null)
                testMsg.enabled = Authenticator.Get().IsTest();

            if (!Authenticator.Get().IsApi())
                return;

            string url = ApiClient.ServerURL + "/users/friends/list";
            WebResponse res = await ApiClient.Get().SendGetRequest(url);
            if (res.success)
            {
                FriendResponse contractList = ApiTool.JsonToObject<FriendResponse>(res.data);
                if (friendsTab.active)
                    SetFriends(contractList);
                else if (requestsTab.active)
                    SetRequests(contractList);
            }
        }
        
        private void SetFriends(FriendResponse contractList)
        {
            DateTime serverTime = DateTime.Parse(contractList.server_time);
            DateTime loginTime = serverTime.AddMinutes(-onlineDuration);

            int index = 0;
            foreach (FriendData user in contractList.friends)
            {
                if (index < friendLines.Count)
                {
                    FriendLine line = friendLines[index];
                    bool valid = DateTime.TryParse(user.last_online_time, out DateTime last_login);
                    bool online = valid && last_login > loginTime;
                    line.SetLine(user, online);
                }
                index++;
            }
        }
        
        private void SetRequests(FriendResponse contractList)
        {
            DateTime serverTime = DateTime.Parse(contractList.server_time);
            DateTime loginTime = serverTime.AddMinutes(-10);

            int index = 0;
            foreach (FriendData user in contractList.friends_requests)
            {
                if (index < friendLines.Count)
                {
                    FriendLine line = friendLines[index];
                    bool valid = DateTime.TryParse(user.last_online_time, out DateTime last_login);
                    bool online = valid && last_login > loginTime;
                    line.SetLine(user, online, true);
                }
                index++;
            }
        }
        
        private async void AddFriend(string fuser)
        {
            FriendAddRequest req = new FriendAddRequest();
            req.username = fuser;

            string url = ApiClient.ServerURL + "/users/friends/add";
            string json = ApiTool.ToJson(req);

            WebResponse res = await ApiClient.Get().SendPostRequest(url, json);
            if (res.success)
            {
                RefreshPanel();
            }
            else
            {
                error.text = res.error;
            }
        }
        
        private async void RemoveFriend(string fuser)
        {
            FriendAddRequest req = new FriendAddRequest();
            req.username = fuser;

            string url = ApiClient.ServerURL + "/users/friends/remove";
            string json = ApiTool.ToJson(req);

            WebResponse res = await ApiClient.Get().SendPostRequest(url, json);
            if (res.success)
            {
                RefreshPanel();
            }
            else
            {
                error.text = res.error;
            }
        }
        
        public void OnClickBack()
        {
            Hide();
        }

        private void OnClickFriendLine(FriendLine user)
        {

        }

        private void OnClickFriendAccept(FriendLine user)
        {
            FriendData friend = user.GetFriend();
            AddFriend(friend.username);
        }

        private void OnClickFriendReject(FriendLine user)
        {
            FriendData friend = user.GetFriend();
            RemoveFriend(friend.username);
        }

        private void OnClickFriendWatch(FriendLine user)
        {
            FriendData friend = user.GetFriend();
            MainMenu.Get().StartObserve(friend.username);
        }

        private void OnClickFriendChallenge(FriendLine user)
        {
            FriendData friend = user.GetFriend();
            MainMenu.Get().StartChallenge(friend.username);
        }
        
        public void OnClickAddFriend()
        {
            string fuser = friendInput.text;
            if (string.IsNullOrWhiteSpace(fuser))
                return;

            error.text = "";
            AddFriend(fuser);
        }

        public void OnClickRemoveFriend()
        {
            string fuser = friendInput.text;
            if (string.IsNullOrWhiteSpace(fuser))
                return;

            error.text = "";
            RemoveFriend(fuser);
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            error.text = "";
            friendInput.text = "";
            friendsTab.Activate();
            RefreshPanel();
        }

        public static FriendPanel Get()
        {
            return instance;
        }
        
        

    }
}