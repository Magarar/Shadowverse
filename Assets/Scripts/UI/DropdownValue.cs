using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class DropdownValue:MonoBehaviour
    {
        public UnityAction<int,string> onValueChanged;
        
        private List<DropdownValueItem> values = new List<DropdownValueItem>();
        
        private TMP_Dropdown dropdown;
        
        void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();
            dropdown.onValueChanged.AddListener(OnChangeValue);
        }
        
        private void Start()
        {

        }

        public void AddOption(string id, string text)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(text);
            dropdown.options.Add(option);
            DropdownValueItem item = new DropdownValueItem();
            item.id = id;
            item.text = text;
            values.Add(item);
            dropdown.RefreshShownValue();
        }
        
        public void ClearOptions()
        {
            values.Clear();
            dropdown.ClearOptions();
        }

        public void SetValue(string value)
        {
            int index = 0;
            foreach (DropdownValueItem item in values)
            {
                if (item.id == value)
                    dropdown.value = index;
                index++;
            }
        }
        
        public void SetValue(int index)
        {
            if (index >= 0 && index < dropdown.options.Count)
                dropdown.value = index;
        }

        private void OnChangeValue(int selectedIndex)
        {
            if (selectedIndex >= 0 && selectedIndex < values.Count)
            {
                DropdownValueItem value = values[selectedIndex];
                onValueChanged?.Invoke(selectedIndex, value.id);
            }
        }

        public DropdownValueItem GetSelected()
        {
            if (dropdown.value >= 0 && dropdown.value < values.Count)
            {
                DropdownValueItem item = values[dropdown.value];
                return item;
            }
            return null;
        }

        public string GetSelectedValue()
        {
            DropdownValueItem item = GetSelected();
            if (item != null)
                return item.id;
            return "";
        }
        
        public string GetSelectedText()
        {
            DropdownValueItem item = GetSelected();
            if (item != null)
                return item.text;
            return "";
        }
 
        
        
        public int GetSelectedIndex()
        {
            return dropdown.value;
        }

        public int Count => dropdown.options.Count;

        public bool interactable
        {
            get => dropdown.interactable;
            set => dropdown.interactable = value;
        }

        public int value
        {
            get => dropdown.value;
            set { dropdown.value = value; dropdown.RefreshShownValue(); }
        }

        public List<DropdownValueItem> Items => values;
    }
    
    [Serializable]
    public class DropdownValueItem
    {
        public string id;
        public string text;
    }
}