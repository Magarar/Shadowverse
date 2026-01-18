using System.IO;
using Data;
using UI;
using UnityEngine;

namespace Unit
{
    ///<总结>
    ///使用此功能将您的所有卡片导出为png图像（这样它们就可以在卡片顶部显示统计信息/ui）
    ///</摘要>
    public class CardExporter:MonoBehaviour
    {
        public string exportPath = "/Export/";
        
        public int width = 856;
        public int height = 1200;
        public VariantData variant;
        
        [Header("Reference")]
        public Camera renderCam;
        public CardUI cardUI;
        
        private RenderTexture texture;
        private Texture2D exportTexture;

        private void Start()
        {
            if (variant == null)
                variant = VariantData.GetDefault();
            GenerateAll();
        }

        private async void GenerateAll()
        {
            QualitySettings.SetQualityLevel(QualitySettings.names.Length -1); //Set Max Quality level
            
            texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            exportTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Point;
            exportTexture.filterMode = FilterMode.Point;
            renderCam.targetTexture = texture;
            renderCam.orthographicSize = height / 2.0f;
            
            var cards = CardData.GetAll();
            for (int i = 0; i < cards.Count; i++)
            {
                CardData card = cards[i];
                if (card.deckbuilding)
                {
                    ShowText("Exporting: " + card.id);
                    GenerateCard(card);
                    await TimeTool.Delay(1);
                    ExportCard(card);
                    await TimeTool.Delay(2);
                }
            }
            ShowText("Completed!");
        }
        
        private void GenerateCard(CardData card)
        {
            cardUI.SetCard(card, variant);
            renderCam.Render();
        }
        
        private void ExportCard(CardData card)
        {
            RenderTexture.active = texture;
            exportTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            byte[] bytes = exportTexture.EncodeToPNG();
            string file = card.id + ".png";
            File.WriteAllBytes(Application.persistentDataPath+exportPath + "/" + file, bytes);
            RenderTexture.active = null;
        }
        
        private void ShowText(string txt)
        {
            Debug.Log(txt);
        }
    }
}