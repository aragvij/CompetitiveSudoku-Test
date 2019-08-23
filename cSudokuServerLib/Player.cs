using System.Net.WebSockets;

namespace cSudokuServerLib
{
    internal class Player
    {
        internal string Name;
        internal int Id { get; }
        internal bool InGame { get; set; }

        public WebSocket WebSocket { get; }
        public int CountWin { get; set; } = 0;
        public string StringForWinnerList => $"У игрока {Name} побед: {CountWin}";
        public bool WebSocketIsOpen => null != WebSocket && WebSocket.State != WebSocketState.Open;

        public Player(WebSocket serverSocket, string playerName, int id)
        {
            WebSocket = serverSocket;
            Name = playerName;
            Id = id;
            InGame = true;
        }

        internal static int ComparisonWinner(Player p1, Player p2) => p2.CountWin - p1.CountWin;
    }
}
