using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class AbilityDataMenuBuilder
    {
        private static CreateNewAbilityData createNewAbilityData;
        
        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("AbilityData", null);
            createNewAbilityData = new CreateNewAbilityData();
            tree.Add("AbilityData/Create New", createNewAbilityData);
            tree.AddAllAssetsAtPath("AbilityData", "Assets/Resources/AbilityData", typeof(AbilityData), true);
        }
        

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewAbilityData?.abilityData);
        }
    }

    public class CreateNewAbilityData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public AbilityData abilityData;

        public CreateNewAbilityData()
        {
            abilityData = ScriptableObject.CreateInstance<AbilityData>();
            abilityData.id = "New Ability";
        }

        [Button("Add New Ability")]
        private void CreateNewData()
        {
            string path = "Assets/Resources/AbilityData";
            AssetDatabase.CreateAsset(abilityData, path + "/" + abilityData.GetTitle() + ".asset");
            AssetDatabase.SaveAssets();
            abilityData = ScriptableObject.CreateInstance<AbilityData>();
            abilityData.id = "New Ability";
        }
        
    }
    
    
}