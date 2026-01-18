using Data;
using GameClient;
using GameLogic;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// The choice selector is a box that appears when using an ability with ChoiceSelector as target
    /// it let you choose between different abilities
    /// </summary>
    public class ChoiceSelector:SelectorPanel
    {
        public ChoiceSelectorChoice[] choices;
        
        private Card caster;
        private AbilityData ability;
        
        public CardUI cardUI;
        
        private static ChoiceSelector instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }
        
        protected override void Start()
        {
            base.Start();

            foreach (ChoiceSelectorChoice choice in choices)
                choice.onClick += OnClickChoice;
        }
        
        protected override void Update()
        {
            base.Update();

            Game game = Gameclient.Get().GetGameData();
            if (game != null && game.selector == SelectorType.None)
                Hide();
        }

        public void RefreshPanel()
        {
            if(ability == null)
                return;
            
            foreach (ChoiceSelectorChoice choice in choices)
                choice.Hide();
            
            Game gdata = Gameclient.Get().GetGameData();
            Player player = Gameclient.Get().GetPlayer();
            cardUI.SetCard(caster);
            
            int index = 0;
            foreach (AbilityData choice in ability.chainAbilities)
            {
                if (choice != null && index < choices.Length)
                {
                    ChoiceSelectorChoice achoice = choices[index];
                    achoice.SetChoice(index, choice);
                    achoice.SetInteractable(gdata.CanSelectAbility(caster, choice));
                    index++;
                }
            }
        }

        public void OnClickChoice(int index)
        {
            Game data = Gameclient.Get().GetGameData();
            if (data.selector == SelectorType.SelectorChoice)
            {
                Gameclient.Get().SelectChoice(index);
                Hide();
            }
            else
            {
                Hide();
            }
        }
        
        public void OnClickCancel()
        {
            Gameclient.Get().CancelSelection();
            Hide();
        }
        
        public override void Show(AbilityData iability, Card caster)
        {
            this.caster = caster;
            this.ability = iability;
            Show();
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }
        
        public override bool ShouldShow()
        {
            Game data = Gameclient.Get().GetGameData();
            int playerID = Gameclient.Get().GetPlayerID();
            return data.selector == SelectorType.SelectorChoice && data.selectorPlayerId == playerID;
        }
        
        public static ChoiceSelector Get()
        {
            return instance;
        }
    }
}