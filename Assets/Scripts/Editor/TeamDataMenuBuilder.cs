using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class TeamDataMenuBuilder
    {
        private static CreateNewTeamData createNewTeamData;

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("TeamData", null);
            createNewTeamData = new CreateNewTeamData();
            tree.Add("TeamData/Create New", createNewTeamData);
            tree.AddAllAssetsAtPath("TeamData/Team", "Assets/Resources/", typeof(TeamData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewTeamData?.teamData);
        }
    }

    public class CreateNewTeamData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public TeamData teamData;

        public CreateNewTeamData()
        {
            teamData = ScriptableObject.CreateInstance<TeamData>();
            teamData.id = "New Team";
        }

        [Button("Add New Team")]
        private void CreateNewData()
        {
            string path = "Assets/Resources/TeamData/";
            AssetDatabase.CreateAsset(teamData, path + teamData.id+ ".asset");
            AssetDatabase.SaveAssets();
            teamData = ScriptableObject.CreateInstance<TeamData>();
            teamData.id = "New Team";
        }
    }
}