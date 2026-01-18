using CameraScripts;
using Data;
using GameLogic;
using Unit;
using UnityEngine;

namespace GameClient
{
    public class Tutorial:MonoBehaviour
    {
        private bool isTuto;
        private TutoStepGroup currentGroup;
        private TutoStep currentStep;
        private bool locked = false;
        
        private static Tutorial instance;

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            if (Gameclient.gameSetting.gameType == GameType.Adventure)
            {
                LevelData level = Gameclient.gameSetting.GetLevel();
                if (level.tutoPrefab != null)
                {
                    isTuto = true;
                    GameObject tutoObj = Instantiate(level.tutoPrefab);
                    tutoObj.GetComponent<Canvas>().worldCamera = GameCamera.GetCamera();
                    
                    Gameclient.Get().onNewTurn += OnNewTurn;
                    Gameclient.Get().onCardPlayed += OnCardPlayed;
                    Gameclient.Get().onAttackStart += OnAttack;
                    Gameclient.Get().onAttackPlayerStart += OnAttackPlayer;
                    Gameclient.Get().onAbilityStart += OnCastAbility;
                    Gameclient.Get().onAbilityTargetCard += OnTargetCard;
                    Gameclient.Get().onAbilityTargetPlayer += OnTargetPlayer;
                }
                
                HideAll();
            }
        }

        void Update()
        {
            if(Gameclient.gameSetting.gameType!=GameType.Adventure)
                return;
            Game data = Gameclient.Get().GetGameData();
            if(data==null)
                return;
        }

        private void OnNewTurn(int playerID)
        {
            Game data = Gameclient.Get().GetGameData();
            if (data == null)
                return;
            
            EndGroup();

            if (playerID != Gameclient.Get().GetPlayerID())
                return;

            TutoStepGroup group = TutoStepGroup.Get(TutoStartTrigger.StartTurn, data.turnCount);
            ShowGroup(group);
            
        }
        
        private void OnCardPlayed(Card card, Slot slot)
        {
            Hide();

            Game data = Gameclient.Get().GetGameData();
            if (card.playerID == Gameclient.Get().GetPlayerID())
            {
                TriggerEndStep(TutoEndTrigger.PlayCard);
                TriggerStartGroup(TutoStartTrigger.PlayCard, card);
            }
        }
        
        private void OnAttack(Card card, Card target)
        {
            Hide();

            Game data = Gameclient.Get().GetGameData();
            if (card.playerID == Gameclient.Get().GetPlayerID())
            {
                TriggerEndStep(TutoEndTrigger.Attack, 2f);
                TriggerStartGroup(TutoStartTrigger.Attack, card);
                TriggerStartGroup(TutoStartTrigger.Attack, target);
            }
        }
        
        private void OnAttackPlayer(Card card, Player target)
        {
            Hide();

            Game data = Gameclient.Get().GetGameData();
            if (card.playerID == Gameclient.Get().GetPlayerID())
            {
                TriggerEndStep(TutoEndTrigger.AttackPlayer, 2f);
                TriggerStartGroup(TutoStartTrigger.Attack, card);
            }
        }
        
        private void OnCastAbility(AbilityData ability, Card card)
        {
            Game data = Gameclient.Get().GetGameData();
            if (card.playerID == Gameclient.Get().GetPlayerID())
            {
                TriggerEndStep(TutoEndTrigger.CastAbility);
                TriggerStartGroup(TutoStartTrigger.CastAbility, card);
            }
        }
        
        private void OnTargetCard(AbilityData ability, Card card, Card target)
        {
            Game data = Gameclient.Get().GetGameData();
            if (card.playerID == Gameclient.Get().GetPlayerID())
            {
                TriggerEndStep(TutoEndTrigger.SelectTarget);
            }
        }
        
        private void OnTargetPlayer(AbilityData ability, Card card, Player target)
        {
            Game data = Gameclient.Get().GetGameData();
            if (card.playerID == Gameclient.Get().GetPlayerID())
            {
                TriggerEndStep(TutoEndTrigger.SelectTarget);
            }
        }

