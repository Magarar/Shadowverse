using GameLogic;
using UI;
using UnityEngine;

namespace GameClient
{
    /// <summary>
    /// Script that contain main controls for clicking on cards, attacking, activating abilities
    /// Holds the currently selected card and will send action to GameClient on click release
    /// </summary>
    public class PlayerControls:MonoBehaviour
    {
        private BoardCard selectedCard = null;
        private BoardEvolutionary selectedEvolutionary = null;
        
        private static PlayerControls instance;

        private void Awake()
        {
            instance = this;
        }

        void Update()
        {
            if(!Gameclient.Get().IsReady())
                return;
            
            if (Input.GetMouseButtonDown(1))
                UnselectAll();

            if (selectedCard != null|| selectedEvolutionary != null)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    ReleaseClick();
                    UnselectAll();
                }
            }
        }

        public void SelectCard(BoardCard bcard)
        {
            Game gdata = Gameclient.Get().GetGameData();
            Player player = Gameclient.Get().GetPlayer();
            Card card = bcard.GetFocusCard();

            if (gdata.IsPlayerSelectorTurn(player) && gdata.selector == SelectorType.SelectTarget)
            {
                if (!Tutorial.Get().CanDo(TutoEndTrigger.SelectTarget, card))
                    return;
                
                //Target selector, select this card
                Gameclient.Get().SelectCard(card);
            }else if (gdata.IsPlayerActionTurn(player) && card.playerID == player.id)
            {
                //Start dragging card
                selectedCard = bcard;
            }
        }

        public void SelectEvolutionary(BoardEvolutionary bevolutionary)
        {
            Game gdata = Gameclient.Get().GetGameData();
            Player player = Gameclient.Get().GetPlayer();
            EvolutionaryType type = bevolutionary.GetEvolutionary();

            if (gdata.IsPlayerActionTurn(player)&&player.id==gdata.currentPlayer)
            {
                selectedEvolutionary = bevolutionary;
            }
        }
        
        public void SelectCardRight(BoardCard card)
        {
            if (!Input.GetMouseButton(0))
            {
                //Nothing on right-click
            }
        }

        private void ReleaseClick()
        {
            bool yourTurn = Gameclient.Get().IsYourTurn();
            if (yourTurn && selectedCard != null)
            {
                Card card = selectedCard.GetCard();
                Vector3 wpos = GameBoard.Get().RaycastMouseBoard();
                BSlot tslot = BSlot.GetNearest(wpos);
                Card target = tslot?.GetSlotCard(wpos);
                AbilityButton ability = AbilityButton.GetFocus(wpos, 1f);

                if (ability != null && ability.IsInteractable())
                {
                    if (!Tutorial.Get().CanDo(TutoEndTrigger.CastAbility, card))
                        return;

                    Gameclient.Get().CastAbility(card, ability.GetAbility());
                }
                
                else if (tslot is BoardSlotPlayer)
                {
                    if (!Tutorial.Get().CanDo(TutoEndTrigger.AttackPlayer, card))
                        return;
                    if (card.exhausted)
                        WarningText.ShowExhausted();
                    else
                        Gameclient.Get().AttackPlayer(card, tslot.GetPlayer());
                } else if (target != null && target.uid != card.uid && target.playerID != card.playerID)
                {
                    if (!Tutorial.Get().CanDo(TutoEndTrigger.Attack, card) && !Tutorial.Get().CanDo(TutoEndTrigger.Attack, target))
                        return;
                    if (card.exhausted)
                        WarningText.ShowExhausted();
                    else
                        Gameclient.Get().AttackTarget(card, target);
                }
                else if (tslot != null && tslot is BoardSlot)
                {
                    if (!Tutorial.Get().CanDo(TutoEndTrigger.Move, tslot.GetSlot()))
                        return;

                    Gameclient.Get().Move(card, tslot.GetSlot());
                }
            }

            if (yourTurn && selectedEvolutionary != null)
            {
                EvolutionaryType type = selectedEvolutionary.GetEvolutionary();
                Vector3 wpos = GameBoard.Get().RaycastMouseBoard();
                BSlot tslot = BSlot.GetNearest(wpos);
                Card target = tslot?.GetSlotCard(wpos);
                
                if (target != null&& target.playerID == Gameclient.Get().GetPlayerID()&&target.CanEvolve())
                {
                    Player player = Gameclient.Get().GetPlayer();
                    if (player.CanUseEvolution())
                    {
                        switch (type)
                        {
                            case EvolutionaryType.Normal:
                                Gameclient.Get().EvolveCard(target,true);
                                break;
                            case EvolutionaryType.Super:
                                Gameclient.Get().SuperEvolveCard(target,true);
                                break;
                        }
                    }
                    else
                    {
                        WarningText.ShowCantEvolutionary();
                    }
                }
            }
        }
        
        public void UnselectAll()
        {
            selectedCard = null;
            selectedEvolutionary = null;
        }
        
        public BoardCard GetSelected()
        {
            return selectedCard;
        }

        public BoardEvolutionary GetSelectedEvolutionary()
        {
            return selectedEvolutionary;
        }

        public static PlayerControls Get()
        {
            return instance;
        }

        
        
        
    }
}