using GameClient;
using GameLogic;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// History bar shows all the previous moved perform by a player this turn
    /// </summary>
    public class TurnHistoryBar: MonoBehaviour
    {
        public bool isOpponent;
        public TurnHistoryLine[] historyLines;
        
        void Start()
        {

        }
        
        void Update()
        {
            if (!Gameclient.Get().IsReady())
                return;

            int playerID = isOpponent ? Gameclient.Get().GetOpponentPlayerID() : Gameclient.Get().GetPlayerID();
            Game data = Gameclient.Get().GetGameData();
            Player player = data.GetPlayer(playerID);

            if (player != null && player.historyList != null)
            {
                int index = 0;
                foreach (ActionHistory order in player.historyList)
                {
                    if (index < historyLines.Length)
                    {
                        historyLines[index].SetLine(order);
                        index++;
                    }
                }

                while (index < historyLines.Length)
                {
                    historyLines[index].Hide();
                    index++;
                }
            }
        }
    }
}