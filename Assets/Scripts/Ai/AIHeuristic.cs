using GameLogic;

namespace Ai
{
    /// <summary>
    /// Values and calculations for various values of the AI decision-making, adjusting these can improve your AI
    /// Heuristic: Represent the score of a board state, high score favor AI, low score favor the opponent
    /// Action Score: Represent the score of an individual action, to proritize actions if too many in a single node
    /// Action Sort Order: Value to determine the order actions should be executed in a single turn to avoid searching same things in different order, executed in ascending order
    /// </summary>
    public class AIHeuristic
    {
        //---------- Heuristic PARAMS -------------
        public int boardCardValue = 20;       //Score of having cards on board
        public int secretCardValue = 10;       //Score of having cards in secret zone
        public int handCardValue = 5;         //Score of having cards in hand
        public int killValue = 5;              //Score of killing a card

        public int playerHpValue = 4;         //Score per player hp
        public int cardAttackValue = 3;       //Score per board card attack
        public int cardHpValue = 2;           //Score per board card hp
        public int cardStatusValue = 15;       //Score per status on card (multiplied by hvalue of StatusData)
        
        //-----------
        private int aiPlayerID;           //ID of this AI, usually the human is 0 and AI is 1
        private int aiLevel;               //ai level (level 10 is the best, level 1 is the worst)
        private int heuristicModifier;     //Randomize heuristic for lower level ai
        private System.Random randomGen;

        public AIHeuristic(int playerID, int level)
        {
            aiPlayerID = playerID;
            aiLevel = level;
            randomGen = new System.Random();
            heuristicModifier = GetHeuristicModifier();
        }

        public int CalculateHeuristic(Game data, NodeState node)
        {
            Player aiplayer = data.GetPlayer(aiPlayerID);
            Player oplayer = data.GetOpponentPlayer(aiPlayerID);
            return CalculateHeuristic(data, node, aiplayer, oplayer);
        }

        //Calculate full heuristic
        //Should return a value between -10000 and 10000 (unless its a win)
        public int CalculateHeuristic(Game data, NodeState node, Player aiplayer, Player oplayer)
        {
            int score = 0;
            
            //Victories
            if (aiplayer.IsDead())
                score += -100000 + node.tdepth * 1000; //Add node depth to seek surviving longest
            if (oplayer.IsDead())
                score += 100000 - node.tdepth * 1000; //Reduce node depth to seek fastest win
            
            //Board state
            score += aiplayer.cardsBoard.Count * boardCardValue;
            score += aiplayer.cardsEquip.Count * boardCardValue;
            score += aiplayer.cardsSecret.Count * secretCardValue;
            score += aiplayer.cardsHand.Count * handCardValue;
            score+= aiplayer.killCount*killValue;
            score += aiplayer.hp * playerHpValue;
            
            score -= oplayer.cardsBoard.Count * boardCardValue;
            score -= oplayer.cardsEquip.Count * boardCardValue;
            score -= oplayer.cardsSecret.Count * secretCardValue;
            score -= oplayer.cardsHand.Count * handCardValue;
            score-= oplayer.killCount*killValue;
            score -= oplayer.hp * playerHpValue;

            foreach (Card card in aiplayer.cardsBoard)
            {
                score += card.GetAttack() * cardAttackValue;
                score += card.GetHp() * cardHpValue;
                foreach (CardStatus status in card.status)
                    score += status.StatusData.hValue * cardStatusValue;
                foreach (CardStatus status in card.ongoingStatus)
                    score += status.StatusData.hValue * cardStatusValue;
            }

            foreach (Card card in oplayer.cardsBoard)
            {
                score -= card.GetAttack() * cardAttackValue;
                score -= card.GetHp() * cardHpValue;
                foreach (CardStatus status in card.status)
                    score -= status.StatusData.hValue * cardStatusValue;
                foreach (CardStatus status in card.ongoingStatus)
                    score -= status.StatusData.hValue * cardStatusValue;
            }
            
            if(heuristicModifier>0)
                score += randomGen.Next(-heuristicModifier, heuristicModifier);
            return score;
            
        }
        
