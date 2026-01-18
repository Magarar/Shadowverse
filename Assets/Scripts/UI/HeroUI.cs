using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using GameClient;
using GameLogic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class HeroUI: MonoBehaviour
    {
        public bool opponent;
        public GameObject powerArea;
        public Button powerButton;
        public Image powerImage;
        public GameObject powerManaSlot;
        public Text powerMana;
        
        public Material activeMat;
        public Material inactiveMat;
        
        private bool focus = false;

        private static List<HeroUI> uiList = new List<HeroUI>();

        private void Awake()
        {
            uiList.Add(this);
        }

        private void OnDestroy()
        {
            uiList.Remove(this);
        }

        private void Start()
        {
            powerArea.SetActive(false);
            if (powerButton != null)
            {
                powerButton.onClick.AddListener(OnClickPower);
            }
            
            EventTrigger trigger = powerArea.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) => { OnEnterMouse(); });
            EventTrigger.Entry exit = new EventTrigger.Entry();
            exit.eventID = EventTriggerType.PointerExit;
            exit.callback.AddListener((eventData) => { OnExitMouse(); });
            trigger.triggers.Add(entry);
            trigger.triggers.Add(exit);
        }

        private void Update()
        {
            if (!Gameclient.Get().IsReady())
                return;
            
            Game gdata = Gameclient.Get().GetGameData();
            Player player = GetPlayer();
            Card hero = player.hero;
            if (hero == null)
                return;
            
            AbilityData ability = hero.GetAbility(AbilityTrigger.Activate);
            if (ability != null)
            {
                powerImage.sprite = hero.CardData.GetBoardArt(hero.VariantData);
                powerImage.material = !hero.exhausted?activeMat:inactiveMat;
                powerManaSlot?.SetActive(gdata.IsPlayerTurn(player)&&!hero.exhausted);
                powerMana.text = ability.manaCost.ToString();
            }

            if (powerButton != null)
            {
                powerButton.interactable = ability!=null&&gdata.IsPlayerTurn(player)&&!hero.exhausted;
            }
            
            if(hero!=null&&!powerArea.activeSelf)
                powerArea.SetActive(true);
        }

        public void OnClickPower()
        {
            Game gdata = Gameclient.Get().GetGameData();
            Player player = Gameclient.Get().GetPlayer();
            Card hero = player.hero;
            AbilityData ability = hero?.GetAbility(AbilityTrigger.Activate);

            if (ability != null && !opponent)
            {
                if (!hero.exhausted && !player.CanPayAbility(hero, ability))
                {
                    WarningText.ShowNoMana();
                    return;
                }
                
                if (!Tutorial.Get().CanDo(TutoEndTrigger.CastAbility, hero))
                    return;
                
                bool valid = gdata.IsPlayerActionTurn(player) && gdata.CanCastAbility(hero, ability);
                if (valid)
                {
                    Gameclient.Get().CastAbility(hero, ability);
                }
            }
        }

        private void OnEnterMouse()
        {
            focus = true;
        }
        
        private void OnExitMouse()
        {
            focus = false;
        }
        
        private void OnDisable()
        {
            focus = false;
        }
        
        public bool IsFocus()
        {
            return focus;
        }
        
        public int GetPlayerID()
        {
            return opponent ? Gameclient.Get().GetOpponentPlayerID() : Gameclient.Get().GetPlayerID();
        }
        
        private Player GetPlayer()
        {
            Game gdata = Gameclient.Get().GetGameData();
            return gdata.GetPlayer(GetPlayerID());
        }
        
        public Card GetCard()
        {
            Player player = GetPlayer();
            return player.hero;
        }

        public static HeroUI GetFocus()
        {
            return uiList.FirstOrDefault(ui => ui.IsFocus());
        }

        public static HeroUI Get(bool opponent)
        {
            return uiList.FirstOrDefault(ui => ui.opponent == opponent);
        }
        
        public static HeroUI Get(int playerID)
        {
            bool opponent = playerID != Gameclient.Get().GetPlayerID();
            return Get(opponent);
        }
       

  

       

       
    }
}