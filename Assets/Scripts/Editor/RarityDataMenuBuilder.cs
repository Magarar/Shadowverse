using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class RarityDataMenuBuilder
    {
        private static CreateNewRarityData createNewRarityData;

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("RarityData", null);
            createNewRarityData = new CreateNewRarityData();
            tree.Add("RarityData/Create New", createNewRarityData);
            tree.AddAllAssetsAtPath("RarityData", "Assets/Resources/Rarity", typeof(RarityData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewRarityData?.rarityData);
        }
    }

    public class CreateNewRarityData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public RarityData rarityData;

        public CreateNewRarityData()
        {
            rarityData = ScriptableObject.CreateInstance<RarityData>();
            rarityData.id = "New Rarity";
        }

        [Button("Create New Rarity")]
        private void CreateNewData()
        {
            AssetDatabase.CreateAsset(rarityData, "Assets/Resources/Rarity/" + rarityData.id + ".asset");
            AssetDatabase.SaveAssets();
            rarityData = ScriptableObject.CreateInstance<RarityData>();
            rarityData.id = "New Rarity";
        }
        
    }
}