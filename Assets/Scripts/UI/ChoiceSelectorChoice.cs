using Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// One choice in the choice selector
    /// Its a button you can click
    /// </summary>
    public class ChoiceSelectorChoice:MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI subtitle;
        public Image highlight;
        
        public UnityAction<int> onClick;
        
        private Button button;
        private int choice;
        private bool focus = false;
        
        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }
        
        private void Update()
        {
            if (highlight != null)
                highlight.enabled = focus;
        }
        
        public void SetChoice(int index, AbilityData ability)
        {
            this.choice = index;
            title.text = ability.title;
            subtitle.text = ability.desc;
            button.interactable = true;
            gameObject.SetActive(true);
            
            if(ability.manaCost>0)
                title.text += " ("+ability.manaCost+")";
        }
        
        public void SetInteractable(bool interact)
        {
            button.interactable = interact;
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void OnClick()
        {
            onClick?.Invoke(choice);
        }
        
        public void MouseEnter()
        {
            if (button.interactable)
                focus = true;
        }
        
        public void MouseExit()
        {
            focus = false;
        }

        

    }
}