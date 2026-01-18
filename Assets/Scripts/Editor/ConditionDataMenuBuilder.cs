using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public static class ConditionDataMenuBuilder
    {
        private static CreateNewConditionData createNewConditionData;
        
        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("ConditionData", null);
            createNewConditionData = new CreateNewConditionData();
            tree.Add("ConditionData/Create New", createNewConditionData);

            tree.AddAllAssetsAtPath("ConditionData/Conditions", "Assets/Resources/Conditions", typeof(ConditionData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewConditionData?.conditionData);
        }
    }

    public class CreateNewConditionData
    {
        public string title;
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public ConditionData conditionData;

        // 添加类型选择字段
        [SerializeField]
        [ValueDropdown("GetConditionDataTypeNames")]
        [OnValueChanged("OnConditionTypeChanged")]
        private string selectedTypeName = "";

        private Dictionary<string, Type> conditionTypeMap;

        public CreateNewConditionData()
        {
            InitializeTypeMap();
            CreateNewInstance();
        }

        private void InitializeTypeMap()
        {
            conditionTypeMap = new Dictionary<string, Type>();
            var baseType = typeof(ConditionData);
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(baseType) && !type.IsAbstract)
                    {
                        conditionTypeMap[type.Name] = type;
                    }
                }
            }
            
            // 设置默认选择
            if (conditionTypeMap.Count > 0)
            {
                selectedTypeName = conditionTypeMap.Keys.First();
            }
        }

        private IEnumerable<string> GetConditionDataTypeNames()
        {
            return conditionTypeMap?.Keys ?? Enumerable.Empty<string>();
        }

        private void OnConditionTypeChanged()
        {
            CreateNewInstance();
        }

        // 创建新实例的方法
        private void CreateNewInstance()
        {
            if (conditionTypeMap != null && conditionTypeMap.ContainsKey(selectedTypeName))
            {
                conditionData = ScriptableObject.CreateInstance(conditionTypeMap[selectedTypeName]) as ConditionData;
            }
            else
            {
                conditionData = ScriptableObject.CreateInstance<ConditionData>();
            }
        }

        [Button("Add New Condition")]
        private void CreateNewData()
        {
            if (conditionData != null)
            {
                string path = "Assets/Resources/Conditions/";
                AssetDatabase.CreateAsset(conditionData, path + title + ".asset");
                AssetDatabase.SaveAssets();
                
                CreateNewInstance(); // 创建一个新的实例用于下次创建
            }
        }
    }
}
