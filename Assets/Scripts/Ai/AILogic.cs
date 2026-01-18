using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Data;
using GameLogic;
using Unit;
using UnityEngine;
using UnityEngine.Profiling;

namespace Ai
{
    /// <summary>
    /// Minimax algorithm for AI. 
    /// </summary>
    public class AILogic
    {
        //-------- AI Logic Params ------------------
        public int aiDepth = 3;               // 预测的回合深度
        public int aiDepthWide = 1;            // 前几回合使用更宽的搜索
        public int actionsPerTurn = 2;          // 每回合最大连续动作数
        public int actionsPerTurnWide = 3;     // 宽搜索时的连续动作数  
        public int nodesPerAction = 4;         // 每个动作评估的子节点数
        public int nodesPerActionWide = 7;     // 宽搜索时的子节点数
        // 第一回合使用宽搜索原因：
        // - 开局决策影响整个游戏
        // - 动作组合较多，需要更全面的评估
        // - 计算资源相对充足（游戏刚开始）

        // 后续回合使用窄搜索原因：  
        // - 避免组合爆炸
        // - 聚焦最有希望的路径
        // - 保持实时响应
        
        public int aiPlayerID;                    //AI player_id  (usually its 1)
        public int aiLevel;                       //AI level
        
        private Gamelogic gameLogic;           //Game logic used to calculate moves
        private Game originalData;             //Original game data when start calculating possibilities
        private AIHeuristic heuristic;
        private Thread aiThread;
        
        private NodeState firstNode = null;
        private NodeState bestMove = null;
        
        private bool running = false;
        private int nbCalculated = 0;
        private int reachedDepth = 0;
        
        private System.Random randomGen;
        
        private Pool<NodeState> nodePool = new Pool<NodeState>();
        private Pool<Game> dataPool = new Pool<Game>();
        private Pool<AIAction> actionPool = new Pool<AIAction>();
        private Pool<List<AIAction>> listPool = new Pool<List<AIAction>>();
        private ListSwap<Card> cardArray = new ListSwap<Card>();
        private ListSwap<Slot> slotArray = new ListSwap<Slot>();

        public static AILogic Create(int playerID, int level)
        {
            AILogic job = new AILogic();
            job.aiPlayerID = playerID;
            job.aiLevel = level;
            
            job.heuristic = new AIHeuristic(playerID, level);
            job.gameLogic = new Gamelogic(true);//Skip all delays for the AI calculations
            return job;
        }

        public void RunAI(Game data)
        {
            if (running)
                return;

            originalData = Game.CloneNew(data); //Clone game data to keep original data unaffected
            gameLogic.ClearResolve();
            gameLogic.SetData(originalData);          //Assign data to game logic
            randomGen = new System.Random();       //Reset random seed

            firstNode = null;
            reachedDepth = 0;
            nbCalculated = 0;
            running = true;
            
            //Uncomment these lines to run on separate thread (and comment Execute()), better for production so it doesn't freeze the UI while calculating the AI
            aiThread = new Thread(Execute);
            aiThread.Start();
            
            //Uncomment this line to run on main thread (and comment the thread one), better for debuging since you will be able to use breakpoints, profiler and Debug.Log
            //Execute();
        }

        public void Stop()
        {
            running = false;
            if (aiThread != null&& aiThread.IsAlive)
                aiThread.Abort();
        }

        private void Execute()
        {
            //Create first node
            firstNode = CreateNode(null,null,aiPlayerID,0,0);
            firstNode.hvalue = heuristic.CalculateHeuristic(originalData,firstNode);
            firstNode.alpha = int.MinValue;
            firstNode.beta = int.MaxValue;
            
            Profiler.BeginSample("AI");
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

            //Calculate first node
            CalculateNode(originalData, firstNode);
            
            Debug.Log("AI: Time " + watch.ElapsedMilliseconds + "ms Depth " + reachedDepth + " Nodes " + nbCalculated);
            Profiler.EndSample();
            
            //Save best move
            bestMove = firstNode.bestChild;
            running = false;
        }

