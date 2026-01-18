using System.Collections.Generic;
using Data;
using Network;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    /// <summary>
    /// Pack panel is similar to the collection, but shows all the packs you own and all available packs
    /// </summary>
    public class PackPanel: UIPanel
    {
        [Header("Packs")]
        public ScrollRect scrollRect;
        public RectTransform scrollContent;
        public CardGrid gridContent;
        public GameObject packPrefab;
        
        private List<GameObject> packList = new List<GameObject>();

        private static PackPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            //Delete grid content
            for (int i = 0; i < gridContent.transform.childCount; i++)
                Destroy(gridContent.transform.GetChild(i).gameObject);
        }
        
        protected override void Start()
        {
            base.Start();

        }

        protected override void Update()
        {
            base.Update();

        }
        
        public async void ReloadUserPack()
        {
            await Authenticator.Get().LoadUserData();
            RefreshPacks();
        }
        
        private void RefreshAll()
        {
            RefreshPacks();
            RefreshStarterDeck();
        }

        public void RefreshPacks()
        {
            UserData udata = Authenticator.Get().UserData;

            foreach (GameObject card in packList)
                Destroy(card.gameObject);
            packList.Clear();

            foreach (PackData pack in PackData.GetAvailable())
            {
                GameObject nPack = Instantiate(packPrefab, gridContent.transform);
                PackUI packUI = nPack.GetComponentInChildren<PackUI>();
                packUI.SetPack(pack, udata.GetPackQuantity(pack.id));
                packUI.onClick += OnClickPack;
                packUI.onClickRight += OnClickPack;
                packList.Add(nPack);
            }
        }

        private void RefreshStarterDeck()
        {
            UserData udata = Authenticator.Get().UserData;
            if (udata != null && (udata.cards.Length == 0 || udata.rewards.Length == 0))
            {
                if (GamePlayData.Get().starterDecks.Length > 0)
                {
                    StarterDeckPanel.Get().Show();
                }
            }
        }

        public void OnClickPack(PackUI pack)
        {
            PackZoomPanel.Get().ShowPack(pack.GetPack());
        }
        
        public void OnClickCardRight(PackUI pack)
        {
            PackZoomPanel.Get().ShowPack(pack.GetPack());
        }
        
        public void OnClickOpenPacks()
        {
            MainMenu.Get().FadeToScene("OpenPack");
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshAll();
        }

        public static PackPanel Get()
        {
            return instance;
        }
        
        
    }
}