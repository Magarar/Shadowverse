using Data;
using GameClient;
using GameLogic;
using TMPro;
using Unit;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// In the game scene, the CardPreviewUI is what shows the card in big with extra info when hovering a card
    /// </summary>
    public class CardPreviewUI: MonoBehaviour
    {
        public UIPanel uiPanel;
        public CardUI cardUI;
        public TextMeshProUGUI desc;
        public float hoverDelayBoard = 0.7f;
        public float hoverDelayHand = 0.4f;
        public float hoverDelayMobile = 0.1f;
        
        public RectTransform[] sideRows;
        public StatusLine[] statusLines;

        private float previewTimer = 0f;
        private Vector2[] startPos;
        
        private void Start()
        {
            startPos = new Vector2[sideRows.Length];
            for (int i = 0; i < sideRows.Length; i++)
            {
                startPos[i] = sideRows[i].anchoredPosition;
            }
        }

        void Update()
        {
            if(!Gameclient.Get().IsReady())
                return;
            
            foreach (StatusLine line in statusLines)
                line.Hide();
            
            PlayerControls controls = PlayerControls.Get();
            HandCard hcard = HandCard.GetFocus();
            BoardCard bcard = BoardCard.GetFocus();
            HeroUI heroUI = HeroUI.GetFocus();
            Card historyCard = TurnHistoryLine.GetHoverCard();
            Card secretCard = SecretIconUI.GetHoverCard();
            
            float delay = hcard!=null?hoverDelayHand:hoverDelayBoard;
            if(GameTool.IsMobile())
                delay = hoverDelayMobile;
            
            Card pcard = hcard != null ? hcard?.GetCard() : bcard?.GetFocusCard();
            if (pcard == null)
                pcard = historyCard;
            if (pcard == null)
                pcard = secretCard;
            if (pcard == null)
                pcard = heroUI?.GetCard();
            
            bool hoverOnly = !Input.GetMouseButton(0) && !HandCardArea.Get().IsDragging();
            bool shouldShowPreview = hoverOnly && pcard != null && !GameUI.IsUIOpened();

            if (shouldShowPreview)
                previewTimer += Time.deltaTime;
            else
                previewTimer = 0f;
            
            bool showPreview = shouldShowPreview&&previewTimer>delay;
            uiPanel.SetVisible(showPreview);

            if (showPreview)
            {
                CardData icard = pcard.CardData;
                cardUI.SetCard(icard,pcard.VariantData);
                
                string cdesc = icard.GetDesc();
                string adesc = icard.GetAbilitiesDesc();
                
                //CardData
                if (!string.IsNullOrWhiteSpace(cdesc))
                    this.desc.text = cdesc + "\n\n" + adesc;
                else
                    this.desc.text = adesc;
                
                //Abilities
                int index = 0;
                foreach (AbilityData ability in pcard.GetAbilities())
                {
                    if (index < statusLines.Length)
                    {
                        //Dont display default ability (GetAbilitiesDesc does that already)
                        //Card instance
                        if (!pcard.CardData.HasAbility(ability) && !string.IsNullOrWhiteSpace(ability.desc))
                        {
                            statusLines[index].SetLine(pcard.CardData, ability);
                            index++;
                        }
                    }
                }
                
                //Status
                foreach (CardStatus status in pcard.GetAllStatus())
                {
                    if (index < statusLines.Length)
                    {
                        StatusData istatus = StatusData.Get(status.type);
                        if (istatus != null && !string.IsNullOrWhiteSpace(istatus.desc))
                        {
                            int ival = Mathf.Max(status.value, Mathf.CeilToInt(status.duration / 2f));
                            statusLines[index].SetLine(istatus, ival);
                            index++;
                        }
                    }
                }
                
            }

        }
    }
}