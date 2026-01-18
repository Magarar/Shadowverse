using System.Collections.Generic;
using System.Linq;
using Data;
using GameClient;
using GameLogic;
using TMPro;
using Unit;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Main player UI inside the GameUI, inside the game scene
    /// there is one for each player
    /// </summary>
    public class PlayerUI: MonoBehaviour
    {
        public bool isOpponent;
   //     public TextMeshProUGUI pname;
        public AvatarUI avatar;
        public IconBar manaBar;
        public TextMeshProUGUI hpText;
    //    public TextMeshProUGUI hpMaxText;
        public TextMeshProUGUI manaText;

        public TextMeshProUGUI cardsHandCount;
        public TextMeshProUGUI cardsDeckCount;
        
        public Animator[] secrets;
        
        public GameObject deadFx;
        public AudioClip deadAudio;
        public Sprite avatarDead;
        
        public BoardArticleArea boardArticleArea;
        
        private bool killed = false;
        private float timer = 0f;
        
        private int prevHp = 0;
        private float delayedDamageTimer = 0f;
        
        private static List<PlayerUI> uiList = new List<PlayerUI>();

        private void Awake()
        {
            uiList.Add(this);
        }

        private void OnDestroy()
        {
            uiList.Remove(this);
        }

        void Start()
        {
          //  pname.text = "";
            hpText.text = "";
        //    hpMaxText.text = "";
            
            for (int i = 0; i < secrets.Length; i++)
                secrets[i].gameObject.SetActive(false);
            
            avatar.onClick += OnClickAvatar;
            Gameclient.Get().onSecretTrigger += OnSecretTrigger;
        }

        void Update()
        {
            if(!Gameclient.Get().IsReady())
                return;
            
            Player player = GetPlayer();

            if (player != null)
            {
                //    pname.text = player.username;
                manaBar.value = player.Mana;
                manaBar.maxValue = player.manaMax;

                string mana = $"{player.mana}";
                if (player.manaOngoing > 0)
                {
                    mana += $"+<color=#FFA500>{player.manaOngoing}</color>";
                }
                mana+= $"/{player.manaMax}";
                manaText.text = mana;
                
                hpText.text = prevHp.ToString();
                //     hpMaxText.text = "/" + player.hpMax.ToString();

                Card adata = player.hero;
                if (avatar != null && adata != null && !killed)
                {
                    avatar.SetAvatar(adata.CardData.artFull);
                }
                
                boardArticleArea?.UpdateBoardArticles(player);
                
                if(cardsHandCount != null)
                    cardsHandCount.text = player.cardsHand.Count.ToString();
                if(cardsDeckCount != null)
                    cardsDeckCount.text = player.cardsDeck.Count.ToString();
                
                delayedDamageTimer -= Time.deltaTime;
                if (!IsDamagedDelayed())
                    prevHp = player.hp;
                    
            }
            
            UpdateCardsCount();
            
            timer += Time.deltaTime;
            if (timer > 0.4f)
            {
                timer = 0f;
                SlowUpdate();
            }
        }

        private void UpdateCardsCount()
        {
            
        }

        private void SlowUpdate()
        {
            Player player = GetPlayer();
            if (player == null)
                return;

            for (int i = 0; i < secrets.Length; i++)
            {
                bool active = i < player.cardsSecret.Count;
                bool wasActive = secrets[i].gameObject.activeSelf;
                if (active != wasActive)
                    secrets[i].gameObject.SetActive(active);
                if(active&&!wasActive)
                    secrets[i].SetTrigger("appear");
                if (active && !wasActive && !isOpponent)
                    secrets[i].GetComponent<SecretIconUI>().SetCard(player.cardsSecret[i]);
                if(!active&&wasActive)
                    secrets[i].Rebind();
            }
        }

        public void Kill()
        {
            killed = true;
            avatar.SetImage(avatarDead);
            AudioTool.Get().PlaySFX("fx", deadAudio);
            FXTool.DoFX(deadFx, avatar.transform.position);
        }

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
        
        private void OnClickAvatar(AvatarData avatar)
        {
            Game gdata = Gameclient.Get().GetGameData();
            int playerID = Gameclient.Get().GetPlayerID();
            if (gdata.selector == SelectorType.SelectTarget && playerID == gdata.selectorPlayerId)
            {
                Gameclient.Get().SelectPlayer(GetPlayer());
            }
        }
        
        private void OnSecretTrigger(Card secret, Card triggerer)
        {
            Player player = GetPlayer();
            int index = player.cardsSecret.Count - 1;
            if (player.id == secret.playerID && index >= 0 && index < secrets.Length)
            {
                secrets[index].SetTrigger("reveal");
            }
        }

        private Player GetPlayer()
        {
            int playerID = isOpponent ? Gameclient.Get().GetOpponentPlayerID() : Gameclient.Get().GetPlayerID();
            Game data = Gameclient.Get().GetGameData();
            return data.GetPlayer(playerID);
        }

        public static PlayerUI Get(bool opponent)
        {
            return uiList.FirstOrDefault(ui => ui.isOpponent == opponent);
        }

    
    }
}