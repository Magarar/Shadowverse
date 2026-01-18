using Data;
using TMPro;
using UI;
using Unit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menu
{
    public class DeckLine: MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image image;
        public Image frame;
        public TextMeshProUGUI title;
        public TextMeshProUGUI value;
        public IconValue cost;
        public UIPanel deleteBtn;
        public AudioClip clickAudio;
        public Material disabledMat;
        public Material defaultMat;
        
        public UnityAction<DeckLine> onClick;
        public UnityAction<DeckLine> onClickRight;
        public UnityAction<DeckLine> onClickDelete;
        
        private CardData card;
        private VariantData variant;
        private DeckData deck;
        private UserDeckData udeck;
        private bool hidden = false;
        private bool hover = false;
        
        void Update()
        {
            if (deleteBtn != null)
            {
                bool visi = hover || GameTool.IsMobile();
                deleteBtn.SetVisible(visi && !hidden && udeck != null);
            }
        }

        public void SetLine(CardData card, VariantData variant, int quantity, bool invalid = false)
        {
            this.card = card;
            this.variant = variant;
            this.deck = null;
            this.udeck = null;
            hidden = false;
            
            if (title != null)
                title.text = card.title;
            if (title != null)
                title.color = variant.color;
            if (value != null)
                value.text = quantity.ToString();
            if (value != null)
                value.enabled = quantity > 1;
            if (cost != null)
                cost.value = card.mana;
            if (this.value != null)
                this.value.color = invalid ? Color.red : Color.white;
            if(invalid)
                title.color = Color.gray;
            
            if (image != null)
            {
                image.sprite = card.GetFullArt(variant);
                image.enabled = true;
                image.material = invalid ? disabledMat : defaultMat;
            }

            if (frame != null)
            {
                frame.sprite = variant.frame;
                frame.enabled = true;
                frame.material = invalid ? disabledMat : defaultMat;
            }

            gameObject.SetActive(true);
        }
        
        public void SetLine(DeckData deck)
        {
            this.card = null;
            this.deck = deck;
            this.udeck = null;
            hidden = false;

            if (this.title != null)
                this.title.text = deck.title;
            if (this.title != null)
                this.title.color = Color.white;
            if (this.value != null)
                this.value.text = deck.GetQuantity().ToString();
            if (this.value != null)
                this.value.enabled = deck.GetQuantity() > 0;

            gameObject.SetActive(true);
        }
        
        public void SetLine(UserData udata, UserDeckData deck)
        {
            this.card = null;
            this.deck = null;
            this.udeck = deck;
            hidden = false;

            if (this.title != null)
                this.title.text = deck.title;
            if (this.title != null)
                this.title.color = Color.white;
            if (this.value != null)
                this.value.text = deck.GetQuantity().ToString() + "/" + GamePlayData.Get().deckSize;
            if (this.value != null)
                this.value.enabled = deck.GetQuantity() > 0;
            if (this.value != null)
                this.value.color = udata.IsDeckValid(deck) ? Color.white : Color.red;

            gameObject.SetActive(true);
        }
        
        public void SetLine(string title)
        {
            this.card = null;
            this.deck = null;
            this.udeck = null;
            hidden = false;

            if (this.title != null)
                this.title.text = title;
            if (this.title != null)
                this.title.color = Color.white;

            if (this.value != null)
                this.value.enabled = false;

            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            this.card = null;
            this.deck = null;
            this.udeck = null;
            hidden = true;
            hover = false;

            if (title != null)
                title.text = "";
            if (this.title != null)
                this.title.color = Color.white;
            if (value != null)
                value.text = "";
            if (value != null)
                value.enabled = true;
            if (cost != null)
                cost.value = 0;
            if (image != null)
                image.enabled = false;
            if (frame != null)
                frame.enabled = false;
            if (deleteBtn != null)
                deleteBtn.SetVisible(false);

            gameObject.SetActive(false);
        }

        public CardData GetCard()
        {
            return card;
        }

        public VariantData GetVariant()
        {
            return variant;
        }

        public DeckData GetDeck()
        {
            return deck;
        }

        public UserDeckData GetUserDeck()
        {
            return udeck;
        }

        public bool IsHidden()
        {
            return hidden;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (hidden)
                return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                onClick?.Invoke(this);
                AudioTool.Get().PlaySFX("ui", clickAudio);
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                onClickRight?.Invoke(this);
                AudioTool.Get().PlaySFX("ui", clickAudio);
            }
        }

        public void OnClickDelete()
        {
            onClickDelete?.Invoke(this);
            AudioTool.Get().PlaySFX("ui", clickAudio);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hover = false;
        }
    }
}