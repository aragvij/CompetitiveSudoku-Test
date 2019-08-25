using System.Net.WebSockets;

namespace cSudokuServerLib
{
    /// <summary>
    /// Класс, представляющий игрока
    /// </summary>
    internal class Player
    {
        /// <summary>
        /// Имя игрока
        /// </summary>
        internal string Name;

        /// <summary>
        /// Id игрока
        /// </summary>
        internal int Id { get; }

        /// <summary>
        /// Признак того, что игрок участвует в игре
        /// </summary>
        internal bool InGame { get; set; }

        /// <summary>
        /// WebSocket клиентской части, относящейся к этому игроку
        /// </summary>
        public WebSocket WebSocket { get; }

        /// <summary>
        /// Счётчик побед
        /// </summary>
        public int CountWin { get; set; } = 0;

        /// <summary>
        /// Строка для списка победителей
        /// </summary>
        public string StringForWinnerList => $"У игрока {Name} побед: {CountWin}";

        /// <summary>
        /// Признак установленного WebSocket-подключения к серверу
        /// </summary>
        public bool WebSocketIsOpen => null != WebSocket && WebSocket.State != WebSocketState.Open;

        /// <summary>
        /// Конструктор экземпляра класса игрока
        /// </summary>
        /// <param name="serverSocket">WebSocket игрока</param>
        /// <param name="playerName">Имя, которе игрок себе выбрал</param>
        /// <param name="id">Id игрока, назначается сервером</param>
        public Player(WebSocket serverSocket, string playerName, int id)
        {
            WebSocket = serverSocket;
            Name = playerName;
            Id = id;
            InGame = true;
        }

        /// <summary>
        /// Сравнение для сортировки списка победителей
        /// </summary>
        /// <param name="p1">Первый сравниваемый игрок</param>
        /// <param name="p2">Второй сравниваемый игрок</param>
        /// <returns>Сравнение игроков по количествам их побед</returns>
        internal static int ComparisonWinner(Player p1, Player p2) => p2.CountWin - p1.CountWin;
    }
}
