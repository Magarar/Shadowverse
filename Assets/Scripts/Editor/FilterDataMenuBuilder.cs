using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public static class FilterDataMenuBuilder
    {
        private static CreateNewFilterData createNewFilterData;

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("FilterData", null);
            createNewFilterData = new CreateNewFilterData();
            tree.Add("FilterData/Create New", createNewFilterData);
            tree.AddAllAssetsAtPath("FilterData/Filter", "Assets/Resources/Filter", typeof(FilterData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewFilterData?.filterData);
        }
    }

    public class CreateNewFilterData
    {
        public string title = "Create New Filter";
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public FilterData filterData;
        
        [SerializeField]
        [ValueDropdown("GetFilterDataTypeNames")]
        [OnValueChanged("OnFilterTypeChanged")]
        private string selectedTypeName = "";
        
        private Dictionary<string, Type> filterTypeMap;

        public CreateNewFilterData()
        {
            InitializeTypeMap();
            CreateNewInstance();
        }

        private void InitializeTypeMap()
        {
            filterTypeMap = new Dictionary<string, Type>();
            var baseType = typeof(FilterData);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(baseType) && !type.IsAbstract)
                    {
                        filterTypeMap[type.Name] = type;
                    }
                }
            }
            
            if (filterTypeMap.Count > 0)
                selectedTypeName = filterTypeMap.Keys.First();
        }

        private IEnumerable<string> GetFilterDataTypeNames()
        {
            return filterTypeMap?.Keys ?? Enumerable.Empty<string>();
        }

        private void OnFilterTypeChanged()
        {
            CreateNewInstance();
        }

        private void CreateNewInstance()
        {
            if (filterTypeMap != null && filterTypeMap.ContainsKey(selectedTypeName))
            {
                filterData = ScriptableObject.CreateInstance(filterTypeMap[selectedTypeName]) as FilterData;
            }
            else
            {
                filterData = ScriptableObject.CreateInstance<FilterData>();
            }
            AssetDatabase.SaveAssets();
        }

        [Button("Add New Filter")]
        private void CreateNewData()
        {
            if (filterData != null)
            {
                string path = "Assets/Resources/Filter/";
                AssetDatabase.CreateAsset(filterData, path + title+ ".asset");
                AssetDatabase.SaveAssets();
                CreateNewInstance();
            }
        }
    }
}