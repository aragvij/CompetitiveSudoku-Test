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
    public delegate void SendString(string TextMessage);
    public delegate void SendStringList(List<string> TextList);

    public class CsServer : IDisposable
    {
        /*
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

        private readonly HttpListener listener;
        private readonly CancellationTokenSource cancelWaitToken = new CancellationTokenSource();
        private readonly PlayerList playerList;
        private readonly SendStringList ShowPlayerList;
        private readonly SendString ShowError;
        private GameSudoku game;

        public CsServer(int port, string[] addresses, SendString showErrorOnServer, SendStringList showPlayerList)
        {
            ShowError = showErrorOnServer;
            ShowPlayerList = showPlayerList;

            playerList = new PlayerList();
            listener = new HttpListener();

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
        public void Start()
        {
            ListenAsync();
        }
        public void Dispose()
        {
            listener?.Abort();
            listener?.Close();
        }

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
                        context = await listener.GetContextAsync();
                    }
                    catch
                    {
                        // Происходит при остановке сервера. Не нашел, как штатно остановить HttpListener.GetContextAsync()
                        return;
                    }

                    if (context.Request.IsWebSocketRequest)
                    {
                        var webSocketContext = await context.AcceptWebSocketAsync(null);
                        WaitIncomeMessageAsync(webSocketContext.WebSocket);
                    }
                    else
                    {
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
        private async void WaitIncomeMessageAsync(WebSocket clientSocket)
        {
            while (clientSocket.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[0x1000]);
                WebSocketReceiveResult receive;
                try
                {
                    receive = await clientSocket.ReceiveAsync(buffer, cancelWaitToken.Token);
                }
                catch (Exception ex)
                {
                    if (ex is WebSocketException webEx)
                    {
                        if (webEx.HResult == -2147467259 &&
                            webEx.InnerException != null &&
                            webEx.InnerException.HResult == -2147467259)
                        {
                            // Один  из игроков вышел
                            playerList.DeleteFreeSocket();
                            TestForEmptyPlayerList();
                            ShowPlayerList(playerList.GetPlayerList());
                            continue;
                        }
                    }

                    if (!cancelWaitToken.IsCancellationRequested)
                    {
                        ShowError(ex.AllMessages());
                    }

                    // работа прервана посредством cancelToken
                    return;
                }

                if (receive.MessageType == WebSocketMessageType.Close)
                {
                    return;
                }

                if (receive.MessageType != WebSocketMessageType.Binary)
                {
                    // Просто не обрабатываем запросы, не относящиеся к судоку
                    continue;
                }

                await ProcessingMessage(clientSocket, SocketMessage.Deserialize(buffer.ToArray()));
            }
        }
        private async Task ProcessingMessage(WebSocket clentSocket, SocketMessage inMessage)
        {
            switch (inMessage.messageType)
            {
                case MessageType.None:
                    await SendMessage(clentSocket, new SocketMessage
                    {
                        messageType = MessageType.StartGame,
                        level = null == game
                         ? SudokuLevel.Unknown
                         : game.GameLevel
                    });
                    break;

                case MessageType.WinnerList:
                    await SendMessage(clentSocket, new SocketMessage
                    {
                        messageType = MessageType.WinnerList,
                        winnerList = playerList.GetWinnerList()
                    });
                    break;

                case MessageType.StartGame:
                    if (playerList.Add(clentSocket, inMessage.text, out string errorMessage, out int playerId))
                    {
                        TestForEmptyPlayerList();
                        ShowPlayerList(playerList.GetPlayerList());
                    }
                    else
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        // если ошибка, игру не начинаем
                        await SendMessage(clentSocket, new SocketMessage
                        {
                            messageType = MessageType.Error,
                            text = errorMessage,
                        });
                        break;
                    }

                    if (null == game)
                    {
                        // начало новой игры
                        game = new GameSudoku(inMessage.level);
                    }

                    await SendMessage(clentSocket, new SocketMessage
                    {
                        messageType = MessageType.PlayGame,
                        gameState = game.GetGameState(),
                        playerId = playerId,
                    });
                    break;

                case MessageType.OutOfGame:
                    if (null == game)
                    {
                        return;
                    }

                    // игрок вышел из турнира
                    playerList.SetOutOfGame(clentSocket);
                    // посылаем игроку пустое поле
                    await SendMessage(clentSocket, new SocketMessage
                    {
                        messageType = MessageType.PlayGame,
                        gameState = game.GetGameState(false),
                    });
                    break;

                case MessageType.SetValue:
                    if (null == game)
                    {
                        return;
                    }

                    if (game.TrySetValue(inMessage.cell, out int? winner))
                    {
                        var message = new SocketMessage
                        {
                            messageType = MessageType.SetValue,
                            gameState = game.GetGameState(),
                        };

                        if (null != winner)
                        {
                            // Если игра окончена, то сообщаем всем Id и имя победившего игрока
                            int winnerId = (int)winner;

                            Player playerWin = playerList.GetPlayerById(winnerId);
                            if (null == playerWin)
                            {
                                ShowError($"В списке игроков не найден игрок с Id={winnerId}");
                                return;
                            }
                            message.text = playerWin.Name;
                            message.playerId = winnerId;

                            // количество побед у победителя увеличиваем на 1
                            playerWin.CountWin++;

                            // новая игра
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
        private void TestForEmptyPlayerList()
        {
            if (playerList.Count == 0)
            {
                // заканчиваем игру, если не осталось игроков
                game = null;
            }
        }
        private async Task SendMessage(WebSocket socket, SocketMessage message)
        {
            var msgBytes = message.Serialize();
            var arrSeg = new ArraySegment<byte>(msgBytes);

            try
            {
                await socket.SendAsync(arrSeg, WebSocketMessageType.Binary, false, (new CancellationTokenSource()).Token);
            }
            catch (Exception ex)
            {
                ShowError("(SendMessage): " + ex.AllMessages());
            }
        }

    }
}