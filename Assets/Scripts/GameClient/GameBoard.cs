using System.Collections;
using System.Collections.Generic;
using CameraScripts;
using Data;
using GameLogic;
using UI;
using Unit;
using UnityEngine;
using UnityEngine.Events;

namespace GameClient
{
    /// <summary>
    /// GameBoard takes care of spawning and despawning BoardCards, based on the refreshed data received from the server
    /// It also ends the game when the server sends a endgame
    /// </summary>

    public class GameBoard:MonoBehaviour
    {
        public GameObject cardPrefab;
        
        public UnityAction<Card> onCardSpawn;
        public UnityAction<Card> onCardKilled;
        
        private bool gameEnded = false;
        
        private static GameBoard instance;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            if(!Gameclient.Get().IsReady())
                return;
            Game data = Gameclient.Get().GetGameData();
            
            //--- Board cards --------
            List<BoardCard> cards = BoardCard.GetAll();
            
            //Add new cards
            foreach (Player p in data.players)
            {
                foreach (Card card in p.cardsBoard)
                {
                    BoardCard bcard = BoardCard.Get(card.uid);
                    if(card!=null&&bcard==null)
                        SpawnNewCard(card);
                }
            }
            
            //Remove cards
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                BoardCard bcard = cards[i];
                if (bcard != null && HasBoardCard(bcard) && !bcard.IsDamagedDelayed())
                {
                    KillCard(bcard);
                }
            }
            
            //--- End Game ----
            if (!gameEnded && data.state == GameState.GameEnded)
            {
                gameEnded = true;
                EndGame();
            }
        }

        private void SpawnNewCard(Card card)
        {
            GameObject cardObj = Instantiate(cardPrefab,Vector3.zero,Quaternion.identity);
            cardObj.SetActive(true);
            cardObj.GetComponent<BoardCard>().SetCard(card);
            onCardSpawn?.Invoke(card);
        }
        
        private void KillCard(BoardCard bcard)
        {
            bcard.Destroy();
            onCardKilled?.Invoke(bcard.GetCard());
        }
        
        private bool HasBoardCard(BoardCard bcard)
        {
            Game data = Gameclient.Get().GetGameData();
            Card card = data.GetBoardCard(bcard.GetCardUID());
            return card == null&&!bcard.IsDead();
        }

        private void EndGame()
        {
            StartCoroutine(EndGameRun());
        }

        private IEnumerator EndGameRun()
        {
            Game data = Gameclient.Get().GetGameData();
            Player pwinner = data.GetPlayer(data.currentPlayer);
            Player player = Gameclient.Get().GetPlayer();
            bool win = pwinner != null && player.id == pwinner.id;
            bool tied = pwinner == null;
            
            AudioTool.Get().FadeOutMusic("music");
            
            yield return new WaitForSeconds(1);

            if (win)
                PlayerUI.Get(true).Kill();
            if(!win&&!tied)
                PlayerUI.Get(false).Kill();

            if (win && AssetData.Get().winFX != null)
                Instantiate(AssetData.Get().winFX, Vector3.zero, Quaternion.identity);
            else if(tied && AssetData.Get().tiedFX != null)
                Instantiate(AssetData.Get().tiedFX, Vector3.zero, Quaternion.identity);
            else if(tied&&AssetData.Get().loseFX!=null)
                Instantiate(AssetData.Get().loseFX, Vector3.zero, Quaternion.identity);

            AudioTool.Get().PlaySFX("ending_sfx", win ? AssetData.Get().winAudio : AssetData.Get().loseAudio);
            
            AudioTool.Get().PlayMusic("music",win?AssetData.Get().winMusic:AssetData.Get().defaultmusic,0.4f,false);
            yield return new WaitForSeconds(2);
            
            EndGamePanel.Get().ShowEnd(data.currentPlayer);
        }
        
        //Raycast mouse position to board position
        public Vector3 RaycastMouseBoard()
        {
            Ray ray = GameCamera.Get().MouseToRay(Input.mousePosition);
            Plane plane = new Plane(transform.forward, 0f);
            bool success = plane.Raycast(ray, out float dist);
            if (success)
                return ray.GetPoint(dist);
            return Vector3.zero;
        }
        
        public Vector3 GetAngles()
        {
            return transform.rotation.eulerAngles;
        }
        
        public static GameBoard Get()
        {
            return instance;
        }
    }
}