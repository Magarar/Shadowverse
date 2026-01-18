using Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Display a pack and all its information
    /// </summary>
    public class PackUI:MonoBehaviour, IPointerClickHandler
    {
        public Image packImg;
        public TextMeshProUGUI packTitle;
        public TextMeshProUGUI packQuantity;
        public Image quantityBar;
        
        public UnityAction<PackUI> onClick;
        public UnityAction<PackUI> onClickRight;
        
        private PackData pack;

        public void SetPack(PackData pack)
        {
            this.pack = pack;
            if (pack != null)
            {
                if (packTitle != null)
                {
                    packTitle.text = pack.title;
                    packTitle.enabled = true;
                }
                packImg.enabled = true;
                packImg.sprite = pack.packImg;
            }
            
            if(packQuantity != null)
                packQuantity.enabled = false;
            if(quantityBar != null)
                quantityBar.enabled = false;
        }

        public void SetPack(PackData pack, int quantity)
        {
            SetPack(pack);
            if (packQuantity != null)
            {
                packQuantity.enabled = quantity > 0;
                packQuantity.text = quantity.ToString();
            }

            if (quantityBar != null)
                quantityBar.enabled = quantity > 0;
        }

        public void Hide()
        {
            this.pack = null;
            packImg.enabled = false;
            if(packTitle != null)
                packTitle.enabled = false;
            if (packQuantity != null)
                packQuantity.enabled = false;
            if (quantityBar != null)
                quantityBar.enabled = false;
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
        
        public PackData GetPack()
        {
            return pack;
        }
        
        
        
   
    }
}