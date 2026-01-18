using System.Collections.Generic;
using Api;
using Data;
using Network;
using TMPro;
using UI;
using Unit;
using UnityEngine;

namespace Menu
{
    /// <summary>
    /// Leaderboard panel contains the ranking of all top players
    /// </summary>
    public class LeaderboardPanel:UIPanel
    {
        public RectTransform content;
        public RankLine lineTemplate;
        public RankLine myLine;
        public float lineSpace = 80f;
        public TextMeshProUGUI testText;
        
        private List<RankLine> lines = new List<RankLine>();
        
        private static LeaderboardPanel instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
            //lines = scroll_content.GetComponentsInChildren<RankLine>();

            myLine.onClick += OnClickLine;
            InitLines();
        }

        private void InitLines()
        {
            for (int i = 0; i < content.transform.childCount; i++)
                Destroy(content.transform.GetChild(i).gameObject);

            int nLines = 100;
            for (int i = 0; i < nLines; i++)
            {
                RankLine line = AddLine(lineTemplate, i);
                lines.Add(line);
            }
        }

        private RankLine AddLine(RankLine template, int i)
        {
            Vector2 pos = Vector2.down * lineSpace;
            GameObject line = Instantiate(template.gameObject, content);
            RectTransform rtrans = line.GetComponent<RectTransform>();
            RankLine rankLine = line.GetComponent<RankLine>();
            rtrans.anchorMin = new Vector2(0.5f, 1f);
            rtrans.anchorMax = new Vector2(0.5f, 1f);
            rtrans.anchoredPosition = pos + Vector2.down * i * lineSpace;
            rankLine.onClick += OnClickLine;
            return rankLine;
        }

        private async void RefreshPanel()
        {
            myLine.Hide();
            foreach (RankLine line in lines)
                line.Hide();
            
//            testText.enabled = !Authenticator.Get().IsApi();
            
            if (!Authenticator.Get().IsApi())
                return;
            
            UserData udata = ApiClient.Get().UserData;
            int index = 0;
            string url = ApiClient.ServerURL + "/users";
            WebResponse res = await ApiClient.Get().SendGetRequest(url);
            
            UserData[] users = ApiTool.JsonToArray<UserData>(res.data);
            List<UserData> sortedUsers = new List<UserData>(users);
            sortedUsers.Sort((a, b) => b.elo.CompareTo(a.elo));
            
            int previousRank = 0;
            int previousIndex = 0;

            foreach (UserData user in sortedUsers)
            {
                if (user.permissionLevel != 1 || user.matches == 0)
                    continue; //Dont show admins and user with no matches
                if (user.username == udata.username)
                {
                    myLine.SetLine(user, index + 1, true);
                }
                if (index < lines.Count)
                {
                    RankLine line = lines[index];
                    int rankOrder = (previousRank == user.elo) ? previousIndex : index;
                    line.SetLine(user, rankOrder + 1, user.username == udata.username);
                    previousRank = user.elo;
                    previousIndex = rankOrder;
                }

                index++;
            }
        }

        private void OnClickLine(string username)
        {
            
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }
        
        public void OnClickBack()
        {
            Hide();
        }

        public static LeaderboardPanel Get()
        {
            return instance;
        }
    }
}