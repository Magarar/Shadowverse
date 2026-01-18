using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class AvatarDataMenuBuilder
    {
        private static CreateNewAvatarData createNewAvatarData;
        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("AvatarData", null);
            createNewAvatarData = new CreateNewAvatarData();
            tree.Add("AvatarData/Create New", createNewAvatarData);
            tree.AddAllAssetsAtPath("AvatarData", "Assets/Resources/Avatar", typeof(AvatarData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewAvatarData?.avatarData);
        }
    }

    public class CreateNewAvatarData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public AvatarData avatarData;

        public CreateNewAvatarData()
        {
            avatarData = ScriptableObject.CreateInstance<AvatarData>();
            avatarData.id = "New Avatar";
        }

        [Button("Add New Avatar")]
        private void CreateNewData()
        {
            string path = "Assets/Resources/Avatar";
            AssetDatabase.CreateAsset(avatarData, path + "/" + avatarData.id + ".asset");
        }
    }
}