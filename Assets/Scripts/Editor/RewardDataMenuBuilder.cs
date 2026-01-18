using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class RewardDataMenuBuilder
    {
        private static CreateNewRewardData createNewRewardData;

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("RewardData", null);
            createNewRewardData = new CreateNewRewardData();
            tree.Add("RewardData/Create New", createNewRewardData);
            tree.AddAllAssetsAtPath("RewardData/Reward", "Assets/Resources/Reward", typeof(RewardData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewRewardData?.rewardData);
        }
        
    }

    public class CreateNewRewardData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public RewardData rewardData;

        public CreateNewRewardData()
        {
            rewardData = ScriptableObject.CreateInstance<RewardData>();
            rewardData.id = "New Reward";
        }

        [Button("Add New Reward")]
        private void CreateNewData()
        {
            string path = "Assets/Resources/Reward/";
            AssetDatabase.CreateAsset(rewardData, path + rewardData.id+ ".asset");
            AssetDatabase.SaveAssets();
            rewardData = ScriptableObject.CreateInstance<RewardData>();
            rewardData.id = "New Reward";
        }
    }
}