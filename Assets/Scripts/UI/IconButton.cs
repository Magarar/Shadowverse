using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// A toggle button that will disable other buttons in same group when clicked
    /// </summary>
    public class IconButton: MonoBehaviour
    {
        public string group;
        public string value;
        
        public Image activeImg;
        public Image disabledImg;
        public bool onIfAllOff;
        
        public UnityAction<IconButton> onClick;
        
        private bool active = false;
        private Button button;
        private static List<IconButton> toggleList = new List<IconButton>();
        
        void Awake()
        {
            toggleList.Add(this);
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);

            if(!onIfAllOff && activeImg != null)
                activeImg.enabled = false;
        }

        private void OnDestroy()
        {
            toggleList.Remove(this);
        }

        private void Update()
        {
            if (onIfAllOff)
            {
                if (activeImg != null && IsAllOff(group))
                {
                    activeImg.enabled = true;
                }
            }
        }
        
        private void OnClick()
        {
            bool wasActive = active;
            DeactivateAll(group);
            if(!wasActive)
                Activate();
            onClick?.Invoke(this);
        }
        
        public void SetActive(bool act)
        {
            if (act) Activate();
            else Deactivate();
        }

        public void Activate()
        {
            active = true;
            if (activeImg != null)
                activeImg.enabled = true;
        }
        
        public void Deactivate()
        {
            active = false;
            if (disabledImg != null)
                activeImg.enabled = false;
        }
        
        public bool IsActive()
        {
            return active;
        }
        

     
        public static bool IsAllOff(string group)
        {
            bool allOff = true;
            foreach (IconButton button in toggleList)
            {
                if (button.group == group && button.IsActive())
                    allOff = false;
            }
            return allOff;
        }
        
        public static void DeactivateAll(string s)
        {
            foreach (IconButton button in toggleList)
            {
                if (button.group == s)
                    button.Deactivate();
            }
        }

        public static List<IconButton> GetAll(string group)
        {
            return toggleList.Where(button => button.group == group).ToList();
        }


        
    }
}