        //This calculates the score of an individual action, instead of the board state
        //When too many actions are possible in a single node, only the ones with best action score will be evaluated
        //Make sure to return a positive value
        public int CalculateActionScore(Game data, AIAction order)
        {
            if (order.type == GameAction.EndTurn)
                return 0; //Other orders are better
            
            if (order.type == GameAction.CancelSelect)
                return 0; //Other orders are better
            
            if (order.type == GameAction.CastAbility)
                return 200;

            if (order.type == GameAction.Attack)
            {
                Card card = data.GetCard(order.cardUID);
                Card target = data.GetCard(order.targetUID);
                int ascore = card.GetAttack()>=target.GetHp()?300:100; //Are you killing the card?
                int oscore = target.GetAttack()>=card.GetHp()?-200:0; //Are you getting killed?
                return ascore + oscore + target.GetAttack() * 5;            //Always better to get rid of high-attack cards
            }

            if (order.type == GameAction.AttackPlayer)
            {
                Card card = data.GetCard(order.cardUID);
                Player player = data.GetPlayer(order.targetPlayerId);
                int ascore = card.GetAttack()>=player.hp?500:200;//Are you killing the player?
                return ascore + (card.GetAttack() * 10) - player.hp;        //Always better to inflict more damage
                
            }

            if (order.type == GameAction.PlayCard)
            {
                Player player = data.GetPlayer(aiPlayerID);
                Card card = data.GetCard(order.cardUID);
                if (card.CardData.IsBoardCard())
                    return 200 + (card.GetMana() * 5) - (30 * player.cardsBoard.Count); //High cost cards are better to play, better to play when not a lot of cards in play
                else if (card.CardData.IsEquipment())
                    return 200 + (card.GetMana() * 5) - (30 * player.cardsEquip.Count);
                else
                    return 200 + (card.GetMana() * 5);
            }
            
            if (order.type == GameAction.Move)
            {
                return 100;
            }
            
            return 100; //Other actions are better than End/Cancel
        }

        //在同一个回合内，只能按排序顺序执行操作，确保它返回大于0的正值，否则它不会被排序
        //这阻止了计算A->B->C B->C->A-C->A-C->A->B等的所有可能性。。
        //如果两个AIActions具有相同的排序值，或者如果排序值为0，ai将测试所有排序变化（较慢）
        //这在每回合只有一个动作的游戏（如国际象棋）中是不必要的，但对于可以在一回合中执行多个动作的AI来说是有用的
        public int CalculateActionSort(Game data, AIAction order)
        {
            if (order.type == GameAction.EndTurn)
                return 0; //End turn can always be performed, 0 means any order
            if (data.selector != SelectorType.None)
                return 0; //Selector actions not affected by sorting
            
            Card card = data.GetCard(order.cardUID);
            Card target = order.targetUID != null ? data.GetCard(order.targetUID) : null;
            bool isSpell = card != null && !card.CardData.IsBoardCard();
            int typeSort = 0;
            if (order.type == GameAction.PlayCard && isSpell)
                typeSort = 1; //Play Spells first
            if (order.type == GameAction.CastAbility)
                typeSort = 2; //Card Abilities second
            if (order.type == GameAction.Move)
                typeSort = 3; //Move third
            if (order.type == GameAction.Attack)
                typeSort = 4; //Attacks fourth
            if (order.type == GameAction.AttackPlayer)
                typeSort = 5; //Player attacks fifth
            if (order.type == GameAction.PlayCard && !isSpell)
                typeSort = 7; //Play Characters last

            int cardSort = card != null ? (card.Hash % 100) : 0;
            int targetSort = target != null ? (target.Hash % 100) : 0;
            int sort = typeSort * 10000 + cardSort * 100 + targetSort + 1;
            return sort;


        }
        //Lower level AI add a random number to their heuristic
        private int GetHeuristicModifier()
        {
            return aiLevel switch
            {
                >= 10 => 0,
                9 => 5,
                8 => 10,
                7 => 20,
                6 => 30,
                5 => 40,
                4 => 50,
                3 => 75,
                2 => 100,
                1 => 200,
                0 => 300,
                _ => 0
            };
        }
        
        //Check if this node represent one of the players winning
        public bool IsWin(NodeState node)
        {
            return node.hvalue > 50000 || node.hvalue < -50000;
        }
        
        

    }
}