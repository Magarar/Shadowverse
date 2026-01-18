using Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Menu
{
    /// <summary>
    /// Visual representation of a card in your collection in the Deckbuilder
    /// </summary>

    public class CollectionCard: MonoBehaviour
    {
        public CardUI cardUI;
        public Image quantityBar;
        public TextMeshProUGUI quantity;
        
        [Header("Mat")]
        public Material colorMat;
        public Material grayscaleMat;
        
        public UnityAction<CardUI> onClick;
        public UnityAction<CardUI> onClickRight;
        
        private void Start()
        {
            cardUI.onClick += onClick;
            cardUI.onClickRight += onClickRight;
        }
        
        public void SetCard(CardData card, VariantData variant, int quantity)
        {
            cardUI.SetCard(card, variant);
            SetQuantity(quantity);
        }
        
        public void SetQuantity(int quantity)
        {
            if (this.quantityBar != null)
                this.quantityBar.enabled = quantity > 0;
            if (this.quantity != null)
                this.quantity.text = quantity.ToString();
            if (this.quantity != null)
                this.quantity.enabled = quantity > 0;
        }
        
        public void SetGrayscale(bool grayscale)
        {
            if (grayscale)
            {
                quantityBar.material = grayscaleMat;
                quantityBar.material = grayscaleMat;
                cardUI.SetMaterial(grayscaleMat);
            }
            else
            {
                quantityBar.material = colorMat;
                quantityBar.material = colorMat;
                cardUI.SetMaterial(colorMat);
            }
        }

        public CardData GetCard()
        {
            return cardUI.GetCard();
        }

        public VariantData GetVariant()
        {
            return cardUI.GetVariant();
        }
    }
}