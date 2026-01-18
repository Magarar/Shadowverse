using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class CardBackDataMenuBuilder
    {
        private static CreateNewCardBackData createNewCardBackData;

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("CardBackData", null);
            createNewCardBackData = new CreateNewCardBackData();
            tree.Add("CardBackData/Create New", createNewCardBackData);
            tree.AddAllAssetsAtPath("CardBackData", "Assets/Resources/CardBack", typeof(CardBackData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewCardBackData?.cardBackData);
        }
    }

    public class CreateNewCardBackData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public CardBackData cardBackData;

        public CreateNewCardBackData()
        {
            cardBackData = ScriptableObject.CreateInstance<CardBackData>();
            cardBackData.id = "New CardBack";
        }

        [Button("Add New CardBack")]
        private void CreateNewData()
        {
            string path = "Assets/Resources/CardBack/";
            AssetDatabase.CreateAsset(cardBackData, path + cardBackData.id + ".asset");
            AssetDatabase.SaveAssets();
            cardBackData = ScriptableObject.CreateInstance<CardBackData>();
            cardBackData.id = "New CardBack";
        }
    }
    
}