using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class TraitDataMenuBuilder
    {
        private static CreateNewTraitData createNewTraitData;

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("TraitData", null);
            createNewTraitData = new CreateNewTraitData();
            tree.Add("TraitData/Create New", createNewTraitData);
            tree.AddAllAssetsAtPath("TraitData/Trait", "Assets/Resources/Trait", typeof(TraitData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewTraitData?.traitData);
        }
    }

    public class CreateNewTraitData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public TraitData traitData;

        public CreateNewTraitData()
        {
            traitData = ScriptableObject.CreateInstance<TraitData>();
            traitData.id = "New Trait";
        }

        [Button("Add New Trait")]
        private void CreateNewData()
        {
            string path = "Assets/Resources/Trait/";
            AssetDatabase.CreateAsset(traitData, path + traitData.id+ ".asset");
            AssetDatabase.SaveAssets();
            traitData = ScriptableObject.CreateInstance<TraitData>();
            traitData.id = "New Trait";
        }
    }
}