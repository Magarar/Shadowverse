using Api;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Menu
{
    /// <summary>
    /// One friend in the friend panel
    /// displays avatar, username and has buttons to interact with the friend
    /// </summary>
    public class FriendLine: MonoBehaviour
    {
        public TextMeshProUGUI username;
        public Image avatar;
        
        public Image onlineImg;
        public Sprite onlineSprite;
        public Sprite offlineSprite;
        public Button acceptBtn;
        public Button rejectBtn;
        public Button watchBtn;
        public Button challengeBtn;
        
        public UnityAction<FriendLine> onClick;
        public UnityAction<FriendLine> onClickAccept;
        public UnityAction<FriendLine> onClickReject;
        public UnityAction<FriendLine> onClickWatch;
        public UnityAction<FriendLine> onClickChallenge;
        
        private FriendData fdata;
        private Sprite default_avat;
        
        private void Awake()
        {
            default_avat = avatar.sprite;

            if (acceptBtn != null)
                acceptBtn.onClick.AddListener(() => { onClickAccept?.Invoke(this); });
            if (rejectBtn != null)
                rejectBtn.onClick.AddListener(() => { onClickReject?.Invoke(this); });
            if (watchBtn != null)
                watchBtn.onClick.AddListener(() => { onClickWatch?.Invoke(this); });
            if (challengeBtn != null)
                challengeBtn.onClick.AddListener(() => { onClickChallenge?.Invoke(this); });
        }
        
        public void SetLine(FriendData user, bool online, bool is_request = false)
        {
            fdata = user;
            username.text = user.username;
            avatar.sprite = default_avat;

            if (avatar != null)
            {
                AvatarData avat = AvatarData.Get(user.avatar);
                if (avat != null)
                    avatar.sprite = avat.avatar;
            }

            if (onlineImg != null)
            {
                onlineImg.sprite = online ? onlineSprite : offlineSprite;
            }

            if (watchBtn != null)
                watchBtn.gameObject.SetActive(online && !is_request);
            if (challengeBtn != null)
                challengeBtn.gameObject.SetActive(online && !is_request);

            if (acceptBtn != null)
                acceptBtn.gameObject.SetActive(is_request);
            if (rejectBtn != null)
                rejectBtn.gameObject.SetActive(is_request);

            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnClick()
        {
            onClick?.Invoke(this);
        }

        public FriendData GetFriend()
        {
            return fdata;
        }
    }
}