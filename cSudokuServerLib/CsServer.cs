using cSudokuCommonLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cSudokuServerLib
{
    /// <summary>
    /// Передаёт строку для её возможного отображения
    /// </summary>
    /// <param name="TextMessage">Строка</param>
    public delegate void SendString(string TextMessage);

    /// <summary>
    /// Передаёт набор строк для их возможного отображения
    /// </summary>
    /// <param name="TextList">Список строк</param>
    public delegate void SendStringList(List<string> TextList);

    /// <summary>
    /// Класс, реализующий функции серверной части для игры в "конкурентное судоку"
    /// </summary>
    public class CsServer : IDisposable
    {
        /* Те информационные ресурсы, которыми я пользовался при написании этого задания
         * https://docs.microsoft.com/ru-ru/dotnet/api/system.net.websockets?view=netframework-4.7.1
         * https://docs.microsoft.com/ru-ru/dotnet/api/system.net.websockets.clientwebsocket.connectasync?view=netframework-4.8
         * https://docs.microsoft.com/ru-ru/dotnet/api/system.net.websockets.websocket.receiveasync?view=netframework-4.8
         * http://www.cyberforum.ru/csharp-beginners/thread2159391.html
         * http://www.cyberforum.ru/csharp-beginners/thread1819503.html#post9649401
         * https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server
         * https://habr.com/ru/sandbox/23231/
         * http://qaru.site/questions/18214/how-to-get-the-ip-address-of-the-server-on-which-my-c-application-is-running-on
         * 
         * https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/configuring-http-and-https
         * https://docs.microsoft.com/ru-ru/dotnet/api/system.net.httplistenerexception
         * http://qaru.site/questions/52284/httplistener-access-denied
         * 
         */

        /// <summary>
        /// "Слушатель" указанного IP-порта, для подключения клиентской части
        /// </summary>
        private readonly HttpListener listener;

        /// <summary>
        /// Токен для прерывания работы веб-сокета. Например, при окончании работы сервера
        /// </summary>
        private readonly CancellationTokenSource cancelWaitToken = new CancellationTokenSource();

        /// <summary>
        /// Список игроков
        /// </summary>
        private readonly PlayerList playerList;

        /// <summary>
        /// Метод для тотбражения списка игроков. В задании этого нет, но он показался мне полезным
        /// </summary>
        private readonly SendStringList ShowPlayerList;

        /// <summary>
        /// Метод отображения сообщений об ошибках
        /// </summary>
        private readonly SendString ShowError;

        /// <summary>
        /// Экземпляр класса, реализующего игровую логику
        /// </summary>
        private GameSudoku game;

        /// <summary>
        /// Конструктор сервера
        /// </summary>
        /// <param name="port">IP-порт, который сервер должен "прослушивать" для подключения клиентов</param>
        /// <param name="addresses">Набор IP-адресов, или доменных имён, по которым происходит работа сервера</param>
        /// <param name="showErrorOnServer">Делегат метода, отображающего сообщение об ошибке. Он может, например, записывать информацию в лог-файл, или отображать её на экране. Или посылать SMS-сообщение</param>
        /// <param name="showPlayerList">Метод отображения списка игроков</param>
        public CsServer(int port, string[] addresses, SendString showErrorOnServer, SendStringList showPlayerList)
        {
            ShowError = showErrorOnServer;
            ShowPlayerList = showPlayerList;

            playerList = new PlayerList();
            listener = new HttpListener();

            // Регистрация в ОС адресов и порта для "прослушивания"
            foreach (string address in addresses)
            {
                string prefix = $"http://{address}:{port}/";
                listener.Prefixes.Add(prefix);

                Process.Start(new ProcessStartInfo("netsh", $"http add urlacl url={prefix} user={Environment.MachineName}\\{Environment.UserName}")
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas"
                });
            }
        }

        /// <summary>
        /// Получение имён и адресов, имеющихся у данного компьютера. Плюс имя "localhost".
        /// </summary>
        /// <returns>Список адресов</returns>
        public static string[] GetAddressList()
        {
            List<string> list = new List<string>();
            list.Add("localhost");

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            list.Add(host.HostName);

            foreach (IPAddress ip in host.AddressList)
            {
                list.Add(ip.ToString());
            }
            return list.ToArray();
        }

        /// <summary>
        /// Начало работы "слушателя"
        /// </summary>
        public void Start()
        {
            ListenAsync();
        }

        /// <summary>
        /// Окончание работы "слушателя" и его закрытие
        /// </summary>
        public void Dispose()
        {
            listener?.Abort();
            listener?.Close();
        }

        /// <summary>
        /// Запуск "слушателя" в работы
        /// </summary>
        private async void ListenAsync()
        {
            if (null == listener)
                return;

            try
            {
                listener.Start();

                while (listener != null && listener.IsListening)
                {
                    HttpListenerContext context = null;
                    try
                    {
                        // Точка, на которой происходит "прослушивание"
                        context = await listener.GetContextAsync();
                    }
                    catch
                    {
                        // Происходит при остановке сервера. Не нашел, как штатно остановить HttpListener.GetContextAsync()
                        return;
                    }

                    if (context.Request.IsWebSocketRequest)
                    {
                        // Пришедший запрос является запросом WebSocket
                        var webSocketContext = await context.AcceptWebSocketAsync(null);

                        // Ожидание входящего сообщения по установленному каналу
                        WaitIncomeMessageAsync(webSocketContext.WebSocket);
                    }
                    else
                    {
                        // Пришедший запрос - не типа WebSocket, например от браузера. Возвращаем в ответ текст с ошибкой
                        string responseString = "<HTML><BODY>The request must be of type WebSocket!</BODY></HTML>";
                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                        context.Response.ContentLength64 = buffer.Length;
                        System.IO.Stream output = context.Response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("csServer.Listen(): " + ex.AllMessages());
            }
        }

        /// <summary>
        /// Ожидание входящих сообщений через клиентский WebSocket
        /// </summary>
        /// <param name="clientSocket">Тот WebSocket, который ожидает входящих сообщений</param>
        private async void WaitIncomeMessageAsync(WebSocket clientSocket)
        {
            while (clientSocket.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[0x1000]);
                WebSocketReceiveResult receive;
                try
                {
                    // Точка ожидания входящих сообщений
                    receive = await clientSocket.ReceiveAsync(buffer, cancelWaitToken.Token);
                }
                catch (Exception ex)
                {
                    // Обработка ошибок работы WebSocket

                    if (ex is WebSocketException webEx)
                    {
                        if (webEx.HResult == -2147467259 &&
                            webEx.InnerException != null &&
                            webEx.InnerException.HResult == -2147467259)
                        {
                            // Один  из игроков вышел. Удаляем его из списка
                            playerList.DeleteFreeSocket();

                            // Если ни одного игрока не осталось, закрываем текущую игру
                            TestForEmptyPlayerList();

                            // Отображение изменившегося списка игроков
                            ShowPlayerList(playerList.GetPlayerList());
                            continue;
                        }
                    }

                    if (!cancelWaitToken.IsCancellationRequested)
                    {
                        // Если исключение вызвано не остановкой работы, показываем сообщение об ошибке
                        ShowError(ex.AllMessages());
                    }

                    // Работа прервана посредством cancelToken. Просто выход из цикла и метода.
                    return;
                }

                if (receive.MessageType == WebSocketMessageType.Close)
                {
                    // При закрытии WebSocket прекращаем его прослушивание.
                    return;
                }

                // Все сообщения в рамках игры передаются в сериализованном при помощи BinaryFormatter виде, то есть сообщение не бинарного вида нас не интересует.
                if (receive.MessageType != WebSocketMessageType.Binary)
                {
                    // Просто не обрабатываем запросы, не относящиеся к судоку
                    continue;
                }

                // Обработка пришедшего сообщения
                await ProcessingMessage(clientSocket, SocketMessage.Deserialize(buffer.ToArray()));
            }
        }

        /// <summary>
        /// Обработка пришедшего сообщения
        /// </summary>
        /// <param name="clentSocket">Клиентский WebSocket, через который пришло сообщение</param>
        /// <param name="inMessage">Пришедшее сообщение</param>
        /// <returns>Приём собщений через этот сокет приостанавливается до обработки одного сообщения</returns>
        private async Task ProcessingMessage(WebSocket clentSocket, SocketMessage inMessage)
        {
            switch (inMessage.messageType)
            {
                case MessageType.None:
                    // Запрос уровня текущей игры

                    await SendMessage(clentSocket, new SocketMessage
                    {
                        messageType = MessageType.StartGame,
                        level = null == game
                         ? SudokuLevel.Unknown
                         : game.GameLevel
                    });
                    break;

                case MessageType.WinnerList:
                    // Запрос списка победителей

                    await SendMessage(clentSocket, new SocketMessage
                    {
                        messageType = MessageType.WinnerList,
                        winnerList = playerList.GetWinnerList()
                    });
                    break;

                case MessageType.StartGame:
                    // Начало игры

                    // Попытка добавить игрока в список
                    if (playerList.Add(clentSocket, inMessage.text, out string errorMessage, out int playerId))
                    {
                        // Если игрок, от которого пришел этот запрос, новый, то добавляем его в список игроков
                        TestForEmptyPlayerList();
                        ShowPlayerList(playerList.GetPlayerList());
                    }
                    else
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        // Если добавление игрока не произошло удачно, игру не начинаем. Выводим сообщение об ошибке.
                        await SendMessage(clentSocket, new SocketMessage
                        {
                            messageType = MessageType.Error,
                            text = errorMessage,
                        });
                        break;
                    }

                    if (null == game)
                    {
                        // Начало новой игры с тем уровнем, который указал первый пришедший игрок
                        game = new GameSudoku(inMessage.level);
                    }

                    // Отсылка игроку состояния игры и присваивание ему Id
                    await SendMessage(clentSocket, new SocketMessage
                    {
                        messageType = MessageType.PlayGame,
                        gameState = game.GetGameState(),
                        playerId = playerId,
                    });
                    break;

                case MessageType.OutOfGame:
                    // Игрок отказался играть на том уровне, на котором игра уже идёт, если она в данный момент идёт

                    if (null == game)
                    {
                        return;
                    }

                    // Игрок вышел из турнира, помечаем его соответственно
                    playerList.SetOutOfGame(clentSocket);

                    // Посылаем игроку пустое поле
                    await SendMessage(clentSocket, new SocketMessage
                    {
                        messageType = MessageType.PlayGame,
                        gameState = game.GetGameState(false),
                    });
                    break;

                case MessageType.SetValue:
                    // Обработка одного хода игрока

                    if (null == game)
                    {
                        return;
                    }

                    if (game.TrySetValue(inMessage.cell, out int? winner))
                    {
                        // Если попытка совершить ход удачна...

                        // Формируем сообщение с новым состоянием игры
                        var message = new SocketMessage
                        {
                            messageType = MessageType.SetValue,
                            gameState = game.GetGameState(),
                        };

                        if (null != winner)
                        {
                            // Если игра окончена, то сообщаем всем Id и имя победившего игрока

                            int winnerId = (int)winner;

                            // Поиск в списке игрока с Id победителя
                            Player playerWin = playerList.GetPlayerById(winnerId);
                            if (null == playerWin)
                            {
                                ShowError($"В списке игроков не найден игрок с Id={winnerId}");
                                return;
                            }

                            // Помещение в сообщение значений: Имя и Id
                            message.text = playerWin.Name;
                            message.playerId = winnerId;

                            // Количество побед у победителя увеличиваем на 1
                            playerWin.CountWin++;

                            // Игра окончена
                            game = null;
                        }

                        // Если поставить значение удалось, то сообщаем об этом всем игрокам
                        foreach (WebSocket socket in playerList.GetActiveSocketList())
                        {
                            await SendMessage(socket, message);
                        }
                    }

                    // Если поставить значение в ячейку не удалось, то ничего не делаем
                    break;

                default:
                    ShowError($"Не реализована обработка socketMessage.messageType '{inMessage.messageType}'");
                    return;
            }
        }

        /// <summary>
        /// Проверка количества игроков: если их нет, то игра заканчивается
        /// </summary>
        private void TestForEmptyPlayerList()
        {
            if (playerList.Count == 0)
            {
                game = null;
            }
        }

        /// <summary>
        /// Отсылка сообщения клиенту
        /// </summary>
        /// <param name="socket">WebSocket клиента</param>
        /// <param name="message">Отсылаемое сообщение</param>
        /// <returns>До продолжения работы с этим WebSocket следует дождаться, пока сообщение будет отослано</returns>
        private async Task SendMessage(WebSocket socket, SocketMessage message)
        {
            // Бинарная сериализация сообщения
            var msgBytes = message.Serialize();
            var arrSeg = new ArraySegment<byte>(msgBytes);

            try
            {
                // Отсылка сообщения
                await socket.SendAsync(arrSeg, WebSocketMessageType.Binary, false, (new CancellationTokenSource()).Token);
            }
            catch (Exception ex)
            {
                ShowError("(SendMessage): " + ex.AllMessages());
            }
        }

    }
}