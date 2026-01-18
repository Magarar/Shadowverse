using System.Collections.Generic;
using System.Linq;
using Data;
using GameLogic;
using UI;
using Unit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameClient
{
    public class BoardArticle:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        public Image articleImage;
        public Sprite defaultSprite;
        public Sprite articleSprite;
        
        private string articleUID;
        
        private float focusTimer = 0f;
        
        private bool focus = false;
        private bool drag = false;
        private bool selected = false;
        
        private BoardArticleArea boardArticleArea;

        private void Awake()
        {
            boardArticleArea = GetComponentInParent<BoardArticleArea>();
        }

        private void OnDestroy()
        {
        }

        private void Update()
        {
            if(!Gameclient.Get().IsReady())
                return;
            
            Article article = GetArticle();
        }

        
        public void SetDefault()
        {
            articleUID = "";
            articleImage.sprite = defaultSprite;
        }

        public void SetArticle(Article article)
        {
            articleUID = article.uid;
            articleSprite = article.ArticleData.art;
            articleImage.sprite = articleSprite;
        }

        public string GetArticleUID()
        {
            return articleUID;
        }
        
        public Article GetArticle()
        {
            Game gdata = Gameclient.Get().GetGameData();
            return gdata.GetArticle(articleUID);
        }

        public ArticleData GetArticleData()
        {
            Article article = GetArticle();
            return article?.ArticleData;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (GameUI.IsUIOpened())
                return;

            if (GameTool.IsMobile())
                return;
            focus = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            
        }
        
        public ArticleData ArticleData => GetArticleData();
        
        public bool IsFocus()
        {
            if (GameTool.IsMobile())
                return selected && !drag;
            return focus && !drag && focusTimer > 0f;
        }
        
        public bool IsDrag()
        {
            return drag;
        }


        
    }
}