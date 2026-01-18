using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class StatusDataMenuBuilder
    {
        private static CreateNewStatusData createNewStatusData;

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("StatusData", null);
            createNewStatusData = new CreateNewStatusData();
            tree.Add("StatusData/Create New", createNewStatusData);
            tree.AddAllAssetsAtPath("StatusData/Status", "Assets/Resources/Status", typeof(StatusData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewStatusData?.statusData);
        }
    }

    public class CreateNewStatusData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public StatusData statusData;

        public CreateNewStatusData()
        {
            statusData = ScriptableObject.CreateInstance<StatusData>();
            statusData.title = "New Status";
        }

        [Button("Create New Status")]
        public void CreateNewStatus()
        {
            AssetDatabase.CreateAsset(statusData, "Assets/Data/Status/" + statusData.title + ".asset");
            AssetDatabase.SaveAssets();
            statusData = ScriptableObject.CreateInstance<StatusData>();
            statusData.title = "New Status";
        }
    }
}