using Data;
using GameLogic;
using TMPro;
using UI;
using Unit;
using UnityEngine;

namespace GameClient
{
    /// <summary>
    /// Represents the visual deck on the board
    /// Will show number of cards in deck/discard when hovering
    /// </summary>
    public class BoardDeck:MonoBehaviour
    {
        public bool opponent;
        public UIPanel hoverPanel;
        public SpriteRenderer deckRender;
        public TextMeshProUGUI deckValue;
        public TextMeshProUGUI discardValue;
        
        private bool hover = false;
        
        void Start()
        {
            if (GameTool.IsMobile())
            {
                hoverPanel?.SetVisible(true);
            }
        }
        
        void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (!Gameclient.Get().IsReady())
                return;
            
            Player player = opponent?Gameclient.Get().GetOpponentPlayer():Gameclient.Get().GetPlayer();
            if (player == null)
                return;
            
            CardBackData cb = CardBackData.Get(player.cardBack);
            if(deckRender!=null&&cb!=null)
                deckRender.sprite = cb.deck;
            if (deckValue != null)
                deckValue.text = player.cardsDeck.Count.ToString();
            if (discardValue != null)
                discardValue.text = player.cardsDiscard.Count.ToString();
        }

        public void ShowDeckCards()
        {
            Player player = Gameclient.Get().GetPlayer();
            CardSelector.Get().Show(player.cardsDeck, "DECK");
        }
        
        public void ShowDiscardCards()
        {
            Player player = opponent ? Gameclient.Get().GetOpponentPlayer() : Gameclient.Get().GetPlayer();
            CardSelector.Get().Show(player.cardsDiscard, "DISCARD");
        }
        
        private void ShowHover(bool hover)
        {
            if(!GameTool.IsMobile())
                hoverPanel?.SetVisible(hover);
        }
        
        private void OnMouseEnter()
        {
            hover = true;
            ShowHover(hover);
            Refresh();
        }
        
        private void OnMouseExit()
        {
            hover = false;
            ShowHover(hover);
        }
        
        private void OnDisable()
        {
            hover = false;
            ShowHover(hover);
        }
        
        private void OnMouseOver()
        {
            if (!opponent && Input.GetMouseButtonDown(0))
                ShowDeckCards(); //Cannot see opponent deck
            else if(Input.GetMouseButtonDown(1))
                ShowDiscardCards(); //Cant see both player discard
        }
    }
}