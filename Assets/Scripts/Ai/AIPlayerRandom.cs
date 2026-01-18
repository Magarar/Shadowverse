using System.Collections;
using System.Collections.Generic;
using Data;
using GameLogic;
using Unit;
using UnityEngine;

namespace Ai
{
    /// <summary>
    /// AI player making completely random decisions, really bad AI but useful for testing
    /// </summary>
    public class AIPlayerRandom: AIPlayer
    {
        private bool isPlaying = false;
        private bool isSelecting = false;
        
        private System.Random rand = new System.Random();

        
        public AIPlayerRandom(Gamelogic gameplay, int id, int level)
        {
            this.gamePlay = gameplay;
            playerID = id;
        }

        public override void Update()
        {
            if (!CanPlay())
                return;
            
            Game gameData = gamePlay.GetGameData();
            Player player = gameData.GetPlayer(playerID);

            if (gameData.IsPlayerTurn(player) && !gamePlay.IsResolving())
            {
                if(!isPlaying && gameData.selector == SelectorType.None && gameData.currentPlayer == playerID)
                {
                    isPlaying = true;
                    TimeTool.StartCoroutine(AiTurn());
                }
            }
            
            if (!isSelecting && gameData.selector != SelectorType.None && gameData.selectorPlayerId == playerID)
            {
                if (gameData.selector == SelectorType.SelectTarget)
                {
                    //AI select target
                    isSelecting = true;
                    TimeTool.StartCoroutine(AiSelectTarget());
                }

                if (gameData.selector == SelectorType.SelectorCard)
                {
                    //AI select target
                    isSelecting = true;
                    TimeTool.StartCoroutine(AiSelectCard());
                }

                if (gameData.selector == SelectorType.SelectorChoice)
                {
                    //AI select target
                    isSelecting = true;
                    TimeTool.StartCoroutine(AiSelectChoice());
                }

                if (gameData.selector == SelectorType.SelectorCost)
                {
                    //AI select target
                    isSelecting = true;
                    TimeTool.StartCoroutine(AiSelectCost());
                }
            }
            
            if (!isSelecting && gameData.IsPlayerMulliganTurn(player))
            {
                isSelecting = true;
                TimeTool.StartCoroutine(AiSelectMulligan());
            }
        }
        
        private IEnumerator AiTurn()
        {
            yield return new WaitForSeconds(1f);

            PlayCard();

            yield return new WaitForSeconds(0.5f);

            PlayCard();

            yield return new WaitForSeconds(0.5f);

            PlayCard();

            yield return new WaitForSeconds(0.5f);

            Attack();

            yield return new WaitForSeconds(0.5f);

            Attack();

            yield return new WaitForSeconds(0.5f);

            AttackPlayer();

            yield return new WaitForSeconds(0.5f);

            EndTurn();

            isPlaying = false;
        }
        
        private IEnumerator AiSelectCard()
        {
            yield return new WaitForSeconds(0.5f);

            SelectCard();

            yield return new WaitForSeconds(0.5f);

            CancelSelect();
            isSelecting = false;
        }
        
        private IEnumerator AiSelectTarget()
        {
            yield return new WaitForSeconds(0.5f);

            SelectTarget();

            yield return new WaitForSeconds(0.5f);

            CancelSelect();
            isSelecting = false;
        }
        
        private IEnumerator AiSelectChoice()
        {
            yield return new WaitForSeconds(0.5f);

            SelectChoice();

            yield return new WaitForSeconds(0.5f);

            CancelSelect();
            isSelecting = false;
        }
        
        private IEnumerator AiSelectCost()
        {
            yield return new WaitForSeconds(0.5f);

            SelectCost();

            yield return new WaitForSeconds(0.5f);

            CancelSelect();
            isSelecting = false;
        }
        
        private IEnumerator AiSelectMulligan()
        {
            yield return new WaitForSeconds(0.5f);

            SelectMulligan();

            yield return new WaitForSeconds(0.5f);
            isSelecting = false;
        }
        
        public void PlayCard()
        {
            if (!CanPlay())
                return;

            Game gameData = gamePlay.GetGameData();
            Player player = gameData.GetPlayer(playerID);
            if (player.cardsHand.Count > 0 && gameData.IsPlayerActionTurn(player))
            {
                Card random = player.GetRandomCard(player.cardsHand, rand);
                Slot slot = player.GetRandomEmptySlot(rand);

                if (random != null && random.CardData.IsRequireTargetSpell())
                    slot = gameData.GetRandomSlot(rand); //Spell can target any slot, not just your side

                if(random != null && random.CardData.IsEquipment())
                    slot = player.GetRandomOccupiedSlot(rand);

                if (random != null)
                    gamePlay.PlayCard(random, slot);
            }
        }
        
