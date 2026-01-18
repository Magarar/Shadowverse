using System.Collections.Generic;
using System.Linq;
using GameLogic;
using UnityEngine;

namespace GameClient
{
    public class BoardArticleArea:MonoBehaviour
    {
        public GameObject boardArticlePrefab;
        public RectTransform boardArticleArea;
        
        [SerializeField]private List<BoardArticle> boardArticles = new();
        

        private string lastDestroyed;
        private float lastDestroyedTimer;

       
        
        
        public void Awake()
        {
            
        }

        private void Update()
        {
            
        }

        public void UpdateBoardArticles(Player player)
        {
            if (!Gameclient.Get().IsReady())
                return;
            
            Game gdata = Gameclient.Get().GetGameData();
            for (int i = 0; i < player.articles.Count; i++)
            {
                if (player.articles[i] == null)
                {
                    boardArticles[i].SetDefault();
                    continue;
                }
                if (boardArticles[i].GetArticleUID() != player.articles[i].uid)
                {
                    boardArticles[i].SetArticle(player.articles[i]);
                }
            }
            
            for (int i = player.articles.Count; i < boardArticles.Count; i++)
            {
                boardArticles[i].SetDefault();
            }
        }

        public BoardArticle GetFocus()
        {
            return boardArticles.FirstOrDefault(boardArticle => boardArticle.IsFocus());
        }
        

        public List<BoardArticle> GetBoardArticles()
        {
            return boardArticles;
        }
        
    }
}