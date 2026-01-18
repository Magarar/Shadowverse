using System.Collections.Generic;
using Data;
using GameClient;
using GameLogic;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class MulliganSelector:SelectorPanel
    {
        public CardMulligan[] cards;

        [FormerlySerializedAs("timerFallout")] public Image timerFillout;
        
        private static MulliganSelector instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;

            foreach (CardMulligan card in cards)
            {
                card.onClick += OnClickCard;
            }
        }

        protected override void Update()
        {
            base.Update();
            
            if(!Gameclient.Get().IsReady()||Gameclient.Get().GetGameData().phase!=GamePhase.Mulligan)
                return;
            
            Game data = Gameclient.Get().GetGameData();
            
            if (timerFillout != null)
                timerFillout.fillAmount = data.turnTimer / GamePlayData.Get().turnDuration;
        }

        private void RefreshMulligan()
        {
            Player player = Gameclient.Get().GetPlayer();
            
            int index = 0;
            foreach (Card card in player.cardsHand)
            {
                string bonusID = GamePlayData.Get().secondBouns != null ? GamePlayData.Get().secondBouns.id : "";
                if (index < cards.Length && card.cardId != bonusID)
                {
                    CardMulligan cardUI = cards[index];
                    cardUI.SetCard(card);
                    index++;
                }
            }
            
        }
        
        private void OnClickCard(CardMulligan cardUI)
        {
            cardUI.SetSelected(!cardUI.IsSelected());
        }
        
        public void OnClickOK()
        {
            List<string> selectedCards = new List<string>();

            foreach (CardMulligan acard in cards)
            {
                if (acard.IsSelected())
                    selectedCards.Add(acard.GetCard().uid);
            }

            Gameclient.Get().Mulligan(selectedCards.ToArray());
            Hide();
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshMulligan();
        }
        
        public override bool ShouldShow()
        {
            Game gdata = Gameclient.Get().GetGameData();
            Player player = Gameclient.Get().GetPlayer();
            return gdata.IsPlayerMulliganTurn(player);
        }
        
        public static MulliganSelector Get()
        {
            return instance;
        }
    }
}