using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class LevelDataMenuBuilder
    {
        private static CreateNewLevelData createNewLevelData;

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("LevelData", null);
            createNewLevelData = new CreateNewLevelData();
            tree.Add("LevelData/Create New", createNewLevelData);
            tree.AddAllAssetsAtPath("LevelData/Level", "Assets/Resources/Level", typeof(LevelData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewLevelData?.levelData);
        }
        
    }

    public class CreateNewLevelData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public LevelData levelData;

        public CreateNewLevelData()
        {
            levelData = ScriptableObject.CreateInstance<LevelData>();
            levelData.id = "New Level";
        }

        [Button("Add New Level")]
        private void CreateNewData()
        {
            string path = "Assets/Resources/Level/";
            AssetDatabase.CreateAsset(levelData, path + levelData.GetTitle() + ".asset");
            AssetDatabase.SaveAssets();
            levelData = ScriptableObject.CreateInstance<LevelData>();
            levelData.id = "New Level";
        }
    }
}