using System.Collections.Generic;
using Data;
using Network;
using UI;
using Unit;
using UnityEngine;

namespace GameClient
{
    /// <summary>
    /// Visual representation of a card found in a pack (once opened)
    /// The card can be flipped by clicking on it
    /// </summary>
    public class PackCard:MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float flipSpeed = 10f;

        public SpriteRenderer cardBack;
        public CardUI cardUI;

        public GameObject newCard;

        [Header("FX")] 
        public GameObject cardFlipFX;
        public GameObject cardRareFlipFX;
        public AudioClip cardFlipAudio;
        public AudioClip cardRareFlipAudio;
        
        private CardData icard;
        private VariantData variant;
        
        private Vector3 target;
        private Quaternion rtarget;
        private bool revealed = false;
        private bool removed = false;
        private bool isNew = false;
        private float timer = 0f;
        
        private static List<PackCard> cardList = new List<PackCard>();

        private void Awake()
        {
            cardList.Add(this);
        }

        private void OnDestroy()
        {
            cardList.Remove(this);
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            if (revealed)
            {
                timer += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, rtarget, flipSpeed * timer);
            }
            
            if (removed && timer > 4f)
                Destroy(gameObject);
        }

        public void SetCard(PackData pack, CardData card, VariantData variant)
        {
            icard = card;
            this.variant = variant;
            
            if(cardBack != null)
                cardBack.sprite = pack.cardBackImg;
            
            cardUI.SetCard(card, variant);
            newCard?.SetActive(false);
            
            UserData udata = Authenticator.Get().GetUserData();
            isNew = !udata.HasCard(icard.id, variant.id);


        }

        public void SetTarget(Vector3 pos)
        {
            target = pos;
            rtarget = Quaternion.Euler(0, 180,0);
            transform.rotation = rtarget;
        }

        public void Reveal()
        {
            if(revealed)
                return;
            
            revealed = true;
            rtarget = Quaternion.Euler(0, 0,0);
            newCard?.SetActive(true);
            cardBack.enabled = false;

            if (icard != null && icard.rarity.rank > 5)
            {
                FXTool.DoFX(cardRareFlipFX, transform.position);
                AudioTool.Get().PlaySFX("pack_open", cardRareFlipAudio);
            }
            else
            {
                FXTool.DoFX(cardFlipFX, transform.position);
                AudioTool.Get().PlaySFX("pack_open", cardFlipAudio);
            }
        }
        
        public void Remove()
        {
            if (removed)
                return;

            removed = true;
            timer = 0f;
            target = Vector3.up * 10f;
        }
        
        public void OnMouseDown()
        {
            if (!GameUI.IsOverUILayer("UI"))
            {
                Reveal();
            }
        }
        
        public bool IsRevealed()
        {
            return revealed && timer > 0.5f;
        } public static List<PackCard> GetAll()
        {
            return cardList;
        }


    }
}