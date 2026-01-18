using Data;
using GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Scripts to display all stats of a card, 
    /// is used by other script that display cards like BoardCard, and HandCard, CollectionCard..
    /// </summary>
    public class CardUI:MonoBehaviour,IPointerClickHandler
    {
        public Image cardImage;
        public Image frameImage;
        public Image teamIcon;
        public Image rarityIcon;
        public Image attackIcon;
        public Image hpIcon;
        public Image costIcon;
        public TextMeshProUGUI hp;
        public TextMeshProUGUI attack;
        public TextMeshProUGUI cost;
        
        public TextMeshProUGUI cardTitle;
        public TextMeshProUGUI cardText;

        public TraitUI[] stats;
        
        public UnityAction<CardUI> onClick;
        public UnityAction<CardUI> onClickRight;

        private CardData card;
        private VariantData variant;

        private void Awake()
        {
            
        }
        
        public void SetCard(Card card)
        {
            if (card == null)
                return;
            
            SetCard(card.Data, card.VariantData);
            if(cost!=null)
                cost.text = card.GetMana().ToString();
            if (cost != null && card.CardData.IsDynamicManaCost())
                cost.text = "X";
            if (hp != null)
                hp.text = card.GetHp().ToString();
            if (attack != null)
                attack.text = card.attack.ToString();
            
            foreach (TraitUI stat in stats)
                stat.SetCard(card);
        }

        public void SetCard(CardData card, VariantData variant)
        {
            if(card==null)
                return;
            
            this.card = card;
            this.variant = variant;
            
            if(cardImage!=null)
                cardImage.sprite = card.GetFullArt(variant);
            if(frameImage!=null)
                frameImage.sprite = variant.frame;
            if(cardTitle!=null)
                cardTitle.text = card.GetTitle().ToUpper();
            if(cardText!=null)
                cardText.text = card.GetText();
            
            if(attackIcon!=null)
                attackIcon.enabled = card.IsCharacter();
            if (attack != null)
                attack.enabled = card.IsCharacter();
            if(hpIcon!=null)
                hpIcon.enabled = card.IsBoardCard()||card.IsEquipment();
            if (hp != null)
                hp.enabled = card.IsBoardCard()||card.IsEquipment();
            if (costIcon != null)
                costIcon.enabled = card.type != CardType.Hero;
            if (cost != null)
                cost.enabled = card.type != CardType.Hero;
            
            if (cost != null)
                cost.text = card.mana.ToString();
            if (cost != null && card.IsDynamicManaCost())
                cost.text = "X";
            if (attack != null)
                attack.text = card.attack.ToString();
            if (hp != null)
                hp.text = card.hp.ToString();

            if (teamIcon != null)
            {
                teamIcon.sprite = card.team.icon;
                teamIcon.enabled = teamIcon.sprite != null;
            }

            if (rarityIcon != null)
            {
                rarityIcon.sprite = card.rarity.icon;
                rarityIcon.enabled = rarityIcon.sprite != null;
            }
            
            foreach (TraitUI stat in stats)
                stat.SetCard(card);
            
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

        }

        public void SetHP(int hpValue)
        {
            if (hp != null)
                hp.text = hpValue.ToString();
        }

        public void SetMaterial(Material mat)
        {
            if(cardImage!=null)
                cardImage.material = mat;
            if(frameImage!=null)
                frameImage.material = mat;
            if(teamIcon!=null)
                teamIcon.material = mat;
            if(rarityIcon!=null)
                rarityIcon.material = mat;
            if(attackIcon!=null)
                attackIcon.material = mat;
            if(hpIcon!=null)
                hpIcon.material = mat;
            if(costIcon!=null)
                costIcon.material = mat;
        }

        public void SetOpacity(float opacity)
        {
            if(cardImage!=null)
                cardImage.color = new Color(cardImage.color.r, cardImage.color.g, cardImage.color.b, opacity);
            if(frameImage!=null)
                frameImage.color = new Color(frameImage.color.r, frameImage.color.g, frameImage.color.b, opacity);
            if(teamIcon!=null)
                teamIcon.color = new Color(teamIcon.color.r, teamIcon.color.g, teamIcon.color.b, opacity);
            if(rarityIcon!=null)
                rarityIcon.color = new Color(rarityIcon.color.r, rarityIcon.color.g, rarityIcon.color.b, opacity);
            if(attackIcon!=null)
                attackIcon.color = new Color(attackIcon.color.r, attackIcon.color.g, attackIcon.color.b, opacity);
            if(hpIcon!=null)
                hpIcon.color = new Color(hpIcon.color.r, hpIcon.color.g, hpIcon.color.b, opacity);
            if(costIcon!=null)
                costIcon.color = new Color(costIcon.color.r, costIcon.color.g, costIcon.color.b, opacity);
            if(hp!=null)
                hp.color = new Color(hp.color.r, hp.color.g, hp.color.b, opacity);
            if(attack!=null)
                attack.color = new Color(attack.color.r, attack.color.g, attack.color.b, opacity);
            if(cost!=null)
                cost.color = new Color(cost.color.r, cost.color.g, cost.color.b, opacity);
            if(cardTitle!=null)
                cardTitle.color = new Color(cardTitle.color.r, cardTitle.color.g, cardTitle.color.b, opacity);
            if(cardText!=null)
                cardText.color = new Color(cardText.color.r, cardText.color.g, cardText.color.b, opacity);
        }
        
        public void Hide()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                onClick?.Invoke(this);
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                onClickRight?.Invoke(this);
            }
        }
        
        public CardData GetCard()
        {
            return card;
        }

        public VariantData GetVariant()
        {
            return variant;
        }
        

        
    }
}