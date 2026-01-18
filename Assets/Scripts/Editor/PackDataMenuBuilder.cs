using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class PackDataMenuBuilder
    {
        private static CreateNewPackData createNewPackData;

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("PackData", null);
            createNewPackData = new CreateNewPackData();
            tree.Add("PackData/Create New", createNewPackData);
            tree.AddAllAssetsAtPath("PackData", "Assets/Data/Packs", typeof(PackData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewPackData?.packData);
        }
    }

    public class CreateNewPackData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public PackData packData;

        public CreateNewPackData()
        {
            packData = ScriptableObject.CreateInstance<PackData>();
            packData.id = "New Pack";
        }

        [Button("Create New PackData")]
        public void Create()
        {
            AssetDatabase.CreateAsset(packData, "Assets/Data/Packs/" + packData.id + ".asset");
            AssetDatabase.SaveAssets();
            packData = ScriptableObject.CreateInstance<PackData>();
            packData.id = "New Pack";
            
        }
        
    }
}