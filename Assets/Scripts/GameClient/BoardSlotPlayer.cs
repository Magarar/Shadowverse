using System.Collections.Generic;
using Data;
using FX;
using GameLogic;
using UI;
using Unit;
using UnityEngine;

namespace GameClient
{
    /// <summary>
    /// Visual zone that can be attacked by opponent's card to damage the player HP
    /// </summary>
    public class BoardSlotPlayer:BSlot
    {
        public bool opponent;
        
        public float rangeX = 3f;
        public float rangeY = 1f;
        
        private static BoardSlotPlayer instanceSelf;
        private static BoardSlotPlayer instanceOpponent;
        
        private static List<BoardSlotPlayer> zoneList = new List<BoardSlotPlayer>();

        protected override void Awake()
        {
            base.Awake();
            zoneList.Add(this);
            if (opponent)
                instanceOpponent = this;
            else
                instanceSelf = this;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            zoneList.Remove(this);
        }

        private void Start()
        {
            Gameclient.Get().onPlayerDamaged += OnPlayerDamaged;
            Gameclient.Get().onAbilityStart += OnAbilityStart;
            Gameclient.Get().onAbilityTargetPlayer += OnAbilityEffect;
        }

        protected override void Update()
        {
            base.Update();
            
            if(!Gameclient.Get().IsReady())
                return;
            
            if(!opponent)
                return;
            
            BoardCard bcardSelected = PlayerControls.Get().GetSelected();
            HandCard dragCard = HandCard.GetDrag();
            bool yourTurn = Gameclient.Get().IsYourTurn();
            
            Game gdata = Gameclient.Get().GetGameData();
            Player player = Gameclient.Get().GetPlayer();
            Player opponentPlayer = Gameclient.Get().GetOpponentPlayer();
            
            targetAlpha = 0f;
            Card selectedCard = bcardSelected?.GetCard();
            if (selectedCard != null)
            {
                bool canAttack = gdata.IsPlayerActionTurn(player)&&selectedCard.CanAttack();
                bool canBeAttacked = gdata.CanAttackTarget(selectedCard, opponentPlayer);
                
                if(canAttack&&canBeAttacked)
                    targetAlpha = 1f;
            }

            if (yourTurn && dragCard != null && dragCard.CardData.IsRequireTargetSpell() &&
                gdata.IsPlayTargetValid(dragCard.GetCard(), GetPlayer()))
            {
                targetAlpha = 1f;
            }

            if (gdata.selector == SelectorType.SelectTarget && player.id == gdata.selectorPlayerId)
            {
                Card caster = gdata.GetCard(gdata.selectorCasterUid);
                AbilityData ability = AbilityData.Get(gdata.selectorAbilityId);
                if (ability != null && ability.CanTarget(gdata, caster, GetPlayer()))
                    targetAlpha = 1f;
            }
        }
        
        private void OnAbilityStart(AbilityData ability, Card caster)
        {
            if (ability != null && caster != null)
            {
                int playerId = opponent ? Gameclient.Get().GetOpponentPlayerID() : Gameclient.Get().GetPlayerID();
                if (caster.CardData.type == CardType.Spell && caster.playerID == playerId)
                {
                    FXTool.DoFX(ability.casterFX, transform.position);
                }
            }
        }

        private void OnAbilityEffect(AbilityData ability, Card caster, Player target)
        {
            if (ability != null && caster != null && target != null)
            {
                int playerId = opponent ? Gameclient.Get().GetOpponentPlayerID() : Gameclient.Get().GetPlayerID();
                if (target.id == playerId)
                {
                    FXTool.DoFX(ability.targetFX, transform.position);
                    FXTool.DoProjectileFX(ability.projectileFX, GetFXSource(caster),transform,ability.GetDamage());
                    AudioTool.Get().PlaySFX("sfx", ability.targetAudio);
                }
            }
        }

        private void OnPlayerDamaged(Player target, int damage)
        {
            if (GetPlayerID() == target.id && damage > 0)
            {
                DamageFX(transform, damage);
            }
        }

        private void DamageFX(Transform target, int value,float delay = 0.5f)
        {
            TimeTool.WaitFor(delay, () =>
            {
                GameObject fx = FXTool.DoFX(AssetData.Get().damageFX, target.position);
                fx.GetComponent<DamageFX>().SetValue(value);
            });
        }
        
        private Transform GetFXSource(Card caster)
        {
            if (caster.CardData.IsBoardCard())
            {
                BoardCard bcard = BoardCard.Get(caster.uid);
                if (bcard != null)
                    return bcard.transform;                
            }
            else
            {
                BoardSlotPlayer slot = Get(caster.playerID);
                if (slot != null)
                    return slot.transform;
            }

            return null;
        }

        public void OnMouseDown()
        {
            if (GameUI.IsUIOpened() || GameUI.IsOverUILayer("UI"))
                return;
            
            Game gdata = Gameclient.Get().GetGameData();
            int playerId = Gameclient.Get().GetPlayerID();
            if (gdata.selector == SelectorType.SelectTarget && playerId == gdata.selectorPlayerId)
            {
                Gameclient.Get().SelectPlayer(GetPlayer());
            }
        }

        public int GetPlayerID()
        {
            return opponent ? Gameclient.Get().GetOpponentPlayerID() : Gameclient.Get().GetPlayerID();
        }

        public override Player GetPlayer()
        {
            return opponent ? Gameclient.Get().GetOpponentPlayer() : Gameclient.Get().GetPlayer();
        }

        public override Slot GetSlot()
        {
            return new Slot(GetPlayerID());
        }

        public static BoardSlotPlayer Get(bool opponent)
        {
            return opponent ? instanceOpponent : instanceSelf;
        }
        
        public static BoardSlotPlayer Get(int playerId)
        {
           bool opponentPlayer = playerId != Gameclient.Get().GetPlayerID();
           return Get(opponentPlayer);
        }

        

        
        
        
    }
}