using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using FX;
using GameLogic;
using TMPro;
using UI;
using Unit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GameClient
{
    /// <summary>
    /// Represents the visual aspect of a card on the board.
    /// Will take the data from Card.cs and display it
    /// </summary>
    public class BoardCard:MonoBehaviour
    {
        public SpriteRenderer cardSprite;
        public SpriteRenderer cardGlow;
        public SpriteRenderer cardShadow;

        public Image armorIcon;
        public TextMeshProUGUI armor;

        public CanvasGroup statusGroup;
        public TextMeshProUGUI statusText;

        public BoardCardEquip equipment;

        public AbilityButton[] buttons;

        public Color glowAlly;
        public Color glowCanAttackPlayer;
        public Color glowEnemy;

        public UnityAction onKill;

        private CardUI cardUI;
        private BoardCardFX cardFX;
        private Canvas canvas;

        private string cardUid = "";
        private bool destroyed = false;
        private bool focus = false;
        private float timer = 0f;
        private float statusAlphaTarget = 0f;
        private float delayedDamageTimer = 0f;
        private int prevHp = 0;

        private bool backToHand = false;
        private Vector3 backToHandTarget;

        private static List<BoardCard> cardList = new();

        private void Awake()
        {
            cardList.Add(this);
            cardUI = GetComponent<CardUI>();
            cardFX = GetComponent<BoardCardFX>();
            canvas = GetComponentInChildren<Canvas>();

            cardGlow.color = new Color(cardGlow.color.r, cardGlow.color.g, cardGlow.color.b, 0f);
            canvas.gameObject.SetActive(false);
            statusAlphaTarget = 0f;

            equipment?.Hide();
            
            if(statusGroup!=null)
                statusGroup.alpha = 0f;
        }

        private void OnDestroy()
        {
            cardList.Remove(this);
        }

        private void Start()
        {
            //Random slight rotation
            Vector3 boardRot = GameBoard.Get().GetAngles();
            transform.rotation = Quaternion.Euler(boardRot.x, boardRot.y, boardRot.z+Random.Range(-1f, 1f));
        }

        private void Update()
        {
            if (!Gameclient.Get().IsReady())
                return;
            
            delayedDamageTimer -= Time.deltaTime;
            timer += Time.deltaTime;
            if(timer>0.15f&&!destroyed&&!canvas.gameObject.activeSelf)
                canvas.gameObject.SetActive(true);
            
            PlayerControls controls = PlayerControls.Get();
            Game data = Gameclient.Get().GetGameData();
            Player player = Gameclient.Get().GetPlayer();
            Card card = data.GetCard(cardUid);

            if (!destroyed)
            {
                cardUI.SetCard(card);
                cardUI.SetHP(prevHp);
            }
            
            //Save Previous HP
            if (!IsDamagedDelayed())
                prevHp = card.GetHp();

            bool selected = controls.GetSelected() == this;
            Vector3 targetPos = GetTargetPos();
            float speed = 12f;
            
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            
            //color
            float targetAlpha = !card.exhausted||IsFocus() || selected ? 1f : 0f;
            if (destroyed|| timer < 0.3f||data.currentPlayer!=player.id)
                targetAlpha = 0f;
            if (equipment != null && equipment.IsFocus())
                targetAlpha = 0f;
            
            Color cardColor = SetGlowColor(player, card);
            float cardAlpha = Mathf.MoveTowards(cardGlow.color.a, targetAlpha * cardColor.a, 4 * Time.deltaTime);
            cardGlow.color = new Color(cardColor.r, cardColor.g, cardColor.b, cardAlpha);
            cardShadow.enabled = !destroyed && timer > .4f;
            cardSprite.color = card.HasStatus(StatusType.Stealth)?Color.gray: Color.white;
            cardUI.hp.color = (destroyed || card.damaged > 0) ? Color.yellow : Color.white;
            
            //armor
            int armorValue = card.GetStatusValue(StatusType.Armor);
            armor.text = armorValue.ToString();
            armor.enabled = armorValue > 0;
            armorIcon.enabled = armorValue > 0;
            
            //Update card image
            Sprite sprite = card.hasEvolved?card.CardData.GetEvolveBoardArt(card.VariantData):card.CardData.GetBoardArt(card.VariantData);
            if(sprite!=null)
                cardSprite.sprite = sprite;

            //Update frame image
            Sprite frame = card.VariantData.frameBoard;
            if(frame!=null&&cardUI.frameImage!=null)
                cardUI.frameImage.sprite = frame;
            
            //Equipment
            if (equipment != null)
            {
                Card equip = data.GetCard(card.equippedUid);
                equipment.SetEquip(equip);
            }
            
            //Ability buttons
            foreach (AbilityButton button in buttons)
            {
                button.Hide();
            }

            if (selected && card.playerID == player.id)
            {
                int index = 0;
                List<AbilityData> abilities = card.GetAbilities();
                foreach (AbilityData ability in abilities)
                {
                    if (ability != null && ability.trigger == AbilityTrigger.Activate)
                    {
                        if (index < buttons.Length)
                        {
                            AbilityButton button = buttons[index];
                            button.SetAbility(card, ability);
                            button.SetInteractable(data.CanCastAbility(card, ability));
                        }
                        index++;
                    }
                    
                }
                
                Card equip = data.GetEquipCard(card.equippedUid);
                if (equip != null)
                {
                    List<AbilityData> equipAbilities = equip.GetAbilities();
                    foreach (AbilityData ability in equipAbilities)
                    {
                        if (ability != null && ability.trigger == AbilityTrigger.Activate)
                        {
                            if (index < buttons.Length)
                            {
                                AbilityButton button = buttons[index];
                                button.SetAbility(equip, ability);
                                button.SetInteractable(data.CanCastAbility(equip, ability));
                            }
                            index++;
                        }
                    }
                }
            }
            
            //Status bar
            if(statusGroup!=null)
                statusGroup.alpha = Mathf.MoveTowards(statusGroup.alpha, statusAlphaTarget, 5 * Time.deltaTime);

        }

        private Color SetGlowColor(Player player, Card card)
        {
            if (player.id != card.playerID)
            {
                return glowEnemy;
            }
            
            if(card.canAttackPlayer&&!card.exhausted)
                return glowCanAttackPlayer;
            return glowAlly;
            
        }

        private Vector3 GetTargetPos()
        {
            Game data = Gameclient.Get().GetGameData();
            Card card = data.GetCard(cardUid);
            
            if(destroyed&&backToHand&&timer>0.5f)
                return backToHandTarget;
            BSlot slot = BSlot.Get(card.slot);
            if (slot != null)
            {
                Vector3 targetPos = slot.GetPosition(card.slot);
                return targetPos;
            }
            return transform.position;
        }
        
        public void SetCard(Card card)
        {
            cardUid = card.uid;
            
            transform.position = GetTargetPos();
            prevHp = card.GetHp();
            
            CardData icard = CardData.Get(card.cardId);
            if (icard)
            {
                cardUI.SetCard(card);
                cardSprite.sprite = card.hasEvolved?icard.GetEvolveBoardArt(card.VariantData):icard.GetBoardArt(card.VariantData);
                armor.enabled = false;
                armorIcon.enabled = false;
                statusAlphaTarget = 0f;
            }
            
        }

        public void SetOrder(int order)
        {
            cardSprite.sortingOrder = order;
            canvas.sortingOrder = order+1;
        }

        public void Destroy()
        {
            if (!destroyed)
            {
                Game data = Gameclient.Get().GetGameData();
                Card card = data.GetCard(cardUid);
                Player player = data.GetPlayer(card.playerID);
                
                destroyed = true;
                timer = 0f;
                statusAlphaTarget = 0f;
                cardGlow.enabled = false;
                cardShadow.enabled = false;
                
                SetOrder(cardSprite.sortingOrder-2);
                Destroy(gameObject,1.3f);
                
                TimeTool.WaitFor(0.8f, () =>
                {
                    canvas.gameObject.SetActive(false);
                });
                
                GameBoard board = GameBoard.Get();
                if (player.HasCard(player.cardsHand, card) || player.HasCard(player.cardsDeck, card))
                {
                    backToHand = true;
                    backToHandTarget = player.id==Gameclient.Get().GetPlayerID()?-board.transform.up:board.transform.up;
                    backToHandTarget *= 10f;
                }

                if (!backToHand)
                {
                    card.hp = 0;
                    cardUI.SetCard(card);
                }
                
                onKill?.Invoke();
            }
        }
        
        //Offset the HP visuals by a value so the HP dont go down before end of animation (like a projectile)
        public void DelayDamage(int damage, float duration = 1f)
        {
            if (damage != 0)
            {
                delayedDamageTimer = duration;
            }
        }

        public bool IsDamagedDelayed()
        {
            return delayedDamageTimer > 0f;
        }

        private void ShowStatusBar()
        {
            Card card = GetCard();
            if (card != null && statusText != null && !destroyed)
            {
                string sText = GetStatusText();
                string traitText = GetTraitText();
                if (sText.Length > 0 && traitText.Length > 0)
                    statusText.text  = traitText+", "+sText;
                else
                    statusText.text = traitText + sText;
            }
            bool showStatus = statusText!=null&&statusText.text.Length > 0;
            statusAlphaTarget = showStatus?1f:0f;
        }

        public string GetStatusText()
        {
            Card card = GetCard();
            string txt = "";
            foreach (CardStatus astatus in card.GetAllStatus())
            {
                StatusData statusData = StatusData.Get(astatus.type);
                if (statusData != null && !string.IsNullOrEmpty(statusData.title))
                {
                    int ival = Mathf.Max(astatus.value, Mathf.CeilToInt(astatus.duration / 2f));
                    string sval = ival>1?" "+ival:"";
                    txt += statusData.GetTitle() + sval + ", ";
                }
            }
            if (txt.Length > 2)
                txt = txt.Substring(0, txt.Length - 2);
            return txt;
        }

        private string GetTraitText()
        {
            Card card = GetCard();
            string txt = "";
            foreach (CardTrait atrait in card.GetAllTraits())
            {
                TraitData traitData = TraitData.Get(atrait.id);
                if (traitData != null && !string.IsNullOrEmpty(traitData.title))
                {
                    int ival = atrait.value;
                    string sval = ival > 1 ? " " + ival : "";
                    txt += traitData.GetTitle() + sval + ", ";
                }
            }
            if (txt.Length > 2)
                txt = txt.Substring(0, txt.Length - 2);
            return txt;
        }

        public bool IsDead()
        {
            return destroyed;
        }
        
        public bool IsFocus()
        {
            return focus;
        }

        public bool IsEquipFocus()
        {
            return equipment != null && equipment.IsFocus();
        }

        public void OnMouseEnter()
        {
            if (GameUI.IsUIOpened())
                return;
            if (GameTool.IsMobile())
                return;
            focus = true;
            ShowStatusBar();
        }

        public void OnMouseExit()
        {
            focus = false;
            statusAlphaTarget = 0f;
        }

        public void OnMouseDown()
        {
            if (GameUI.IsOverUILayer("UI"))
                return;
            PlayerControls.Get().SelectCard(this);
            if (GameTool.IsMobile())
            {
                focus = true;
                ShowStatusBar();
            }
        }
        
        public void OnMouseUp()
        {

        }
        
        public void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(1))
            {
                PlayerControls.Get().SelectCardRight(this);
            }
        }

        public string GetCardUID()
        {
            return cardUid;
        }

        public Card GetCard()
        {
            Game data = Gameclient.Get().GetGameData();
            Card card = data.GetCard(cardUid);
            return card;
        }

        public Card GetEquipCard()
        {
            Game data = Gameclient.Get().GetGameData();
            Card card = GetCard();
            Card equip = data?.GetEquipCard(card.equippedUid);
            return equip;
        }
        
        public Card GetFocusCard()
        {
            if (IsEquipFocus())
                return GetEquipCard();
            return GetCard();
        }

        public CardData GetCardData()
        {
            Card card = GetCard();
            return card?.Data;
        }

        public Slot GetSlot()
        {
            return GetCard().slot;
        }

        public BoardCardFX GetCardFX()
        {
            return cardFX;
        }
        
        public CardData CardData => GetCardData();

        public static int GetNbCardsBoardPlayer(int playerId)
        {
            int nb = 0;
            foreach (BoardCard card in cardList)
            {
                if (card != null && card.GetCard().playerID == playerId)
                {
                    nb++;
                }
            }
            return nb;
        }

        public static BoardCard GetNearestPlayer(Vector3 pos, int skipPlayerID, BoardCard skip, float range = 2f)
        {
            BoardCard nearest = null;
            float minDist = range;
            foreach (BoardCard card in cardList)
            {
                float dist = (card.transform.position - pos).magnitude;
                if (dist < minDist && card != skip && skipPlayerID != card.GetCard().playerID)
                {
                    minDist = dist;
                    nearest = card;
                }
            }
            return nearest;
        }

        public static BoardCard GetNearest(Vector3 pos, BoardCard skip, float range = 2f)
        {
            BoardCard nearest = null;
            float minDist = range;
            foreach (BoardCard card in cardList)
            {
                float dist = (card.transform.position - pos).magnitude;
                if (dist < minDist && card != skip)
                {
                    minDist = dist;
                    nearest = card;
                }
            }
            return nearest;
        }

        public static BoardCard GetFocus()
        {
            if(GameUI.IsUIOpened())
                return null;
            foreach (BoardCard card in cardList)
            {
                if (card.IsFocus() || card.IsEquipFocus())
                    return card;
            }
            return null;
        }

        public static void UnfocusAll()
        {
            foreach (BoardCard card in cardList)
            {
                card.focus = false;
                card.statusAlphaTarget = 0f;
            }
        }

        public static BoardCard Get(string uid)
        {
            return cardList.FirstOrDefault(x => x.GetCardUID() == uid);
        }
        
        public static List<BoardCard> GetAll()
        {
            return cardList;
        }
    }
}