        //Add list of all possible orders and search in all of them
        private void CalculateNode(Game data, NodeState node)
        {
            Profiler.BeginSample("Add Actions");
            Player player = data.GetPlayer(data.currentPlayer);
            List<AIAction> actionList = listPool.Create();
            
            int maxActions = node.tdepth < aiDepthWide ? actionsPerTurnWide : actionsPerTurn;
            if (node.taction < maxActions)
            {
                if (data.selector == SelectorType.None)
                {
                    //Play card
                    for (int c = 0; c < player.cardsHand.Count; c++)
                    {
                        Card card = player.cardsHand[c];
                        AddActions(actionList, data, node, GameAction.PlayCard, card);
                    }
                    
                    //Action on board
                    for (int c = 0; c < player.cardsBoard.Count; c++)
                    {
                        Card card = player.cardsBoard[c];
                        AddActions(actionList, data, node, GameAction.Attack, card);
                        AddActions(actionList, data, node, GameAction.AttackPlayer, card);
                        AddActions(actionList, data, node, GameAction.CastAbility, card);
                    }
                    
                    if (player.hero != null)
                        AddActions(actionList, data, node, GameAction.CastAbility, player.hero);
                }
                else
                {
                    AddSelectActions(actionList, data, node);
                }
            }
            
            //End Turn (dont add action if ai can still attack player, or ai hasnt spent any Mana)
            bool isFullMana = HasAction(actionList, GameAction.PlayCard) && player.Mana >= player.manaMax;
            bool canAttackPlayer = HasAction(actionList, GameAction.AttackPlayer);
            bool canEnd = !canAttackPlayer && !isFullMana && data.selector == SelectorType.None;
            if (actionList.Count == 0 || canEnd)
            {
                AIAction action = CreateAction(GameAction.EndTurn);
                actionList.Add(action);
            }
            
            //Remove actions with low score
            FilterActions(data, node, actionList);
            Profiler.EndSample();
            
            //Execute valid action and search child node
            for (int i = 0; i < actionList.Count; i++)
            {
                AIAction action = actionList[i];
                if (action.valid && node.alpha < node.beta)
                {
                    CalculateChildNode(data, node, action);
                }
            }
            
            actionList.Clear();
            listPool.Dispose(actionList);


        } 
        
        //Mark valid/invalid on each action, if too many actions, will keep only the ones with best score
        private void FilterActions(Game data, NodeState node, List<AIAction> actionList)
        {
            int countValid = 0;
            for (int i = 0; i < actionList.Count; i++)
            {
                AIAction action = actionList[i];
                action.sort = heuristic.CalculateActionSort(data, action);
                action.valid =action.sort <= 0 || action.sort >= node.sortMin;
                if (action.valid)
                    countValid++;
            }
            
            int maxActions = node.tdepth < aiDepthWide ? nodesPerActionWide : nodesPerAction;
            int maxActionsSkip = maxActions + 2; //No need to calculate all scores if its just to remove 1-2 actions
            if (countValid <= maxActionsSkip)
                return; //No filtering needed
            
            //Calculate scores
            for (int i = 0; i < actionList.Count; i++)
            {
                AIAction action = actionList[i];
                if (action.valid)
                {
                    action.score = heuristic.CalculateActionScore(data, action);
                }
            }
            //Sort, and invalidate actions with low score
            actionList.Sort((a,b) => b.score.CompareTo(a.score));
            for (int i = 0; i < actionList.Count; i++)
            {
                AIAction action = actionList[i];
                action.valid = action.valid && i < maxActions;
            }
            

        }

