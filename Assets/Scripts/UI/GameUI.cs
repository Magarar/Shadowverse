using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CameraScripts;
using Data;
using GameClient;
using GameLogic;
using TMPro;
using Unit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class GameUI:MonoBehaviour
    {
        public Canvas gameCanvas;
        public Canvas panelCanvas;
        public Canvas topCanvas;
        public UIPanel menuPanel;
        public TextMeshProUGUI quitBtn;

        [Header("Turn Area")]
        public TextMeshProUGUI turnCount;
       // public TextMeshProUGUI turnTimer;
        public Image turnTimerFillout;
        public Button endTurnButton;
        public Animator timeoutAnimator;
        public AudioClip timeoutAudio;
        

        private float selectorTimer = 0f;
        private float endTurnTimer = 0f;
        private int prevTimeVal = 0;

        private static GameUI instance;
        
        void Awake()
        {
            instance = this;

            if (gameCanvas.worldCamera == null)
                gameCanvas.worldCamera = Camera.main;
            if (panelCanvas.worldCamera == null)
                panelCanvas.worldCamera = Camera.main;
            if (topCanvas.worldCamera == null)
                topCanvas.worldCamera = Camera.main;
        }
        
        private void Start()
        {
            Gameclient.Get().onGameStart += OnGameStart;
            Gameclient.Get().onNewTurn += OnNewTurn;
            LoadPanel.Get().Show(true);
            BlackPanel.Get().Show(true);
            BlackPanel.Get().Hide();

            if (quitBtn != null)
                quitBtn.text = Gameclient.gameSetting.IsOnlinePlayer() ? "Resign" : "Quit";
        }

        void Update()
        {
            Game data = Gameclient.Get().GetGameData();
            bool isConnecting = data == null || data.state == GameState.Connecting;
            bool connectionLost = !isConnecting && !Gameclient.Get().IsReady();
            ConnectionPanel.Get().SetVisible(connectionLost);
            
            //Menu
            if (Input.GetKeyDown(KeyCode.Escape))
                menuPanel.Toggle();
            
            if (!Gameclient.Get().IsReady())
                return;
            
            bool yourTurn = Gameclient.Get().IsYourTurn();
            LoadPanel.Get().SetVisible(isConnecting && !data.HasStarted());
            endTurnButton.interactable = yourTurn && endTurnTimer > 1f;
            endTurnTimer += Time.deltaTime;
            selectorTimer += Time.deltaTime;
            
            //Timer
            //turnCount.text = "Turn " + data.turnCount.ToString();
            // turnTimer.enabled = data.turnTimer > 0f;
            // turnTimer.text = Mathf.RoundToInt(data.turnTimer).ToString();
            // turnTimer.enabled = data.turnTimer < 999f;
            
            turnTimerFillout.fillAmount = data.turnTimer / GamePlayData.Get().turnDuration;
            turnTimerFillout.color = yourTurn?Color.green:Color.red;
            turnTimerFillout.enabled = data.turnTimer > 0f;
            
            //Simulate timer
            if (data.state == GameState.Play && data.turnTimer > 0f)
                data.turnTimer -= Time.deltaTime;
            
            //Timer warning
            if (data.state == GameState.Play)
            {
                int val = Mathf.RoundToInt(data.turnTimer);
                int tickVal = 10;
                if (val < prevTimeVal && val <= tickVal)
                    PulseFX();
                prevTimeVal = val;
            }
            
            //Show selector panels
            foreach (SelectorPanel panel in SelectorPanel.GetAll())
            {
                bool shouldShow = panel.ShouldShow();
                if (shouldShow != panel.IsVisible() && selectorTimer > 1f)
                {
                    selectorTimer = 0f;
                    panel.SetVisible(shouldShow);
                    if (shouldShow)
                    {
                        AbilityData ability = AbilityData.Get(data.selectorAbilityId);
                        Card caster = data.GetCard(data.selectorCasterUid);
                        panel.Show(ability, caster);
                    }
                }
            }
            
            //Hide
            if (!yourTurn && data.phase != GamePhase.Mulligan)
            {
                SelectorPanel.HideAll();
            }
        }
        
        private void PulseFX()
        {
            //timeoutAnimator?.SetTrigger("pulse");
            AudioTool.Get().PlaySFX("time", timeoutAudio, 1f);
        }
        
        private void OnGameStart()
        {
            Game game = Gameclient.Get().GetGameData();
            ExtraManaToggle.Get().SetEnable(game.firstPlayer != Gameclient.Get().GetPlayerID());
        }
        
        private void OnNewTurn(int playerID)
        {
            CardSelector.Get().Hide();
            SelectTargetUI.Get().Hide();
        }
        
        public void OnClickNextTurn()
        {
            if (!Tutorial.Get().CanDo(TutoEndTrigger.EndTurn))
                return;

            Gameclient.Get().EndTurn();
            endTurnTimer = 0f; //Disable button immediately (dont wait for refresh)
        }
        
        public void OnClickRestart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        
        public void OnClickMenu()
        {
            menuPanel.Show();
        }
        public void OnClickBack()
        {
            menuPanel.Hide();
        }
        
        public void OnClickQuit()
        {
            bool online = Gameclient.gameSetting.IsOnlinePlayer();
            bool ended = Gameclient.Get().HasEnded();
            if (online && !ended)
                Gameclient.Get().Resign();
            else
                StartCoroutine(QuitRoutine("Menu"));
            menuPanel.Hide();
        }

        private IEnumerator QuitRoutine(string scene)
        {
            BlackPanel.Get().Show();
            AudioTool.Get().FadeOutMusic("music");
            AudioTool.Get().FadeOutSFX("ambience");
            AudioTool.Get().FadeOutSFX("ending_sfx");

            yield return new WaitForSeconds(1f);

            Gameclient.Get().Disconnect();
            SceneNav.GoTo(scene);
        }
        
        public void OnClickSwapObserve()
        {
            int other = Gameclient.Get().GetPlayerID() == 0 ? 1 : 0;
            Gameclient.Get().SetObserverMode(other);
        }
        
        public static bool IsUIOpened()
        {
            return CardSelector.Get().IsVisible() || EndGamePanel.Get().IsVisible();
        }
        
        public static bool IsOverUI()
        {
            //return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
        
        public static bool IsOverUILayer(string sortingLayer)
        {
            return IsOverUILayer(SortingLayer.NameToID(sortingLayer));
        }

        public static bool IsOverUILayer(int sortingLayer)
        {
            //return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            int count = results.Count(result => result.sortingLayer == sortingLayer);
            return count > 0;
        }

        public static bool IsOverRectTransform(Canvas canvas, RectTransform rect)
        {
            PointerEventData pevent = new PointerEventData(EventSystem.current);
            pevent.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            raycaster.Raycast(pevent, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.transform == rect || result.gameObject.transform.IsChildOf(rect))
                    return true;
            }
            return false;
        }
        
        public static Vector2 MouseToRectPos(Canvas canvas, RectTransform rect, Vector2 screenPos)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, canvas.worldCamera, out var anchorPos);
                return anchorPos;
            }
            else
            {
                Vector2 anchorPos = screenPos - new Vector2(rect.position.x, rect.position.y);
                anchorPos = new Vector2(anchorPos.x / rect.lossyScale.x, anchorPos.y / rect.lossyScale.y);
                return anchorPos;
            }
        }
        
        public static Vector3 MouseToWorld(Vector2 mousePos, float distance = 10f)
        {
            Camera cam = GameCamera.Get() != null ? GameCamera.GetCamera() : Camera.main;
            Vector3 wpos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance));
            return wpos;
        }
        
        public static string FormatNumber(int value)
        {
            return string.Format("{0:#,0}", value);
        }
        
        

        public static GameUI Get()
        {
            return instance;
        }

    }
}