        public void Attack()
        {
            if (!CanPlay())
                return;

            Game gameData = gamePlay.GetGameData();
            Player player = gameData.GetPlayer(playerID);
            if (player.cardsBoard.Count > 0 && gameData.IsPlayerActionTurn(player))
            {
                Card random = player.GetRandomCard(player.cardsBoard, rand);
                Card rtarget = gameData.GetRandomBoardCard(rand);
                if (random != null && rtarget != null)
                    gamePlay.AttackTarget(random, rtarget);
            }
        }
        
        public void AttackPlayer()
        {
            if (!CanPlay())
                return;

            Game gameData = gamePlay.GetGameData();
            Player player = gameData.GetPlayer(playerID);
            Player oplayer = gameData.GetRandomPlayer(rand);
            if (player.cardsBoard.Count > 0 && gameData.IsPlayerActionTurn(player))
            {
                Card random = player.GetRandomCard(player.cardsBoard, rand);
                if (random != null && oplayer != null && oplayer != player)
                    gamePlay.AttackPlayer(random, oplayer);
            }
        }
        
        public void SelectCard()
        {
            if (!CanPlay())
                return;

            Game gameData = gamePlay.GetGameData();
            Player player = gameData.GetPlayer(playerID);
            AbilityData ability = AbilityData.Get(gameData.selectorAbilityId);
            Card caster = gameData.GetCard(gameData.selectorCasterUid);
            if (player != null && ability != null && caster != null)
            {
                List<Card> card_list = ability.GetCardTargets(gameData, caster);
                if (card_list.Count > 0)
                {
                    Card card = card_list[rand.Next(0, card_list.Count)];
                    gamePlay.SelectCard(card);
                }
            }
        }
        
        public void SelectTarget()
        {
            if (!CanPlay())
                return;

            Game gameData = gamePlay.GetGameData();
            if (gameData.selector != SelectorType.None)
            {
                int targetPlayer = playerID;
                AbilityData ability = AbilityData.Get(gameData.selectorAbilityId);
                if (ability != null && ability.target == AbilityTarget.SelectTarget)
                    targetPlayer = (playerID == 0 ? 1 : 0);

                Player tplayer = gameData.GetPlayer(targetPlayer);
                if (tplayer.cardsBoard.Count > 0)
                {
                    Card random = tplayer.GetRandomCard(tplayer.cardsBoard, rand);
                    if (random != null)
                        gamePlay.SelectCard(random);
                }
            }
        }
        
        public void SelectChoice()
        {
            if (!CanPlay())
                return;

            Game gameData = gamePlay.GetGameData();
            if (gameData.selector != SelectorType.None)
            {
                AbilityData ability = AbilityData.Get(gameData.selectorAbilityId);
                if (ability != null && ability.chainAbilities.Length > 0)
                {
                    int choice = rand.Next(0, ability.chainAbilities.Length);
                    gamePlay.SelectChoice(choice);
                }
            }
        }
        
        public void SelectCost()
        {
            if (!CanPlay())
                return;

            Game gameData = gamePlay.GetGameData();
            if (gameData.selector != SelectorType.None)
            {
                Player player = gameData.GetPlayer(playerID);
                Card card = gameData.GetCard(gameData.selectorCasterUid);
                if (player != null && card != null)
                {
                    int max = Mathf.Clamp(player.Mana, 0, 9);
                    int choice = rand.Next(0, max + 1);
                    gamePlay.SelectCost(choice);
                }
            }
        }
        
        public void CancelSelect()
        {
            if (CanPlay())
            {
                gamePlay.CancelSelection();
            }
        }
        
        public void SelectMulligan()
        {
            if (!CanPlay())
                return;

            Game gameData = gamePlay.GetGameData();
            if (gameData.phase == GamePhase.Mulligan)
            {
                Player player = gameData.GetPlayer(playerID);
                string[] cards = new string[0]; //Don't mulligan
                gamePlay.Mulligan(player, cards);
            }
        }
        
        public void EndTurn()
        {
            if (CanPlay())
            {
                gamePlay.EndTurn();
            }
        }
    }
}