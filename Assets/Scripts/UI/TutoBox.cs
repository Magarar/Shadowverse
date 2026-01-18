    using GameClient;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class TutoBox:MonoBehaviour
    {
        [Header("UI")]
        public Button nextBtn;

        void Awake()
        {

        }

        public void SetNextButton(bool active)
        {
            nextBtn.gameObject.SetActive(active);
        }

        public void OnClickNext()
        {
            Tutorial.Get().ShowNext();
        }
    }
}