        //Create a child node for parent, and calculate it
        private void CalculateChildNode(Game data, NodeState parent, AIAction action)
        {
            if (action.type == GameAction.None)
                return;
            
            int playerID = data.currentPlayer;
            
            //Clone data so we can update it in a new node
            Profiler.BeginSample("Clone Data");
            Game ndata = dataPool.Create();
            Game.Clone(data, ndata);
            gameLogic.ClearResolve();
            gameLogic.SetData(ndata);
            Profiler.EndSample();
            
            //Execute move and update data
            Profiler.BeginSample("Execute AIAction");
            //模拟使用，不会影响现有游戏
            DoAIAction(ndata, action, playerID);
            Profiler.EndSample();

            //Update depth
            bool newTurn = action.type == GameAction.EndTurn;
            int nextTdepth = parent.tdepth;
            int nextTaction = parent.taction+1;

            if (newTurn)
            {
                nextTdepth = parent.tdepth+1;
                nextTaction = 0;
            }
            
            //Create node
            Profiler.BeginSample("Create Node");
            NodeState childNode = CreateNode(parent, action, playerID, nextTdepth, nextTaction);
            parent.childs.Add(childNode);
            Profiler.EndSample();
            
            //Set minimum sort for next AIActions, if new turn, reset to 0
            childNode.sortMin = newTurn ? 0 : Mathf.Max(action.sort, childNode.sortMin);
            
            //If win or reached max depth, stop searching deeper
            if (!ndata.HasEnded() && childNode.tdepth < aiDepth)
            {
                //Calculate child
                CalculateNode(ndata, childNode);
            }else
            {
                //End of tree, calculate full Heuristic
                childNode.hvalue = heuristic.CalculateHeuristic(ndata, childNode);
            }
            
            //Update parents hvalue, alpha, beta, and best child
            if (playerID == aiPlayerID)
            {
                //AI player
                if (parent.bestChild == null || childNode.hvalue > parent.hvalue)
                {
                    parent.bestChild = childNode;
                    parent.hvalue = childNode.hvalue;
                    parent.alpha = Mathf.Max(parent.alpha, parent.hvalue);
                }
            }
            else
            {
                //Opponent player
                if (parent.bestChild == null || childNode.hvalue < parent.hvalue)
                {
                    parent.bestChild = childNode;
                    parent.hvalue = childNode.hvalue;
                    parent.beta = Mathf.Min(parent.beta, parent.hvalue);
                }
            }
            
            
            //Just for debug, keep track of node/depth count
            nbCalculated++;
            if (childNode.tdepth > reachedDepth)
                reachedDepth = childNode.tdepth;
            
            //We are done with this game data, dispose it.
            //Dont dispose NodeState here (node_pool) since we want to retrieve the full tree path later
            dataPool.Dispose(ndata);
        }
        
        private NodeState CreateNode(NodeState parent, AIAction action, int playerID, int turnDepth, int turnAction)
        {
            NodeState nnode = nodePool.Create();
            nnode.currentPlayer = playerID;
            nnode.tdepth = turnDepth;
            nnode.taction = turnAction;
            nnode.parent = parent;
            nnode.lastAction = action;
            nnode.alpha = parent?.alpha ?? int.MinValue;
            nnode.beta = parent?.beta ?? int.MaxValue;
            nnode.hvalue = 0;
            nnode.sortMin = 0;
            return nnode;
        }

