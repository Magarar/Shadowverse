using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class AvatarUI:MonoBehaviour
    {
        public UnityAction<AvatarData> onClick;
        
        private Image avatarImage;
        private Button avatarButton;
        private Sprite defaultIcon;
        
        private AvatarData avatar;

        private void Awake()
        {
            avatarImage = GetComponent<Image>();
            avatarButton = GetComponent<Button>();
            defaultIcon = avatarImage.sprite;

            if (avatarButton != null)
                avatarButton.onClick.AddListener(OnClick);
        }

        public void SetAvatar(AvatarData avatar)
        {
            this.avatar = avatar;
            avatarImage.enabled = true;
            avatarImage.sprite = defaultIcon;
            
            if (avatar != null)
                avatarImage.sprite = avatar.avatar;
        }
        
        public void SetAvatar(Sprite avatar)
        {
            avatarImage.enabled = true;
            avatarImage.sprite = defaultIcon;
            
            if (avatar != null)
                avatarImage.sprite = avatar;
        }

        public void SetDefaultAvatar()
        {
            this.avatar = null;
            avatarImage.enabled = true;
            avatarImage.sprite = defaultIcon;
        }

        public void SetImage(Sprite sprite)
        {
            avatarImage.sprite = sprite;
        }

        public void Hide()
        {
            avatarImage.enabled = false;
            avatar = null;
        }
        
        public AvatarData GetAvatar()
        {
            return avatar;
        }

        private void OnClick()
        {
            onClick?.Invoke(avatar);
        }
    }
}