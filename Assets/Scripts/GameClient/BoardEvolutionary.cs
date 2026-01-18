using GameLogic;
using TMPro;
using UI;
using Unit;
using UnityEngine;

namespace GameClient
{
    public class BoardEvolutionary:MonoBehaviour
    {
        public bool opponent;
        
        public TextMeshProUGUI evolutionaryTurnTip;
        
        public IconBar evolutionaryBar;
        
        public bool focus;
        
        public EvolutionaryType type;

        protected virtual void Start()
        {
            
        }
        
        protected virtual void Update()
        {
           
        }

        protected virtual void OnMouseDown()
        {
            if (GameUI.IsOverUILayer("UI"))
                return;
            PlayerControls.Get().SelectEvolutionary(this);
            if (GameTool.IsMobile())
            {
                focus = true;
            }
        }

        protected virtual void OnMouseUp()
        {
            
        }
      

        protected Player GetPlayer()
        {
            return opponent?Gameclient.Get().GetOpponentPlayer():Gameclient.Get().GetPlayer();
        }
        
        public EvolutionaryType GetEvolutionary() => type;
    }

    public enum EvolutionaryType
    {
        Normal = 0,
        Super = 1,
    }
    
    
    
}