        private void AddActions(List<AIAction> actions, Game data, NodeState node, ushort type, Card card)
        {
            Player player = data.GetPlayer(data.currentPlayer);
            
            if (data.selector != SelectorType.None)
                return;

            if (card.HasStatus(StatusType.Paralysed))
                return;

            if (type == GameAction.PlayCard)
            {
                if (card.CardData.IsBoardCard())
                {
                    //Doesn't matter where the card is played
                    Slot slot = player.GetRandomEmptySlot(randomGen, slotArray.Get());

                    if (data.CanPlayCard(card, slot))
                    {
                        AIAction action = CreateAction(type, card);
                        action.slot = slot;
                        actions.Add(action);
                    }
                }else if (card.CardData.IsEquipment())
                {
                    Player tplayer = data.GetPlayer(card.playerID);
                    for (int c = 0; c < tplayer.cardsBoard.Count; c++)
                    {
                        Card tcard = tplayer.cardsBoard[c];
                        if (data.CanPlayCard(card, tcard.slot))
                        {
                            AIAction action = CreateAction(type, card);
                            action.slot = tcard.slot;
                            action.targetPlayerId = tplayer.id;
                            actions.Add(action);
                        }
                    }
                }else if (card.CardData.IsRequireTargetSpell())
                {
                    for (int p = 0; p < data.players.Length; p++)
                    {
                        Player tplayer = data.players[p];
                        Slot tslot = new Slot(tplayer.id);
                        if (data.CanPlayCard(card, tslot))
                        {
                            AIAction action = CreateAction(type, card);
                            action.slot = tslot;
                            action.targetPlayerId = tplayer.id;
                            actions.Add(action);
                        }
                    }
                    foreach (Slot slot in Slot.GetAll())
                    {
                        if (data.CanPlayCard(card, slot))
                        {
                            Card slotCard = data.GetSlotCard(slot);
                            AIAction action = CreateAction(type, card);
                            action.slot = slot;
                            action.targetUID = slotCard?.uid;
                            actions.Add(action);
                        }
                    }
                }else if (data.CanPlayCard(card, Slot.None))
                {
                    AIAction action = CreateAction(type, card);
                    actions.Add(action);
                }
            }
            
            if (type == GameAction.Attack)
            {
                if (card.CanAttack())
                {
                    for (int p = 0; p < data.players.Length; p++)
                    {
                        if (p != player.id)
                        {
                            Player oplayer = data.players[p];
                            for (int tc = 0; tc < oplayer.cardsBoard.Count; tc++)
                            {
                                Card target = oplayer.cardsBoard[tc];
                                if (data.CanAttackTarget(card, target))
                                {
                                    AIAction action = CreateAction(type, card);
                                    action.targetUID = target.uid;
                                    actions.Add(action);
                                }
                            }
                        }
                    }
                }
            }
            
            if (type == GameAction.AttackPlayer)
            {
                if (card.CanAttack())
                {
                    for (int p = 0; p < data.players.Length; p++)
                    {
                        if (p != player.id)
                        {
                            Player oplayer = data.players[p];
                            if (data.CanAttackTarget(card, oplayer))
                            {
                                AIAction action = CreateAction(type, card);
                                action.targetPlayerId = oplayer.id;
                                actions.Add(action);
                            }
                        }
                    }
                }
            }
            
            
            if (type == GameAction.CastAbility)
            {
                List<AbilityData> abilities = card.GetAbilities();
                for (int a = 0; a < abilities.Count; a++)
                {
                    AbilityData ability = abilities[a];
                    if (ability.trigger == AbilityTrigger.Activate && data.CanCastAbility(card, ability) && ability.HasValidSelectTarget(data, card))
                    {
                        AIAction action = CreateAction(type, card);
                        action.abilityID = ability.id;
                        actions.Add(action);
                    }
                }
            }
            
            if (type == GameAction.Move)
            {
                foreach (Slot slot in Slot.GetAll(player.id))
                {
                    if (data.CanMoveCard(card, slot))
                    {
                        AIAction action = CreateAction(type, card);
                        action.slot = slot;
                        actions.Add(action);
                    }
                }
            }
        }

