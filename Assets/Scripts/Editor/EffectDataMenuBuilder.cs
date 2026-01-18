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
    public static class EffectDataMenuBuilder
    {
        private static CreateNewEffectData createNewEffectData;

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("EffectData", null);
            createNewEffectData = new CreateNewEffectData();
            tree.Add("EffectData/Create New", createNewEffectData);
            tree.AddAllAssetsAtPath("EffectData/Effects", "Assets/Resources/Effect",typeof(EffectData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewEffectData?.effectData);
        }
    }

    public class CreateNewEffectData
    {
        public string title = "New Effect";
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public EffectData effectData;
        
        [SerializeField]
        [ValueDropdown("GetEffectDataTypeNames")]
        [OnValueChanged("OnEffectTypeChanged")]
        private string selectedTypeName = "";
        
        private Dictionary<string, Type> effectTypeMap;
        
        public CreateNewEffectData()
        {
            InitializeTypeMap();
            CreateNewInstance();
        }

        private void InitializeTypeMap()
        {
            effectTypeMap = new Dictionary<string, Type>();
            var baseType = typeof(EffectData);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(baseType) && !type.IsAbstract)
                    {
                        effectTypeMap[type.Name] = type;
                    }
                }
            }

            if (effectTypeMap.Count > 0)
            {
                selectedTypeName = effectTypeMap.Keys.First();
            }
        }

        private IEnumerable<string> GetEffectDataTypeNames()
        {
            return effectTypeMap?.Keys ?? Enumerable.Empty<string>();
        }

        private void OnEffectTypeChanged()
        {
            CreateNewInstance();
        }

        private void CreateNewInstance()
        {
            if (effectTypeMap != null && effectTypeMap.ContainsKey(selectedTypeName))
            {
                effectData = ScriptableObject.CreateInstance(effectTypeMap[selectedTypeName]) as EffectData;
            }
            else
            {
                effectData = ScriptableObject.CreateInstance<EffectData>();
            }
        }

        [Button("Add New Effect")]
        private void CreateNewData()
        {
            if (effectData != null)
            {
                string path = "Assets/Resources/Effect/";
                AssetDatabase.CreateAsset(effectData, path + title + ".asset");
                AssetDatabase.SaveAssets();
                
                CreateNewInstance();
            }
        }
        
        
    }
}