using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class DeckDataMenuBuilder
    {
        private static CreateNewDeckData createNewDeckData;

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            tree.Add("DeckData", null);
            createNewDeckData = new CreateNewDeckData();
            tree.Add("DeckData/Create New", createNewDeckData);
            tree.AddAllAssetsAtPath("DeckData", "Assets/Resources/Deck", typeof(DeckData), true);
        }

        public static void OnDestroy()
        {
            Object.DestroyImmediate(createNewDeckData?.deckData);
        }
        
    }

    public class CreateNewDeckData
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public DeckData deckData;

        public CreateNewDeckData()
        {
            deckData = ScriptableObject.CreateInstance<DeckData>();
            deckData.id = "New Deck";
        }

        [Button("Add New Deck")]
        private void CreateNewDeck()
        {
            string path = "Assets/Resources/Deck";
            AssetDatabase.CreateAsset(deckData, path + "/" + deckData.id + ".asset");
            AssetDatabase.SaveAssets();
            deckData = ScriptableObject.CreateInstance<DeckData>();
            deckData.id = "New Deck";
        }
    }
}