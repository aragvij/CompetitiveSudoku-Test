using cSudokuCommonLib;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace cSudokuClientLib
{
    /// <summary>
    /// Установка связи с сервером может быть либо с началом игры либо во время запроса списка победителей
    /// </summary>
    public enum TypeOfFirstRequest
    {
        Unknown,
        StartGame,
        WinnerList,
    }

    /// <summary>
    /// Класс, представляющий WebSocket-клиента
    /// </summary>
    public class CsClient : IDisposable
    {
        // Материалы, которые помогли мне при написании этого класса
        // https://metanit.com/sharp/tutorial/12.1.php
        // https://github.com/sta/websocket-sharp

        /// <summary>
        /// Id игрока
        /// </summary>
        private int playerId;

        /// <summary>
        /// Флаг, показывающий, принимаетли этот игрок участие в чемпионате
        /// </summary>
        private bool inGame;

        /// <summary>
        /// Строка подключения к WebSocket=серверу
        /// </summary>
        private Uri serverName;

        /// <summary>
        /// WebSocket для подключения к серверу
        /// </summary>
        private ClientWebSocket clientSocket;

        /// <summary>
        /// Интерфейс отображения состояния игры и управления ей со стороный игрока
        /// </summary>
        private readonly ICSudokuClient visual;

        /// <summary>
        /// Токен для подключения к WebSocket-серверу
        /// </summary>
        private CancellationTokenSource connectCancelToken;

        /// <summary>
        /// Токен для WebSocket входящих сообщений от сервера. Используется для определения разрыва соединения с сервером в процессе игры.
        /// </summary>
        private readonly CancellationTokenSource cancelToken = new CancellationTokenSource();

        /// <summary>
        /// Уровень сложности текущей игры
        /// </summary>
        public SudokuLevel GameLevel { get; set; }

        /// <summary>
        /// Флаг, определяющий, существует ли в настоящий момент WebSocket-соединение с сервером
        /// </summary>
        public bool Connected => clientSocket != null && clientSocket.State == WebSocketState.Open;

        /// <summary>
        /// Конструктор WebSocket-клиента
        /// </summary>
        /// <param name="visualForm">Визуальная часть клиента, через которую пррисходит управление игрой и сам процесс игры</param>
        public CsClient(ICSudokuClient visualForm)
        {
            visual = visualForm;
        }

        /// <summary>
        /// Деструктор WebSocket-клиента
        /// </summary>
        public void Dispose()
        {
            if (clientSocket != null)
            {
                clientSocket.Abort();
                clientSocket.Dispose();
            }

            if (connectCancelToken != null)
            {
                ((IDisposable)connectCancelToken).Dispose();
            }
        }

        /// <summary>
        /// Разрыв соединения с WebSocket-сервером
        /// </summary>
        public void Disconnect()
        {
            connectCancelToken?.Cancel();
        }

        /// <summary>
        /// Попытка соединения с WebSocket-сервером
        /// </summary>
        /// <param name="server">Строка подключения. Например: ws://localhost:80/</param>
        /// <returns>Задача, выполнение которой означает соединение с WebSocket-сервером</returns>
        public async Task TryConnectToServerAsync(string server)
        {
            if (Connected)
            {
                return;
            }

            // Uri, полученный на основании строки подключения
            serverName = new Uri(server);

            // Устанавливаем таймаут попытки подключения 5 секунд
            connectCancelToken = new CancellationTokenSource(5000);

            clientSocket = new ClientWebSocket();
            try
            {
                // Попытка соединения с сервером
                await clientSocket.ConnectAsync(serverName, connectCancelToken.Token);
            }
            catch (Exception ex)
            {
                if (connectCancelToken.IsCancellationRequested)
                {
                    // Запрос был прерван вручную
                    return;
                }

                visual.ShowError(new Exception("Попытка соединения с сервером:\n\n" + ex.AllMessages()));
                return;
            }

            // После установки соединения начинаем принимать входящие сообщения
            ListenAsync();
        }

        /// <summary>
        /// Отсылка на WebSocket-сервер запроса уровня сложности текущей игры
        /// </summary>
        public async void RequestGameState()
        {
            // Запрос уровня текущей игры
            await SendMessage(new SocketMessage
            {
                messageType = MessageType.None
            });
        }

        /// <summary>
        /// Отсылка на WebSocket-сервер запроса списка победителей
        /// </summary>
        public async void GetWinnersAsync()
        {
            await SendMessage(new SocketMessage
            {
                messageType = MessageType.WinnerList
            });
        }

        /// <summary>
        /// Посылка сообщения на WebSocket-сервер с запросом на начало новой игры
        /// </summary>
        /// <param name="level">Выбранный игроком уровень сложности</param>
        /// <param name="playerName">Имя игрока</param>
        public async void NewGameAsync(SudokuLevel level, string playerName)
        {
            GameLevel = level;
            inGame = true;
            await SendMessage(new SocketMessage
            {
                messageType = MessageType.StartGame,
                level = GameLevel,
                text = playerName,
            });
        }

        /// <summary>
        /// Посылка на WebSocket-сервер сообщения, содержащее информацию об очередном ходе игрока
        /// </summary>
        /// <param name="x">Координата X ячейки, в которую игрока ставит своё значение</param>
        /// <param name="y">Координата Y ячейки, в которую игрока ставит своё значение</param>
        /// <param name="num">То значение, которое игрок хочет поставить в ячейку с координатами[x,y]</param>
        public async void TrySetValue(byte x, byte y, byte num)
        {
            if (inGame)
            {
                // Посылаем это соощение, только если пользователь принимает участие в игре

                await SendMessage(new SocketMessage
                {
                    messageType = MessageType.SetValue,
                    cell = new CSudokuCell
                    {
                        Owner = playerId,
                        Value = num,
                        X = x,
                        Y = y
                    }
                });
            }
        }

        /// <summary>
        /// Посылка на WebSocket-сервер сообщения о том, что игрок не хочет играть в игру с имеющимся уровнем сложности, а значит и принимать дальнейшего участия в текущей игре
        /// </summary>
        /// <param name="playerLevel">Уровень сложности, выбранный игроком</param>
        public async void PlayerOutOfGame(SudokuLevel playerLevel)
        {
            // Установка соответствующего флага
            inGame = false;

            // Отказ от получения сообщений
            await SendMessage(new SocketMessage
            {
                messageType = MessageType.OutOfGame,
                level = playerLevel,
            });
        }

        /// <summary>
        /// Посылка на WebSocket-сервер сообщения
        /// </summary>
        /// <param name="message">Сообщение для посылки на WebSocket-сервер</param>
        /// <returns>Задача, которая будет завершена после того, как посылка будет сделана</returns>
        private async Task SendMessage(SocketMessage message)
        {
            // Сериализация сообщения
            byte[] msgBytes = message.Serialize();
            ArraySegment<byte> arrSeg = new ArraySegment<byte>(msgBytes);

            try
            {
                // Собственно посылка информации на WebSocket-сервер
                await clientSocket.SendAsync(arrSeg, WebSocketMessageType.Binary, false, (new CancellationTokenSource()).Token);
            }
            catch (Exception ex)
            {
                // В случае неудачи - показ сообщения об ошибке
                visual.ShowError(new Exception("(SendMessage): " + ex.AllMessages()));
            }
        }

        /// <summary>
        /// Приём приходящих с WebSocket-сервера сообщений
        /// </summary>
        private async void ListenAsync()
        {
            // Буфер для принимаемого сообщения
            byte[] msgBytes = new byte[0x1000];
            ArraySegment<byte> arrSeg = new ArraySegment<byte>(msgBytes);

            // Экземпляр класса для приёма сообщения
            WebSocketReceiveResult result;

            while (true)
            {
                try
                {
                    // В этой точке происходит рпиём собщения с WebSocket-сервера
                    result = await clientSocket.ReceiveAsync(arrSeg, cancelToken.Token);
                }
                catch (Exception ex)
                {
                    if (!cancelToken.Token.IsCancellationRequested)
                        // В случае ошибки при приёме, сообщаем об этом пользователю, если только это не запрошенное им самим закрытие соединения с WebSocket-сервером
                        visual.ShowError(ex);

                    // Заканчиваем ожидание входящих сообщений
                    return;
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    // Перенос принятого сообщения в MemoryStream
                    stream.Write(arrSeg.Array, 0, result.Count);

                    // Десериализация сообщения и передача его на обработку
                    ProcessingMessage(SocketMessage.Deserialize(stream.ToArray()));
                }
            }
        }

        /// <summary>
        /// Обработка сообщений, полученных от WebSocket-сервера
        /// </summary>
        /// <param name="inMessage">Принятое сообщение</param>
        private void ProcessingMessage(SocketMessage inMessage)
        {
            switch (inMessage.messageType)
            {
                case MessageType.StartGame:
                    // Начало игры

                    visual.StartNewGame(inMessage.level);
                    return;

                case MessageType.Error:
                    // Сообщение об ошибке

                    visual.ShowError(new Exception(inMessage.text));
                    return;

                case MessageType.WinnerList:
                    // Вывод списка победителей

                    visual.ShowWinnerList(inMessage.winnerList);
                    return;

                case MessageType.PlayGame:
                    // Состояние новой игры, либо очистка поля для вышедшего из игры игрока

                    // Отображение игрового поля
                    visual.SetSudokuState(inMessage.gameState);

                    if (inGame)
                    {
                        // Здесь же происходит присваивание Id этому игроку
                        playerId = inMessage.playerId;
                    }
                    return;

                case MessageType.SetValue:
                    // Состояние игрового поля после очередного хода

                    if (!inGame)
                    {
                        // Если игрок вне игры, ничего не делаем
                        return;
                    }

                    // Отображение игрового поля
                    visual.SetSudokuState(inMessage.gameState);

                    if (inMessage.playerId != CSudokuCell.ServerId)
                    {
                        // Определён победитель, конец игры
                        visual.ShowMessage($"Победил игрок '{inMessage.text}'");
                        visual.EndOfGame();
                    }
                    return;

                default:
                    visual.ShowMessage($"(ProcessingMessage) Не реализована обработка message.messageType '{inMessage.messageType}'");
                    return;
            }
        }
    }
}
