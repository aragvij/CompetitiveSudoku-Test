using cSudokuCommonLib;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace cSudokuClientLib
{
    public enum TypeOfFirstRequest
    {
        Unknown,
        StartGame,
        WinnerList,
    }

    public class CsClient : IDisposable
    {
        // https://metanit.com/sharp/tutorial/12.1.php
        // https://github.com/sta/websocket-sharp

        private int playerId;
        private bool inGame;
        private Uri serverName;
        private ClientWebSocket clientSocket;
        private readonly ICSudokuClient visual;
        private CancellationTokenSource connectCancelToken;
        private readonly CancellationTokenSource cancelToken = new CancellationTokenSource();

        public SudokuLevel GameLevel { get; set; }
        public bool Connected => clientSocket != null && clientSocket.State == WebSocketState.Open;

        public CsClient(ICSudokuClient visualForm)
        {
            visual = visualForm;
        }
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
        public void Disconnect()
        {
            connectCancelToken?.Cancel();
        }
        public async Task TryConnectToServerAsync(string server)
        {
            if (Connected)
            {
                return;
            }

            serverName = new Uri(server);
            connectCancelToken = new CancellationTokenSource(5000);
            clientSocket = new ClientWebSocket();
            try
            {
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
        public async void RequestGameState()
        {
            await SendMessage(new SocketMessage
            {
                messageType = MessageType.None
            });
        }
        public async void GetWinnersAsync()
        {
            await SendMessage(new SocketMessage
            {
                messageType = MessageType.WinnerList
            });
        }
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
        public async void TrySetValue(byte x, byte y, byte num)
        {
            // если пользователь в игре
            if (inGame)
            {
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
        public async void PlayerOutOfGame(SudokuLevel playerLevel)
        {
            inGame = false;
            // отказ от получения сообщений
            await SendMessage(new SocketMessage
            {
                messageType = MessageType.OutOfGame,
                level = playerLevel,
            });
        }

        private async Task SendMessage(SocketMessage message)
        {
            byte[] msgBytes = message.Serialize();
            ArraySegment<byte> arrSeg = new ArraySegment<byte>(msgBytes);

            try
            {
                await clientSocket.SendAsync(arrSeg, WebSocketMessageType.Binary, false, (new CancellationTokenSource()).Token);
            }
            catch (Exception ex)
            {
                visual.ShowError(new Exception("(SendMessage): " + ex.AllMessages()));
            }
        }
        private async void ListenAsync()
        {
            byte[] msgBytes = new byte[0x1000];
            ArraySegment<byte> arrSeg = new ArraySegment<byte>(msgBytes);
            WebSocketReceiveResult result;

            while (true)
            {
                try
                {
                    result = await clientSocket.ReceiveAsync(arrSeg, cancelToken.Token);
                }
                catch (Exception ex)
                {
                    if (!cancelToken.Token.IsCancellationRequested)
                        visual.ShowError(ex);
                    return;
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(arrSeg.Array, 0, result.Count);
                    ProcessingMessage(SocketMessage.Deserialize(stream.ToArray()));
                }
            }
        }
        private void ProcessingMessage(SocketMessage inMessage)
        {
            switch (inMessage.messageType)
            {
                case MessageType.StartGame:
                    visual.StartNewGame(inMessage.level);
                    return;

                case MessageType.Error:
                    visual.ShowError(new Exception(inMessage.text));
                    return;

                case MessageType.WinnerList:
                    visual.ShowWinnerList(inMessage.winnerList);
                    return;

                case MessageType.PlayGame:
                    visual.SetSudokuState(inMessage.gameState);
                    if (inGame)
                    {
                        playerId = inMessage.playerId;
                    }
                    return;

                case MessageType.SetValue:
                    if (!inGame)
                    {
                        return;
                    }

                    visual.SetSudokuState(inMessage.gameState);
                    if (inMessage.playerId != CSudokuCell.ServerId)
                    {
                        // определён победитель, конец игры
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
