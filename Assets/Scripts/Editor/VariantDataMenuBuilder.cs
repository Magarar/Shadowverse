using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class VariantDataMenuBuilder
    {
        private static CreateNewVariantData createNewVariantData;

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("VariantData", null);
            createNewVariantData = new CreateNewVariantData();
            // 使用从DataEditor传入的createNewCardData实例
            tree.Add("VariantData/Create New", createNewVariantData);

            tree.AddAllAssetsAtPath("VariantData/Variant", "Assets/Resources/Variants", typeof(VariantData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewVariantData?.variantData);
        }
    }

    public class CreateNewVariantData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public VariantData variantData;

        public CreateNewVariantData()
        {
            variantData = ScriptableObject.CreateInstance<VariantData>();
            variantData.id = "New Variant";
        }

        [Button("Create New Variant")]
        private void CreateNewData()
        {
            string path = "Assets/Resources/Variants";
            AssetDatabase.CreateAsset(variantData, path + variantData.id + ".asset");
            AssetDatabase.SaveAssets();
            variantData = ScriptableObject.CreateInstance<VariantData>();
            variantData.id = "New Variant";
        }
    }
}