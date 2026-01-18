using System.Collections.Generic;
using System.Linq;
using Data;
using Menu;
using Network;
using TMPro;
using UI;
using Unit;
using UnityEngine;
using UnityEngine.UI;

namespace GameClient
{
    /// <summary>
    /// Visual representation of a booster pack in hand, for the OpenPack scene
    /// </summary>
    public class HandPack: MonoBehaviour
    {
        public Image packSprite;
        public Image packGlow;
        public TextMeshProUGUI packQuantity;

        public float moveSpeed = 10f;
        public float moveRotateSpeed = 4f;
        public float moveMaxRotate = 10f;
        
        [HideInInspector]public Vector2 deckPosition;
        [HideInInspector]public float deckAngle;
        
        [Header("FX")]
        public GameObject packOpenFx;
        public AudioClip packOpenAudio;

        private string packTid = "";
        private int quantity = 0;
        
        private RectTransform handTransform;
        private RectTransform cardTransform;
        private Vector3 startScale;
        private float currentAlpha = 0f;
        private Vector3 currentRotate;
        private Vector3 targetRotate;
        private Vector3 prevPos;
        
        private bool destroyed = false;
        private float focusTimer = 0f;

        private bool focus = false;
        private bool drag = false;
        
        private static List<HandPack> packList = new List<HandPack>();
        
        private void Awake()
        {
            packList.Add(this);
            cardTransform = GetComponent<RectTransform>();
            handTransform = transform.parent.GetComponent<RectTransform>();
            startScale = transform.localScale;
        }
        
        private void Start()
        {

        }

        private void OnDestroy()
        {
            packList.Remove(this);
        }

        private void Update()
        {
            focusTimer += Time.deltaTime;

            Vector2 targetPos = deckPosition;
            Vector3 targetSize = startScale;
            
            float targetAlpha = 1f;
            bool playerDragging = HandPackArea.Get().IsDragging();

            if (focus && focusTimer > 0.5f)
            {
                targetPos = deckPosition+Vector2.up * 40f;
            }

            if (drag)
            {
                targetPos = GetTargetPosition();
                targetSize = startScale * 0.8f;
                Vector3 dir = cardTransform.position - prevPos;
                Vector3 addrot = new Vector3(dir.y * 90f, -dir.x * 90f, 0f);
                targetRotate += addrot*moveRotateSpeed*Time.deltaTime;
                targetRotate = new Vector3(Mathf.Clamp(targetRotate.x, -moveMaxRotate, moveMaxRotate),
                    Mathf.Clamp(targetRotate.y, -moveMaxRotate, moveMaxRotate), 0f);
                currentRotate = Vector3.Lerp(currentRotate, targetRotate, moveRotateSpeed*Time.deltaTime);
                moveSpeed = 9f;
                targetAlpha = 0.8f;
            }
            else
            {
                targetRotate = new Vector3(0,0,deckAngle);
                currentRotate = new Vector3(0,0,deckAngle);
            }
            
            cardTransform.anchoredPosition = Vector2.Lerp(cardTransform.anchoredPosition, targetPos, moveSpeed*Time.deltaTime);
            cardTransform.rotation = Quaternion.Slerp(cardTransform.localRotation, Quaternion.Euler(currentRotate),
                moveSpeed * Time.deltaTime);
            cardTransform.localScale = Vector3.Lerp(cardTransform.localScale, targetSize, 5f*Time.deltaTime);
            
            packGlow.enabled = (focus&&!playerDragging)||drag;
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, 2f * Time.deltaTime);
            packSprite.color = new Color(1f, 1f, 1f, currentAlpha);
            packGlow.color = new Color(packSprite.color.r, packSprite.color.g, packSprite.color.b, currentAlpha);
            packQuantity.text = quantity.ToString();
            
            prevPos = Vector3.Lerp(prevPos, cardTransform.position, 1f * Time.deltaTime);
        }

        private Vector2 GetTargetPosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(handTransform, Input.mousePosition, Camera.main,
                out Vector2 mousePos);
            return mousePos;
        }

        public void SetPack(UserCardData pack)
        {
            packTid = pack.tid;
            quantity = pack.quantity;
            PackData ipack = PackData.Get(packTid);
            if (ipack != null)
            {
                packSprite.sprite = ipack.packImg;
            }
        }

        public void OpenPack()
        {
            FXTool.DoFX(packOpenFx, transform.position);
            AudioTool.Get().PlaySFX("pack_open",packOpenAudio);
            Destroy(gameObject);
            OpenPackMenu.Get().OpenPack(packTid);
        }

        public void Remove()
        {
            quantity--;
            if (quantity <= 0)
                Kill();
            
        }
        
        public void Kill()
        {
            if (!destroyed)
            {
                destroyed = true;
                Destroy(gameObject);
            }
        }
        
        public bool IsFocus()
        {
            return focus && !drag;
        }

        public bool IsDrag()
        {
            return drag;
        }
        
        public PackData GetPackData()
        {
            return PackData.Get(packTid);
        }
        
        public string GetPackTid()
        {
            return packTid;
        }
        
        public int GetPackQuantity()
        {
            UserData udata = Authenticator.Get().UserData;
            return udata.GetPackQuantity(packTid);
        }
        
        public void OnMouseEnterCard()
        {
            if (HandPackArea.Get().IsLocked())
                return;

            focus = true;
        }
        
        public void OnMouseExitCard()
        {
            focus = false;
            focusTimer = 0f;
        }
        
        public void OnMouseDownCard()
        {
            if (HandPackArea.Get().IsLocked())
                return;

            drag = true;
            AudioTool.Get().PlaySFX("hand_card", AssetData.Get().handCardClickAudio);
        }

        public void OnMouseUpCard()
        {
            Vector3 worldPos = MouseToWorld(Input.mousePosition);
            if (drag && worldPos.y > -2.5f)
                OpenPack();
            else
                HandPackArea.Get().SortCards();
            drag = false;
        }
        
        public Vector3 MouseToWorld(Vector3 mousePos)
        {
            Vector3 wpos = Camera.main.ScreenToWorldPoint(mousePos);
            wpos.z = 0f;
            return wpos;
        }
        
        public static HandPack GetDrag()
        {
            return packList.FirstOrDefault(card => card.IsDrag());
        }
        
        public static HandPack GetFocus()
        {
            return packList.FirstOrDefault(card => card.IsFocus());
        }
        
        public static HandPack Get(string uid)
        {
            return packList.FirstOrDefault(card => card && card.GetPackTid() == uid);
        }

        public static List<HandPack> GetAll()
        {
            return packList;
        }
    }
}