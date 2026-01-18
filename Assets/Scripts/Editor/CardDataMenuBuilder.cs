using Data;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace Editor
{
    public static class CardDataMenuBuilder
    {
        private static CreateNewCardData createNewCardData;
        
        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("CardData", null);
            createNewCardData = new CreateNewCardData();
            // 使用从DataEditor传入的createNewCardData实例
            tree.Add("CardData/Create New", createNewCardData);

            tree.AddAllAssetsAtPath("CardData/Card", "Assets/Resources/Card", typeof(CardData), true);
            tree.AddAllAssetsAtPath("CardData/Hero", "Assets/Resources/Hero", typeof(CardData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewCardData?.cardData);
        }
    }

    public class CreateNewCardData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public CardData cardData;

        public CreateNewCardData()
        {
            cardData = ScriptableObject.CreateInstance<CardData>();
            cardData.id = "New Card";
        }

        [Button("Add New Card")]
        private void CreateNewData()
        {
            string path = "Assets/Resources/";
            if (cardData.type == CardType.Hero)
            {
                path += "Hero/";
            }
            else
            {
                path += "Card/";
            }
            AssetDatabase.CreateAsset(cardData, path + cardData.GetTitle() + ".asset");
            AssetDatabase.SaveAssets();
            
            cardData = ScriptableObject.CreateInstance<CardData>();
            cardData.id = "New Card";
        }
    }
}