        private void AddSelectActions(List<AIAction> actions, Game data, NodeState node)
        {
            if (data.selector == SelectorType.None)
                return;
            
            Player player = data.GetPlayer(data.selectorPlayerId);
            Card caster = data.GetCard(data.selectorCasterUid);
            AbilityData ability = AbilityData.Get(data.selectorAbilityId);
            
            if (player == null || caster == null)
                return;

            if (data.selector == SelectorType.SelectTarget && ability != null)
            {
                for (int p = 0; p < data.players.Length; p++)
                {
                    Player tplayer = data.players[p];
                    if (ability.CanTarget(data, caster, tplayer))
                    {
                        AIAction action = CreateAction(GameAction.SelectPlayer, caster);
                        action.targetPlayerId = tplayer.id;
                        actions.Add(action);
                    }
                }
                
                foreach (Slot slot in Slot.GetAll())
                {
                    Card tcard = data.GetSlotCard(slot);
                    if (tcard != null && ability.CanTarget(data, caster, tcard))
                    {
                        AIAction action = CreateAction(GameAction.SelectCard, caster);
                        action.targetUID = tcard.uid;
                        actions.Add(action);
                    }
                    else if (tcard == null && ability.CanTarget(data, caster, slot))
                    {
                        AIAction action = CreateAction(GameAction.SelectSlot, caster);
                        action.slot = slot;
                        actions.Add(action);
                    }
                }
            }
            
            if (data.selector == SelectorType.SelectorCard && ability != null)
            {
                for (int p = 0; p < data.players.Length; p++)
                {
                    List<Card> cards = ability.GetCardTargets(data, caster, cardArray);
                    foreach (Card tcard in cards)
                    {
                        AIAction action = CreateAction(GameAction.SelectCard, caster);
                        action.targetUID = tcard.uid;
                        actions.Add(action);
                    }
                }
            }
            
            if (data.selector == SelectorType.SelectorChoice && ability != null)
            {
                for (int i = 0; i < ability.chainAbilities.Length; i++)
                {
                    AbilityData choice = ability.chainAbilities[i];
                    if (choice != null && data.CanSelectAbility(caster, choice))
                    {
                        AIAction action = CreateAction(GameAction.SelectChoice, caster);
                        action.value = i;
                        actions.Add(action);
                    }
                }
            }
            
            if (data.selector == SelectorType.SelectorCost)
            {
                for (int i = 1; i <= player.Mana; i++)
                {
                    AIAction action = CreateAction(GameAction.SelectCost, caster);
                    action.value = i;
                    actions.Add(action);
                }
            }
            
            //Add option to cancel, if no valid options
            if (actions.Count == 0)
            {
                AIAction caction = CreateAction(GameAction.CancelSelect, caster);
                actions.Add(caction);
            }
        }

        private AIAction CreateAction(ushort type)
        {
            AIAction action = actionPool.Create();
            action.Clear();
            action.type = type;
            action.valid = true;
            return action;
        }
        
        private AIAction CreateAction(ushort type, Card card)
        {
            AIAction action = actionPool.Create();
            action.Clear();
            action.type = type;
            action.cardUID = card.uid;
            action.valid = true;
            return action;
        }
        
        //Simulate AI action
        private void DoAIAction(Game data, AIAction action, int playerID)
        {
             Player player = data.GetPlayer(playerID);

            if (action.type == GameAction.PlayCard)
            {
                Card card = player.GetHandCard(action.cardUID);
                gameLogic.PlayCard(card, action.slot);
            }

            if (action.type == GameAction.Move)
            {
                Card card = player.GetBoardCard(action.cardUID);
                gameLogic.MoveCard(card, action.slot);
            }

            if (action.type == GameAction.Attack)
            {
                Card card = player.GetBoardCard(action.cardUID);
                Card target = data.GetBoardCard(action.targetUID);
                gameLogic.AttackTarget(card, target);
            }

            if (action.type == GameAction.AttackPlayer)
            {
                Card card = player.GetBoardCard(action.cardUID);
                Player tplayer = data.GetPlayer(action.targetPlayerId);
                gameLogic.AttackPlayer(card, tplayer);
            }

            if (action.type == GameAction.CastAbility)
            {
                Card card = player.GetCard(action.cardUID);
                AbilityData ability = AbilityData.Get(action.abilityID);
                gameLogic.CastAbility(card, ability);
            }

            if (action.type == GameAction.SelectCard)
            {
                Card target = data.GetCard(action.targetUID);
                gameLogic.SelectCard(target);
            }

            if (action.type == GameAction.SelectPlayer)
            {
                Player target = data.GetPlayer(action.targetPlayerId);
                gameLogic.SelectPlayer(target);
            }

            if (action.type == GameAction.SelectSlot)
            {
                gameLogic.SelectSlot(action.slot);
            }

            if (action.type == GameAction.SelectChoice)
            {
                gameLogic.SelectChoice(action.value);
            }

            if (action.type == GameAction.CancelSelect)
            {
                gameLogic.CancelSelection();
            }

            if (action.type == GameAction.EndTurn)
            {
                gameLogic.EndTurn();
            }
        }
        

        private bool HasAction(List<AIAction> list, ushort type)
        {
            return list.Any(t => t.type == type);
        }
        
        //----Return values----

