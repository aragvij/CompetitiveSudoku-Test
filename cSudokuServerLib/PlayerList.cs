using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;

namespace cSudokuServerLib
{
    internal class PlayerList
    {
        private const int MaxPlayers = int.MaxValue;

        private readonly List<Player> playerList = new List<Player>();
        public int Count => playerList.Count;

        public string[] GetWinnerList()
        {
            playerList.Sort(Player.ComparisonWinner);
            return playerList.Select(p => p.StringForWinnerList).ToArray();
        }
        public bool Add(WebSocket socket, string playerName, out string errorMessage, out int playerId)
        {
            errorMessage = null;

            Player player = playerList.FirstOrDefault(p => p.WebSocket == socket);
            if (null == player)
                lock (playerList)
                {
                    int maxId = playerList.Count > 0 ? playerList.Max(p => p.Id) : 0;
                    if (maxId == MaxPlayers)
                    {
                        errorMessage = "Количество участников турнира достигло максимума!";
                        playerId = -1;
                        return false;
                    }
                    playerId = maxId + 1;
                    player = new Player(socket, playerName, playerId);
                    playerList.Add(player);
                    return true;
                }
            playerId = player.Id; ;
            return false;
        }
        public IEnumerable<WebSocket> GetActiveSocketList()
        {
            List<WebSocket> sList = new List<WebSocket>();
            foreach (Player player in playerList)
            {
                if (player.InGame)
                {
                    sList.Add(player.WebSocket);
                }
            }
            return sList;
        }
        public Player GetPlayerById(int Id)
        {
            return playerList.FirstOrDefault(p => p.Id == Id);
        }
        public void DeleteFreeSocket()
        {
            Player freePlayer = playerList.FirstOrDefault(p => p.WebSocketIsOpen);
            playerList.Remove(freePlayer);
        }
        public List<string> GetPlayerList()
        {
            return playerList.Select(p => p.Name).ToList();
        }
        public void SetOutOfGame(WebSocket socket)
        {
            Player player = playerList.FirstOrDefault(p => p.WebSocket == socket);
            if (null != player)
            {
                player.InGame = false;
            }
        }
    }
}
