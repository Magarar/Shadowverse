using System;
using Data;
using Effects;
using GameClient;
using GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ExtraManaToggle:MonoBehaviour
    {
        public Toggle extraManaToggle;
        public GameObject bg;
        
        public AbilityData addExtraMana;
        public AbilityData removeExtraMana;

        public TextMeshProUGUI canUseText;
        
        private static ExtraManaToggle instance;

        private void Awake()
        {
            instance = this;
            
            extraManaToggle = GetComponent<Toggle>();
            extraManaToggle.onValueChanged.AddListener(OnToggle);
            
        }

        private void Update()
        {
            if(!gameObject.activeSelf)
                return;
            if(!Gameclient.Get().IsReady())
                return;
            Game gdata = Gameclient.Get().GetGameData();
            Player player = Gameclient.Get().GetPlayer();
            Card hero = player.hero;
            
            bg.SetActive(extraManaToggle.isOn);
            
            bool yourTurn = gdata.IsPlayerActionTurn(player);
            extraManaToggle.interactable = player.canUseExtraMana&&yourTurn;
            if (!yourTurn)
                extraManaToggle.isOn = false;

            if (canUseText != null)
            {
                if (5-gdata.turnCount>0)
                {
                    canUseText.text = $"{(5-gdata.turnCount)}回合后可使用";
                }
                else
                {
                    canUseText.text = "";
                }
            }
            
        }

        private void OnToggle(bool value)
        {
            Game gdata = Gameclient.Get().GetGameData();
            Player player = Gameclient.Get().GetPlayer();
            Card hero = player.hero;
            if(!gdata.IsPlayerActionTurn(player))
                return;
            Debug.Log($"extraManaToggle:{player.canUseExtraMana}");
            if (!player.canUseExtraMana)
                return;
            var ability = value ? addExtraMana : removeExtraMana;
            if(ability==null)
                return;
            
            AddExtraMana(value, ability);
        }

        private void AddExtraMana(bool value, AbilityData ability)
        {
            Game gdata = Gameclient.Get().GetGameData();
            Player player = Gameclient.Get().GetPlayer();
            Card hero = player.hero;
            
            bool valid = gdata.IsPlayerActionTurn(player);
            
            if (!valid)
                return;
            Debug.Log($"hero:{hero.cardId}");

            Gameclient.Get().CastAbility(hero, ability);
        }

        public void SetEnable(bool enable)
        {
            gameObject.SetActive(enable);
        }
        

        public static ExtraManaToggle Get()
        {
            return instance;
        }
    }
}