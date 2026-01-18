using System.Collections.Generic;
using Data;
using GameLogic;
using Network;
using Unit;
using UnityEngine;

namespace GameClient
{
    /// <summary>
    /// Area of the hand of packs, will spawn/despawns visual packs based on what player has in the data
    /// </summary>
    public class HandPackArea:MonoBehaviour
    {
        public GameObject packTemple;
        public RectTransform handArea;
        public float cardSpacing = 100f;
        public float cardAngel = 10f;
        public float cardOffsetY = 10f;
        
        private List<HandPack> packs = new();
        
        private Vector3 startPos;
        private bool isDragging;
        private bool isLocked;

        private string lastDestroyed;
        private float lastDestroyedTimer;
        
        private static HandPackArea instance;

        public void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            packTemple.SetActive(false);
            startPos = handArea.anchoredPosition;
            if(Authenticator.Get().IsConnected())
                LoadPacks();
            else
                RefreshLogin();
        }

        private async void RefreshLogin()
        {
            bool success = await Authenticator.Get().RefreshLogin();
            if(success)
                LoadPacks();
            else
                SceneNav.GoTo("LoginMenu");
        }

        public async void LoadPacks()
        {
            UserData udata = await Authenticator.Get().LoadUserData();
            if (udata != null)
            {
                RefreshPacks();
            }
        }

        private void RefreshPacks()
        {
            UserData udata = Authenticator.Get().UserData;
            
            foreach (UserCardData pack in udata.packs)
            {
                PackData dpack = PackData.Get(pack.tid);
                if (dpack != null && !HasPack(pack.tid))
                    SpawnNewPack(pack);
            }
            
            //Remove removed cards
            for (int i = packs.Count - 1; i >= 0; i--)
            {
                HandPack pack = packs[i];
                if (pack == null || !udata.HasPack(pack.GetPackTid()))
                {
                    packs.RemoveAt(i);
                    if (pack)
                        pack.Remove();
                }
            }
        }

        void Update()
        {
            lastDestroyedTimer += Time.deltaTime;
            
            //Set card index
            int index = 0;
            float countHalf = packs.Count / 2f;
            foreach (HandPack pack in packs)
            {
                pack.deckPosition = new Vector2((index - countHalf) * cardSpacing,
                    (index - countHalf) * (index - countHalf) * -cardOffsetY);
                pack.deckAngle = (index-countHalf) * -cardAngel;
                index++;

            }
            
            HandPack dragPack = HandPack.GetDrag();
            isDragging = dragPack != null;
        }
        
        

        private void SpawnNewPack(UserCardData pack)
        {
            GameObject cardObj = Instantiate(packTemple, handArea.transform);
            cardObj.SetActive(true);
            cardObj.GetComponent<HandPack>().SetPack(pack);
            cardObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -100f);
            packs.Add(cardObj.GetComponent<HandPack>());
        }

        public void DelayRefresh(Card card)
        {
            lastDestroyed = card.uid;
            lastDestroyedTimer = 0;
        }

        public void Lock(bool locked)
        {
            isLocked = locked;
        }
        
        public void SortCards()
        {
            packs.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            int i = 0;
            foreach (HandPack acard in packs)
            {
                acard.transform.SetSiblingIndex(i);
                i++;
            }
        }

        private bool HasPack(string packTid)
        {
            HandPack card = HandPack.Get(packTid);
            bool just_destroyed = packTid == lastDestroyed && lastDestroyedTimer < 0.5f;
            return card != null || just_destroyed;
        }

        public bool IsDragging()
        {
            return isDragging;
        }

        public bool IsLocked()
        {
            return isLocked;
        }
        
        public static HandPackArea Get()
        {
            return instance;
        }
    }
}