        private void TriggerEndStep(TutoEndTrigger trigger, float time = 1f)
        {
            if (currentStep != null && currentStep.endTrigger == trigger)
            {
                Hide();
                locked = true;
                TimeTool.WaitFor(time, () =>
                {
                    locked = false;
                    ShowNext();
                });
            }
        }

        private void TriggerStartGroup(TutoStartTrigger trigger, Card card)
        {
            if (currentGroup == null || !currentGroup.forced)
            {
                if (currentStep == null || !currentStep.forced)
                {
                    CardData target = card?.CardData;
                    ShowGroup(TutoStartTrigger.PlayCard, target);
                }
            }
        }
        
        public void ShowGroup(TutoStartTrigger trigger, CardData target)
        {
            Game data = Gameclient.Get().GetGameData();
            TutoStepGroup group = TutoStepGroup.Get(trigger, target, data.turnCount);
            ShowGroup(group);
        }
        
        public void ShowGroup(TutoStepGroup group)
        {
            if (group != null)
            {
                currentGroup = group;
                group.SetTriggered();
                TutoStep step = TutoStep.Get(group, 0);
                Show(step);
            }
        }

        public void ShowNext()
        {
            if (currentGroup != null)
            {
                int index = GetNextIndex();
                TutoStep step = TutoStep.Get(currentGroup, index);
                if (step != null)
                    Show(step);
                else
                    EndGroup();
            }
        }
        
        public void Show(TutoStep step)
        {
            HideAll();
            currentStep = step;
            if (step != null)
                step.Show();
        }
        
        public void EndGroup()
        {
            HideAll();
            currentGroup = null;
            currentStep = null;
        }
        
        public void Hide(TutoStep step)
        {
            if (step != null)
                step.Hide();
        }
        
        public void Hide()
        {
            Hide(currentStep);
        }
        
        public bool CanDo(TutoEndTrigger trigger)
        {
            return CanDo(trigger, null);
        }
        
        public bool CanDo(TutoEndTrigger trigger, Slot slot)
        {
            Game data = Gameclient.Get().GetGameData();
            Card card = data.GetSlotCard(slot);
            return CanDo(trigger, card);
        }
        
        public bool CanDo(TutoEndTrigger trigger, Card target)
        {
            if (!isTuto)
                return true; //Not a tutorial

            if (locked)
                return false;

            if (currentStep != null && currentStep.forced)
            {
                if (trigger == TutoEndTrigger.CastAbility && currentStep.endTrigger == TutoEndTrigger.SelectTarget)
                    return true; //Dont get locked into select target if ability was canceled

                if (currentStep.endTrigger != trigger)
                    return false; //Wrong trigger

                CardData targetData = target?.CardData;
                if (currentStep.triggerTarget != null && currentStep.triggerTarget != targetData)
                    return false; //Wrong target
            }

            return true;
        }
        
        public int GetNextIndex()
        {
            if (currentStep != null)
                return currentStep.GetStepIndex() + 1;
            return 0;
        }
        
        public bool IsTuto()
        {
            return isTuto;
        }
        
        public TutoEndTrigger GetEndTrigger()
        {
            if (currentStep != null)
                return currentStep.endTrigger;
            return TutoEndTrigger.Click;
        }

        private void HideAll()
        {
            TutoStep.HideAll();
        }

        public static Tutorial Get()
        {
            return instance;
        }

     
        
    }
    
    public enum TutoStartTrigger
    {
        StartTurn = 0,
        PlayCard = 10,
        Attack = 20,
        CastAbility = 30,
    }

    public enum TutoEndTrigger
    {
        Click = 0,
        EndTurn = 5,
        PlayCard = 10,
        Move = 15,
        Attack = 20,
        AttackPlayer = 25,
        CastAbility = 30,
        SelectTarget = 35,
    }
}