        public bool IsRunning()
        {
            return running;
        }
        
        public string GetNodePath()
        {
            return GetNodePath(firstNode);
        }
        
        public string GetNodePath(NodeState node)
        {
            string path = "Prediction: HValue: " + node.hvalue + "\n";
            NodeState current = node;
            AIAction move;

            while (current != null)
            {
                move = current.lastAction;
                if (move != null)
                    path += "Player " + current.currentPlayer + ": " + move.GetText(originalData) + "\n";
                current = current.bestChild;
            }
            return path;
        }
        
        public void ClearMemory()
        {
            originalData = null;
            firstNode = null;
            bestMove = null;

            foreach (NodeState node in nodePool.GetAllActive())
                node.Clear();
            foreach (AIAction order in actionPool.GetAllActive())
                order.Clear();

            dataPool.DisposeAll();
            nodePool.DisposeAll();
            actionPool.DisposeAll();
            listPool.DisposeAll();

            System.GC.Collect(); //Free memory from AI
        }
        
        public int GetNbNodesCalculated()
        {
            return nbCalculated;
        }

        public int GetDepthReached()
        {
            return reachedDepth;
        }

        public NodeState GetBest()
        {
            return bestMove;
        }

        public NodeState GetFirst()
        {
            return firstNode;
        }

        public AIAction GetBestAction()
        {
            return bestMove?.lastAction;
        }

        public bool IsBestFound()
        {
            return bestMove != null;
        }
        

    }
    
  
    public class NodeState
    {
        public int tdepth;       // 回合深度 - 当前节点在搜索树中的回合数
        public int taction;     // 当前回合动作数 - 在当前回合中已经执行的动作数量
        public int sortMin;    // 排序最小值 - 用于避免重复计算A→B和B→A相同路径
        public int hvalue;      // 启发值 - AI试图最大化，对手试图最小化
        public int alpha;       // Alpha值 - AI玩家可达的最高启发值，用于剪枝
        public int beta;         // Beta值 - 对手玩家可达的最低启发值，用于剪枝

        public AIAction lastAction = null; // 导致此状态的上一个动作
        public int currentPlayer;  // 当前玩家ID

        public NodeState parent;  // 父节点
        public NodeState bestChild = null;  // 最佳子节点（选择的行动）
        public List<NodeState> childs = new List<NodeState>();// 所有可能的子节点

        public NodeState() { }

        public NodeState(NodeState parent, int player_id, int turn_depth, int turn_action, int turn_sort)
        {
            this.parent = parent;
            this.currentPlayer = player_id;
            this.tdepth = turn_depth;
            this.taction = turn_action;
            this.sortMin = turn_sort;
        }

        public void Clear()
        {
            lastAction = null;
            bestChild = null;
            parent = null;
            childs.Clear();
        }
    }
    
    public class AIAction
    {
        public ushort type;

        public string cardUID;
        public string targetUID;
        public int targetPlayerId;
        public string abilityID;
        public Slot slot;
        public int value;

        public int score;           //Score to determine which orders get cut and ignored
        public int sort;            //Orders must be executed in sort order
        public bool valid;          //If false, this order will be ignored

        public AIAction() { }
        public AIAction(ushort t) { type = t; }

        public string GetText(Game data)
        {
            string txt = GameAction.GetString(type);
            Card card = data.GetCard(cardUID);
            Card target = data.GetCard(targetUID);
            if (card != null)
                txt += " card " + card.cardId;
            if (target != null)
                txt += " target " + target.cardId;
            if (slot != Slot.None)
                txt += " slot " + slot.x + "-" + slot.p;
            if (abilityID != null)
                txt += " ability " + abilityID;
            if (value > 0)
                txt += " value " + value;
            return txt;
        }

        public void Clear()
        {
            type = 0;
            valid = false;
            cardUID = null;
            targetUID = null;
            abilityID = null;
            targetPlayerId = -1;
            slot = Slot.None;
            value = -1;
            score = 0;
            sort = 0;
        }

        public static AIAction None { get { AIAction a = new AIAction(); a.type = 0; return a; } }
    }
}