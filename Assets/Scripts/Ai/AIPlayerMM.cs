using System.Collections;
using Data;
using GameLogic;
using Unit;
using UnityEngine;

namespace Ai
{
    public class AIPlayerMM: AIPlayer
    {
        private AILogic aiLogic;
        
        private bool isPlaying = false;
        
        public AIPlayerMM(Gamelogic gamelogic, int id, int level)
        {
            gamePlay = gamelogic;
            playerID = id;
            aiLevel = level;
            aiLogic = AILogic.Create(id, level);
        }

        public override void Update()
        {
            Game gameData = gamePlay.GetGameData();
            Player player = gameData.GetPlayer(playerID);
            
            if (!isPlaying && gameData.IsPlayerTurn(player))
            {
                isPlaying = true;
                TimeTool.StartCoroutine(AiTurn());
            }

            if (!isPlaying && gameData.IsPlayerMulliganTurn(player))
            {
                SkipMulligan();
            }

            if (!gameData.IsPlayerTurn(player) && aiLogic.IsRunning())
                Stop();
        }

        private IEnumerator AiTurn()
        {
            yield return new WaitForSeconds(1f);
            Game gameData = gamePlay.GetGameData();
            aiLogic.RunAI(gameData);

            while (aiLogic.IsRunning())
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            AIAction best = aiLogic.GetBestAction();
            if (best != null)
            {
                Debug.Log("Execute AI Action: " + best.GetText(gameData) + "\n" + aiLogic.GetNodePath());
                ExecuteAction(best);
            }
            
            aiLogic.ClearMemory();
            yield return new WaitForSeconds(0.5f);
            isPlaying = false;
        }
        
        private void Stop()
        {
            aiLogic.Stop();
            isPlaying = false;
        }

        private void ExecuteAction(AIAction action)
        {
            if (!CanPlay())
                return;

            if (action.type == GameAction.PlayCard)
            {
                PlayCard(action.cardUID, action.slot);
            }

            if (action.type == GameAction.Attack)
            {
                AttackCard(action.cardUID, action.targetUID);
            }

            if (action.type == GameAction.AttackPlayer)
            {
                AttackPlayer(action.cardUID, action.targetPlayerId);
            }

            if (action.type == GameAction.Move)
            {
                MoveCard(action.cardUID, action.slot);
            }

            if (action.type == GameAction.CastAbility)
            {
                CastAbility(action.cardUID, action.abilityID);
            }

            if (action.type == GameAction.SelectCard)
            {
                SelectCard(action.targetUID);
            }

            if (action.type == GameAction.SelectPlayer)
            {
                SelectPlayer(action.targetPlayerId);
            }

            if (action.type == GameAction.SelectSlot)
            {
                SelectSlot(action.slot);
            }

            if (action.type == GameAction.SelectChoice)
            {
                SelectChoice(action.value);
            }

            if (action.type == GameAction.SelectCost)
            {
                SelectCost(action.value);
            }

            if (action.type == GameAction.SelectMulligan)
            {
                SkipMulligan();
            }

            if (action.type == GameAction.CancelSelect)
            {
                CancelSelect();
            }

            if (action.type == GameAction.EndTurn)
            {
                EndTurn();
            }

            if (action.type == GameAction.Resign)
            {
                Resign();
            }
        }
        
        private void PlayCard(string cardUid, Slot slot)
        {
            Game gameData = gamePlay.GetGameData();
            Card card = gameData.GetCard(cardUid);
            if (card != null)
            {
                gamePlay.PlayCard(card, slot);
            }
        }
        
        private void MoveCard(string cardUid, Slot slot)
        {
            Game gameData = gamePlay.GetGameData();
            Card card = gameData.GetCard(cardUid);
            if (card != null)
            {
                gamePlay.MoveCard(card, slot); 
            }
        }
        
        private void AttackCard(string attackerUid, string targetUid)
        {
            Game gameData = gamePlay.GetGameData();
            Card card = gameData.GetCard(attackerUid);
            Card target = gameData.GetCard(targetUid);
            if (card != null && target != null)
            {
                gamePlay.AttackTarget(card, target);
            }
        }
        
        private void AttackPlayer(string attackerUid, int targetPlayerID)
        {
            Game gameData = gamePlay.GetGameData();
            Card card = gameData.GetCard(attackerUid);
            if (card != null)
            {
                Player oplayer = gameData.GetPlayer(targetPlayerID);
                gamePlay.AttackPlayer(card, oplayer);
            }
        }

        private void CastAbility(string casterUid, string abilityID)
        {
            Game gameData = gamePlay.GetGameData();
            Card caster = gameData.GetCard(casterUid);
            AbilityData iability = AbilityData.Get(abilityID);
            if (caster != null && iability != null)
            {
                gamePlay.CastAbility(caster, iability);
            }
        }
        
        private void SelectCard(string targetUid)
        {
            Game gameData = gamePlay.GetGameData();
            Card target = gameData.GetCard(targetUid);
            if (target != null)
            {
                gamePlay.SelectCard(target);
            }
        }
        
        private void SelectPlayer(int tplayerID)
        {
            Game gameData = gamePlay.GetGameData();
            Player target = gameData.GetPlayer(tplayerID);
            if (target != null)
            {
                gamePlay.SelectPlayer(target);
            }
        }
        
        private void SelectSlot(Slot slot)
        {
            if (slot != Slot.None)
            {
                gamePlay.SelectSlot(slot);
            }
        }

        private void SelectChoice(int choice)
        {
            gamePlay.SelectChoice(choice);
        }

        private void SelectCost(int cost)
        {
            gamePlay.SelectCost(cost);
        }

        private void CancelSelect()
        {
            if (CanPlay())
            {
                gamePlay.CancelSelection();
            }
        }
        
        private void SkipMulligan()
        {
            string[] cards = new string[0]; //Don't mulligan
            SelectMulligan(cards);
        }

        private void SelectMulligan(string[] cards)
        {
            Game game_data = gamePlay.GetGameData();
            Player player = game_data.GetPlayer(playerID);
            gamePlay.Mulligan(player, cards);
        }
        
        private void EndTurn()
        {
            if (CanPlay())
            {
                gamePlay.EndTurn();
            }
        }

        private void Resign()
        {
            int other = playerID == 0 ? 1 : 0;
            gamePlay.EndGame(other);
        }
    }
    
    
}