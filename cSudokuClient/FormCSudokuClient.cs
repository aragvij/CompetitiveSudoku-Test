using cSudokuClientLib;
using cSudokuCommonLib;
using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cSudokuClient
{
    public partial class FormCSudokuClient : Form, ICSudokuClient
    {
        const int ShowFormConnectInterval = 500;
        const string CellName = "cell";
        const string SquareName = "square";

        CsClient csClient;
        Timer timerShowConnect;
        FormConnect formConnect;
        FormCellSudoku selectedCell;

        private string ServerName => textBoxServer.Text.Trim();
        private string PlayerName => textBoxPlayerName.Text.Trim();
        private bool IsServerNameLengthNotEmpty => ServerName.Length > 0;
        private bool IsPlayerNameLengthNotEmpty => PlayerName.Length > 0;

        private FormCellSudoku SelectedCell
        {
            get => selectedCell;
            set
            {
                if (null != selectedCell)
                {
                    selectedCell.Selected = false;
                }

                selectedCell = value;
                selectedCell.Selected = true;
            }
        }

        public FormCSudokuClient()
        {
            InitializeComponent();
        }
        public void StartNewGame(SudokuLevel level)
        {
            bool enableStartButton = false;
            try
            {
                if (level == SudokuLevel.Unknown)
                {
                    // Выбор уровня сложности игры
                    FormSelectLevel formSelectLevel = new FormSelectLevel();
                    if (formSelectLevel.ShowDialog() == DialogResult.OK)
                    {
                        csClient.NewGameAsync(formSelectLevel.Level, PlayerName);
                        return;
                    }
                    else
                    {
                        enableStartButton = true;
                    }
                }
                else
                {
                    if (MessageBox.Show($"Уже идёт игра с уровнем '{level}'.\n\nХотите присоединиться к ней?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        csClient.GameLevel = level;
                        csClient.NewGameAsync(level, PlayerName);
                    }
                    else
                    {
                        // игрок вышел из турнира
                        csClient.PlayerOutOfGame(csClient.GameLevel);
                        enableStartButton = true;
                    }
                }
            }
            finally
            {
                if (enableStartButton)
                {
                    textBoxPlayerName.Enabled = true;
                    buttonStart.Enabled = true;
                }
            }
        }
        public void SetSudokuState(CSudokuState state)
        {
            try
            {
                FillPlayTable(state);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }
        public void ShowError(Exception exception)
        {
            timerShowConnect.Stop();
            formConnect?.Close();

            MessageBox.Show(exception.AllMessages(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            textBoxPlayerName.Enabled =
            textBoxServer.Enabled =
            buttonStart.Enabled =
            buttonShowTop.Enabled = true;
        }
        public void ShowMessage(string message)
        {
            MessageBox.Show(message, "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public void ShowWinnerList(string[] winnerList)
        {
            buttonShowTop.Enabled = true;
            if (null == winnerList)
            {
                MessageBox.Show("Список победителей пуст.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (string winner in winnerList)
            {
                sb.AppendLine(winner);
            }
            MessageBox.Show(sb.ToString(), "Список победителей");
        }
        public void EndOfGame() =>
            buttonStart.Enabled = true;

        private FormCellSudoku GetCellByCoordinates(int x, int y) =>
            ((FormCellSudoku)(Controls.Find($"{CellName}_{x}_{y}", false)[0]));
        private void TextBoxServer_TextChanged(object sender, EventArgs e)
        {
            CheckForButtons();
        }
        private void TextBoxPlayerName_TextChanged(object sender, EventArgs e)
        {
            CheckForButtons();
        }
        private void CheckForButtons()
        {
            buttonShowTop.Enabled = IsServerNameLengthNotEmpty;
            buttonStart.Enabled = IsServerNameLengthNotEmpty & IsPlayerNameLengthNotEmpty;
        }
        private void FormCSudokuClient_Load(object sender, EventArgs e)
        {
            // рисование поля 9х9

            SuspendLayout();
            int x, y;

            // ячейки 15, 147
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    FormCellSudoku cell = new FormCellSudoku
                    {
                        Name = $"{CellName}_{i}_{j}",
                        Index_X = (byte)i,
                        Index_Y = (byte)j,
                    };
                    x = 10 + i * cell.Width + 5 * (i / 3 + i + 1);
                    y = 142 + j * cell.Height + 5 * (j / 3 + j + 1);
                    cell.Location = new Point(x, y);
                    cell.SudokuClick = Cell_Click;
                    cell.SudokuKey = Cell_KeyUp;


                    Controls.Add(cell);
                }
            }

            // квадраты 10; 142
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    SquareSudoku square = new SquareSudoku
                    {
                        Name = $"{SquareName}_{i}_{j}"
                    };

                    x = 10 + i * square.Width;
                    y = 142 + j * square.Height;

                    square.Location = new Point(x, y);

                    Controls.Add(square);

                }
            }

            ResumeLayout();

            // ставим указатель на левую верхнюю ячейку
            SelectedCell = (FormCellSudoku)(Controls[$"{CellName}_0_0"]);

            // переводим фокус на поле ввода имени игрока
            textBoxPlayerName.Focus();
            textBoxPlayerName.Select();

            // создаём таймер показа окна ожидания соединения
            timerShowConnect = new Timer
            {
                Interval = ShowFormConnectInterval
            };
            timerShowConnect.Tick += TimerShowConnect_Tick;
        }
        private void Cell_Click(object sender, EventArgs e)
        {
            SelectedCell = (FormCellSudoku)(((Button)sender).Tag);
        }
        private void ButtonStart_Click(object sender, EventArgs e)
        {
            FirstRequestToServer(TypeOfFirstRequest.StartGame);
        }
        private void ButtonShowTop_Click(object sender, EventArgs e)
        {
            FirstRequestToServer(TypeOfFirstRequest.WinnerList);
        }
        private void TimerShowConnect_Tick(object sender, EventArgs e)
        {
            try
            {
                timerShowConnect.Stop();
                formConnect = new FormConnect();
                if (formConnect.ShowDialog() == DialogResult.Cancel)
                {
                    csClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }
        private void FillPlayTable(CSudokuState cSudokuState)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    SetValueToCell(cSudokuState.GetValue(i, j));
                }
            }
        }
        private void SetValueToCell(CSudokuCell cell)
        {
            GetCellByCoordinates(cell.X, cell.Y).Value = cell;
        }
        private void FormCSudokuClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (csClient != null)
            {
                csClient.Dispose();
                csClient = null;
            }
        }
        private void LabelSudokuNum_Click(object sender, EventArgs e)
        {
            byte num;
            try
            {
                num = Convert.ToByte(((Label)sender).Name.Split('_')[1]);
            }
            catch
            {
                return;
            }

            SetValueByUser(num);
        }
        private void Cell_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back ||
                e.KeyCode == Keys.Delete)
            {
                SetValueByUser(0);
                return;
            }

            if ((e.KeyValue >= 49 && e.KeyValue <= 57) || // D1..D9
                (e.KeyValue >= 97 && e.KeyValue <= 105))  // NumPad1..NumPad9
            {
                string keyCodeStr = e.KeyCode.ToString();
                if (int.TryParse(keyCodeStr.Substring(keyCodeStr.Length - 1), out int num))
                {
                    SetValueByUser(Convert.ToByte(num));
                }
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (SelectedCell.Index_X == 0)
                    {
                        // Выделена первая ячейка, выделяем последнюю в той же строке
                        SelectedCell = GetCellByCoordinates(((byte)8), SelectedCell.Index_Y);
                    }
                    else
                    {
                        // Выделить ячейку слева от текущей
                        SelectedCell = GetCellByCoordinates((byte)(SelectedCell.Index_X - 1), SelectedCell.Index_Y);
                    }
                    return;

                case Keys.Right:
                    if (SelectedCell.Index_X == 8)
                    {
                        // Выделена последняя ячейка, выделяем первую в той же строке
                        SelectedCell = GetCellByCoordinates(((byte)0), SelectedCell.Index_Y);
                    }
                    else
                    {
                        // Выделить ячейку слева от текущей
                        SelectedCell = GetCellByCoordinates((byte)(SelectedCell.Index_X + 1), SelectedCell.Index_Y);
                    }
                    return;

                case Keys.Up:
                    if (SelectedCell.Index_Y == 0)
                    {
                        // Выделена верхняя ячейка, выделяем нижнюю в том же столбце
                        SelectedCell = GetCellByCoordinates(SelectedCell.Index_X, ((byte)8));
                    }
                    else
                    {
                        // Выделить ячейку сверху от текущей
                        SelectedCell = GetCellByCoordinates(SelectedCell.Index_X, (byte)(SelectedCell.Index_Y - 1));
                    }
                    return;

                case Keys.Down:
                    if (SelectedCell.Index_Y == 8)
                    {
                        // Выделена первая ячейка, выделяем последнюю в той же строке
                        SelectedCell = GetCellByCoordinates(SelectedCell.Index_X, ((byte)0));
                    }
                    else
                    {
                        // Выделить ячейку снизу от текущей
                        SelectedCell = GetCellByCoordinates(SelectedCell.Index_X, (byte)(SelectedCell.Index_Y + 1));
                    }
                    return;
            }
        }
        private void SetValueByUser(byte num)
        {
            csClient?.TrySetValue(SelectedCell.Index_X, SelectedCell.Index_Y, num);
        }
        private async void FirstRequestToServer(TypeOfFirstRequest startOrWinnerList)
        {
            if (null == csClient)
            {
                csClient = new CsClient(this);
            }

            await ConnectToServer();

            if (csClient.Connected)
            {
                textBoxServer.Enabled = false;
                switch (startOrWinnerList)
                {
                    case TypeOfFirstRequest.StartGame:
                        textBoxPlayerName.Enabled = false;
                        buttonStart.Enabled = false;
                        csClient.RequestGameState();
                        break;

                    case TypeOfFirstRequest.WinnerList:
                        buttonShowTop.Enabled = false;
                        csClient.GetWinnersAsync();
                        break;

                    default:
                        MessageBox.Show("Внутренняя ошибка: не определено место вызова ConnectToServer().", "Ошибка программиста", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
        }
        private async Task ConnectToServer()
        {
            timerShowConnect.Start();
            try
            {
                await csClient.TryConnectToServerAsync(ServerName);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            finally
            {
                timerShowConnect.Stop();
                formConnect?.Close();
            }
        }
    }
}
