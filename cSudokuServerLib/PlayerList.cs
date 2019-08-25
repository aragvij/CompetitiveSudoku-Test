using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;

namespace cSudokuServerLib
{
    /// <summary>
    /// Список игроков
    /// </summary>
    internal class PlayerList
    {
        /// <summary>
        /// Максимально допустимое количество игроков
        /// </summary>
        private const int MaxPlayers = int.MaxValue;

        /// <summary>
        /// Динамически изменяющийся список игроков
        /// </summary>
        private readonly List<Player> playerList = new List<Player>();

        /// <summary>
        /// Текущее количество игроков
        /// </summary>
        public int Count => playerList.Count;

        /// <summary>
        /// Получение списка игроков для таблицы рекордов
        /// </summary>
        /// <returns>Набор строк, содержащих информацию про одного игрока</returns>
        public string[] GetWinnerList()
        {
            // Сортировка текущего списка по убыванию количества побед
            playerList.Sort(Player.ComparisonWinner);

            // Формирование списка
            return playerList.Select(p => p.StringForWinnerList).ToArray();
        }

        /// <summary>
        /// Добавление игрока в список
        /// </summary>
        /// <param name="socket">WebSocket добавляемого игрока</param>
        /// <param name="playerName">Имя игрока</param>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        /// <param name="playerId">Id нового игрока</param>
        /// <returns>true, если игрок добавлен в список, иначе false</returns>
        public bool Add(WebSocket socket, string playerName, out string errorMessage, out int playerId)
        {
            errorMessage = null;

            // Поиск игрока в исписке по его WebSocket
            Player player = playerList.FirstOrDefault(p => p.WebSocket == socket);
            if (null == player)
                lock (playerList)
                {
                    // Игрок не найден, добавляем его в список в монопольном режиме

                    // Определение Id нового игрока
                    int maxId = playerList.Count > 0 ? playerList.Max(p => p.Id) : 0;
                    if (maxId == MaxPlayers)
                    {
                        errorMessage = "Количество участников турнира достигло максимума!";
                        playerId = -1;
                        return false;
                    }
                    playerId = maxId + 1;

                    // Создание экземпляра класса игрока
                    player = new Player(socket, playerName, playerId);
                    playerList.Add(player);

                    return true;
                }

            playerId = player.Id; ;
            return false;
        }

        /// <summary>
        /// Возврат списка WebSocket игроков, которые находятся в данный момент в игре
        /// </summary>
        /// <returns>Список WebSocket</returns>
        public IEnumerable<WebSocket> GetActiveSocketList()
        {
            List<WebSocket> sList = new List<WebSocket>();
            foreach (Player player in playerList)
            {
                if (player.InGame)
                {
                    // Добавляем в выдаваемый список только активных игроков
                    sList.Add(player.WebSocket);
                }
            }
            return sList;
        }

        /// <summary>
        /// Определение игрока по его Id
        /// </summary>
        /// <param name="Id">Id игрока</param>
        /// <returns>Игрок с требуемым Id</returns>
        public Player GetPlayerById(int Id)
        {
            return playerList.FirstOrDefault(p => p.Id == Id);
        }

        /// <summary>
        /// Удаление из списка игроков, связь с которыми через WebSocket отсутствует
        /// </summary>
        public void DeleteFreeSocket()
        {
            Player freePlayer = playerList.FirstOrDefault(p => p.WebSocketIsOpen);
            playerList.Remove(freePlayer);
        }

        /// <summary>
        /// Получение списка имён всех игроков
        /// </summary>
        /// <returns>Список имён всех игроков</returns>
        public List<string> GetPlayerList()
        {
            return playerList.Select(p => p.Name).ToList();
        }

        /// <summary>
        /// Пометка игрока выбывшим из турнира
        /// </summary>
        /// <param name="socket">WebSocket отказавшегося играть игрока</param>
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
