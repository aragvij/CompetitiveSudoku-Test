using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace cSudokuCommonLib
{
    /// <summary>
    /// Класс, нужный только для статического метода получения полного списка сообщений из иерархоческой цепочки исключений (Exception)
    /// </summary>
    public static class MyException
    {
        /// <summary>
        /// Возвращает список всех сообщений исключений (Exception)
        /// </summary>
        /// <param name="ex">Exception, все принадлежащие которому сообщения нужно получить</param>
        /// <returns>Список всех сообщений исключений, разделённый символом "точка с запятой"</returns>
        public static string AllMessages(this Exception ex)
        {
            if (null == ex)
                return null;

            StringBuilder stringBuilder = new StringBuilder();
            while (ex != null)
            {
                stringBuilder.Append(ex.Message);
                ex = ex.InnerException;
                if (ex != null)
                    stringBuilder.Append("; ");
            }
            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// Тип сообщения, которое передаётся от сервера клиенту и обратно
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Запрос уровня текущей игры
        /// </summary>
        None,

        /// <summary>
        /// Сообщение об ошибке на сервере
        /// </summary>
        Error,

        /// <summary>
        /// Начало игры
        /// </summary>
        StartGame,

        /// <summary>
        /// Передача текущего состояния игры
        /// </summary>
        PlayGame,

        /// <summary>
        /// Запрос списка победителей
        /// </summary>
        WinnerList,

        /// <summary>
        /// Передача об очередном ходе игрока
        /// </summary>
        SetValue,

        /// <summary>
        /// Сообщение о том, что игрок покинул чемпионат
        /// </summary>
        OutOfGame,
    }

    /// <summary>
    /// Сообщение для обмена информацией между сервером и клиентом через WebSocket
    /// </summary>
    [Serializable]
    public class SocketMessage
    {
        /// <summary>
        /// Тип сообщения
        /// </summary>
        public MessageType messageType;

        /// <summary>
        /// Список победителей, отсортированный от чемпиона к аутсайдеру
        /// </summary>
        public string[] winnerList;

        /// <summary>
        /// Текстовое поле
        /// </summary>
        public string text;

        /// <summary>
        /// Уровень сложности игры
        /// </summary>
        public SudokuLevel level;

        /// <summary>
        /// Состояние игры
        /// </summary>
        public CSudokuState gameState;

        /// <summary>
        /// Значение ячейки игры
        /// </summary>
        public CSudokuCell cell;

        /// <summary>
        /// Id игрока
        /// </summary>
        public int playerId;

        /// <summary>
        /// Класс, используемый для банарной сериализации
        /// </summary>
        private static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        /// <summary>
        /// Бинарная сериализация этого экземпляра сообщения
        /// </summary>
        /// <returns>Массив байтов, содержащий всю информацию об этом сообщении</returns>
        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                binaryFormatter.Serialize(stream, this);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Бинарная десериализация экземпляра сообщения
        /// </summary>
        /// <param name="bytes">Массив байтов, содержащий информацию о сообщении</param>
        /// <returns>Сообщение "клиент-сервер"</returns>
        public static SocketMessage Deserialize(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                try
                {
                    return (SocketMessage)binaryFormatter.Deserialize(stream);
                }
                catch (Exception ex)
                {
                    // В случае ошибки при десериализации воозвращаем сообщение типа MessageType.Error с описанием этой ошибки
                    return new SocketMessage
                    {
                        messageType = MessageType.Error,
                        text = "SocketMessage.Deserialize: " + ex.AllMessages()
                    };
                }
            }
        }
    }
}
