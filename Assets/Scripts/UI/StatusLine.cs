using Data;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// In the CardPreviewUI, a list of current status appear
    /// This is one of those
    /// </summary>
    public class StatusLine: MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI desc;
        
        private float timer = 0f;
        
        private void Start()
        {
            gameObject.SetActive(false);
        }
        void Update()
        {
            timer += Time.deltaTime;
        }
        
        public void Hide()
        {
            if (timer > 0.05f)
                gameObject.SetActive(false);
        }

        public void SetLine(CardData icard, AbilityData ability)
        {
            if (!string.IsNullOrWhiteSpace(ability.desc))
            {
                title.text = ability.GetTitle();
                desc.text = ability.GetDesc(icard);
                gameObject.SetActive(true);
                timer = 0f;
            }
        }
        
        public void SetLine(StatusType effect, int value)
        {
            StatusData sdata = StatusData.Get(effect);
            if (sdata != null)
                SetLine(sdata, value);
        }

        public void SetLine(StatusData effect, int value)
        {
            if (!string.IsNullOrWhiteSpace(effect.desc))
            {
                title.text = effect.GetTitle();
                desc.text = effect.GetDesc(value);
                gameObject.SetActive(true);
                timer = 0f;
            }
        }
    }
}