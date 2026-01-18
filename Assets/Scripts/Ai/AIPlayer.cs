using GameLogic;

namespace Ai
{
    /// <summary>
    /// AI player base class, other AI inherit from this
    /// </summary>
    public abstract class AIPlayer
    {
        public int playerID;
        public int aiLevel;
        
        protected Gamelogic gamePlay;
        
        public virtual void Update()
        {
            //Script called by game server to update AI
            //Override this to let the AI play
        }
        
        public bool CanPlay()
        {
            Game gameData = gamePlay.GetGameData();
            Player player = gameData.GetPlayer(playerID);
            bool canPlay = gameData.IsPlayerTurn(player) || gameData.IsPlayerMulliganTurn(player);
            return canPlay && !gamePlay.IsResolving();
        }
        
        public static AIPlayer Create(AIType type, Gamelogic gamePlay, int id, int level = 0)
        {
            if (type == AIType.Random)
                return new AIPlayerRandom(gamePlay, id, level);
            if (type == AIType.MiniMax)
                return new AIPlayerMM(gamePlay, id, level);
            return null;
        }
        
    
        
    }
    
    public enum AIType
    {
        Random = 0,      //Dumb AI that just do random moves, useful for testing cards without getting destroyed
        MiniMax = 10,    //Stronger AI using Minimax algo with alpha-beta pruning
    }
    
    
}