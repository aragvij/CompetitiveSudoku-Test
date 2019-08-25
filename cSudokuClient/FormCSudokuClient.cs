using cSudokuClientLib;
using cSudokuCommonLib;
using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cSudokuClient
{
    /// <summary>
    /// Форма WinForms, являющаяся пользовательским интерфейсом игры в CompetitiveSudoku
    /// </summary>
    public partial class FormCSudokuClient : Form, ICSudokuClient
    {
        /// <summary>
        /// Таймаут, через который следует показать форму ожидания, если соединение с сервером до тех пор не произошло. 500 мс = 1/2 секунды
        /// </summary>
        const int ShowFormConnectInterval = 500;

        /// <summary>
        /// Шаблон имени визуального элемента, который будет представлять собой клетку игрового поля
        /// </summary>
        const string CellName = "cell";

        /// <summary>
        /// Шаблон имени визуального элемента, который будет представлять собой регион 3х3 игрового поля
        /// </summary>
        const string SquareName = "square";

        /// <summary>
        /// Клиентская часть игры
        /// </summary>
        CsClient csClient;

        /// <summary>
        /// Таймер для показа формы ожидания соединения
        /// </summary>
        Timer timerShowConnect;

        /// <summary>
        /// Форма ожидания соединения с сервером; на ней просто написано: "Подождите, выполняется соединение с сервером..."
        /// </summary>
        FormConnect formConnect;

        /// <summary>
        /// Выделенная в настоящий момент ячейка игрового поля
        /// </summary>
        FormCellSudoku selectedCell;

        /// <summary>
        /// Введённая пользователем строка соединения с сервером. По умолчанию: ws://localhost:80/
        /// </summary>
        private string ServerName => textBoxServer.Text.Trim();

        /// <summary>
        /// Введённое пользователем имя игрока без лидирующих и завершеющих пробелов
        /// </summary>
        private string PlayerName => textBoxPlayerName.Text.Trim();

        /// <summary>
        /// true, если строка соединения с сервером не пуста, иначе false
        /// </summary>
        private bool IsServerNameLengthNotEmpty => ServerName.Length > 0;

        /// <summary>
        /// true, если имя пользователя не пусто, иначе false
        /// </summary>
        private bool IsPlayerNameLengthNotEmpty => PlayerName.Length > 0;

        /// <summary>
        /// Выделенная в настоящий момент клетка игрового поля
        /// </summary>
        private FormCellSudoku SelectedCell
        {
            // Возвращаем сохранённое значение
            get => selectedCell;

            // Сохраняем значение, помечаем эту клетку выделенной, а ту клетку, которая была выделена перед этим, помечаем "не выделенной" (если такая была)
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

        /// <summary>
        /// Конструктор
        /// </summary>
        public FormCSudokuClient()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Начало новой игры
        /// </summary>
        /// <param name="level">Уровень сложности игры</param>
        public void StartNewGame(SudokuLevel level)
        {
            // Признак того, надо ли оставить кнопку НАЧАТЬ включенной или выключить её. Сначала выключаем.
            bool enableStartButton = false;

            try
            {
                if (level == SudokuLevel.Unknown)
                {
                    // Если уровень текущей игры не определён (игра не начата)

                    // Выбор уровня сложности игры на соответствующей форме
                    FormSelectLevel formSelectLevel = new FormSelectLevel();
                    if (formSelectLevel.ShowDialog() == DialogResult.OK)
                    {
                        // Прользователь выбрал нужный ему уровень сложности игры, то посылаем соответствующий запрос на сервер
                        csClient.NewGameAsync(formSelectLevel.Level, PlayerName);
                        return;
                    }
                    else
                    {
                        // Если пользователь не стал выбирать уровень сложности, оставим кнопку НАЧАТЬ включенной
                        enableStartButton = true;
                    }
                }
                else
                {
                    // Игра уже идёт. Спрашиваем пользователя, хочет ли он присоединиться к ней?

                    if (MessageBox.Show($"Уже идёт игра с уровнем '{level}'.\n\nХотите присоединиться к ней?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        // Пользователь согласился присоединиться

                        // Устанавливаем у него уровень идущей игры
                        csClient.GameLevel = level;

                        // Сообщаем на сервер о присоединении этого пользователя
                        csClient.NewGameAsync(level, PlayerName);
                    }
                    else
                    {
                        // Игрок отказался от игры с предложенным уровнем сложности (вышел из турнира)

                        // Посылаем на сервер сообщение с отказом от этой игры
                        csClient.PlayerOutOfGame(csClient.GameLevel);

                        // Кнопку НАЧАТЬ включаем - пусть у игрока будет шанс присоединиться к игре позже
                        enableStartButton = true;
                    }
                }
            }
            finally
            {
                if (enableStartButton)
                {
                    // Если игрок не присоединился к игре, открываем ему возможность изменить своё имя и вновь подключиться
                    textBoxPlayerName.Enabled = true;
                    buttonStart.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Отображение на игровом поле текущей мгровой ситуации
        /// </summary>
        /// <param name="state">Текущее положение в игре</param>
        public void SetSudokuState(CSudokuState state)
        {
            try
            {
                // Заполнение игрового поля значениями
                FillPlayTable(state);
            }
            catch (Exception ex)
            {
                // В случае ошибки - показ окошка с сообщением об этом
                ShowError(ex);
            }
        }

        /// <summary>
        /// Отображение окна с описанием исключительной ситуации (Exception)
        /// </summary>
        /// <param name="exception">Исключение (Exception), информацию о котором нужно показать</param>
        public void ShowError(Exception exception)
        {
            // Если был запущен таймер, отсчитывающий время до показа окна ожидания соединения с сервером, останавливаем его
            timerShowConnect.Stop();

            // Если было показано окно ожидания соединения с сервером, закрываем его
            formConnect?.Close();

            // Отображение окна с описанием ошибки
            MessageBox.Show(exception.AllMessages(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // Деаем доступными оба текстовых поля и обе кнопки (НАЧАТЬ и ПОБЕДИТЕЛИ) на форме
            textBoxPlayerName.Enabled =
            textBoxServer.Enabled =
            buttonStart.Enabled =
            buttonShowTop.Enabled = true;
        }

        /// <summary>
        /// Отображение сообщения в окне с заголовком "Сообщение"
        /// </summary>
        /// <param name="message">Строка, содержащая сообщение для показа</param>
        public void ShowMessage(string message)
        {
            MessageBox.Show(message, "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Показ списка победителей
        /// </summary>
        /// <param name="winnerList">Набор строк, содержащий список победителей</param>
        public void ShowWinnerList(string[] winnerList)
        {
            // Даём возможность вновь нажать на кнопку ПОБЕДИТЕЛИ
            buttonShowTop.Enabled = true;

            if (null == winnerList)
            {
                // Если списка победителей нет, сообщаем об этом, и всё

                MessageBox.Show("Список победителей пуст.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Формируем сообщение для показа в окне "Список победителей"
            StringBuilder sb = new StringBuilder();
            foreach (string winner in winnerList)
            {
                sb.AppendLine(winner);
            }

            // Отображение списка
            MessageBox.Show(sb.ToString(), "Список победителей");
        }

        /// <summary>
        /// Конец игры. Игрок получае возможность начать новую игру.
        /// </summary>
        public void EndOfGame() =>
            buttonStart.Enabled = true;

        /// <summary>
        /// Получение клетки по её координатам на игровом поле
        /// </summary>
        /// <param name="x">Столбец, в котором находится искомая клетка</param>
        /// <param name="y">Строка, в которой находится искомая клетка</param>
        /// <returns>Клетка игрового поля</returns>
        private FormCellSudoku GetCellByCoordinates(int x, int y) =>
            ((FormCellSudoku)(Controls.Find($"{CellName}_{x}_{y}", false)[0]));

        /// <summary>
        /// Обработка изменения текста в поле "Сервер игры"
        /// </summary>
        private void TextBoxServer_TextChanged(object sender, EventArgs e)
        {
            // Проверка и установка доступности кнопок ПОБЕДИТЕЛИ и НАЧАТЬ
            CheckForButtons();
        }

        /// <summary>
        /// Обработка изменения текста в поле "Имя игрока"
        /// </summary>
        private void TextBoxPlayerName_TextChanged(object sender, EventArgs e)
        {
            // Проверка и установка доступности кнопок ПОБЕДИТЕЛИ и НАЧАТЬ
            CheckForButtons();
        }

        /// <summary>
        /// Проверка и установка доступности кнопок ПОБЕДИТЕЛИ и НАЧАТЬ
        /// </summary>
        private void CheckForButtons()
        {
            // Кнопка ПОБЕДИТЕЛИ доступна, если поле "Сервер игры" не пустое
            buttonShowTop.Enabled = IsServerNameLengthNotEmpty;

            // Кнопка НАЧАТЬ доступна, если поля "Имя игрока" и "Сервер игры" не пустые
            buttonStart.Enabled = IsServerNameLengthNotEmpty & IsPlayerNameLengthNotEmpty;
        }

        /// <summary>
        /// Отображение формы при запуске программы
        /// </summary>
        private void FormCSudokuClient_Load(object sender, EventArgs e)
        {
            // Рисование игрового поля 9х9

            // Предотвращение видимого изменения формы
            SuspendLayout();

            int x, y;

            // Ячейки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    // Создание новой видимой ячейки
                    FormCellSudoku cell = new FormCellSudoku
                    {
                        // Имя ячейки содержит её координаты
                        Name = $"{CellName}_{i}_{j}",
                        Index_X = (byte)i,
                        Index_Y = (byte)j,
                    };

                    // Вычисление координат (в пикселях) левого верхнего угла игровой ячейки. Ячейка [0,0] расположена по координатам [10+5, 142+5], найденным эмпирически
                    x = 10 + i * cell.Width + 5 * (i / 3 + i + 1);
                    y = 142 + j * cell.Height + 5 * (j / 3 + j + 1);

                    // Расположение ячейки по найденным координатам
                    cell.Location = new Point(x, y);

                    // Назначение обработчика событий для новой ячейки: щелчок по ней мышью
                    cell.SudokuClick = Cell_Click;

                    // Назначение обработчика событий для новой ячейки: нажатие клавиши
                    cell.SudokuKey = Cell_KeyUp;

                    // Добавление ячейки в коллекцию элементов управления этой формы
                    Controls.Add(cell);
                }
            }

            // Регионы 3х3
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    // Создание нового региона
                    SquareSudoku square = new SquareSudoku
                    {
                        Name = $"{SquareName}_{i}_{j}"
                    };

                    // Определение координат левого верхнего угла региона. Координаты (в пикселях) самого левого верхнего региона [10, 142] найдены эмпирически
                    x = 10 + i * square.Width;
                    y = 142 + j * square.Height;

                    // Расположение региона по вычисленным координатам
                    square.Location = new Point(x, y);

                    // Добавление региона в коллекцию элементов управления этой формы
                    Controls.Add(square);
                }
            }

            // Реализация всех призведённых в этом методе изменений формы
            ResumeLayout();

            // Левую верхнюю ячейку игрового поля помечаем выделенной
            SelectedCell = (FormCellSudoku)(Controls[$"{CellName}_0_0"]);

            // Переводим фокус на поле ввода имени игрока
            textBoxPlayerName.Focus();
            textBoxPlayerName.Select();

            // Создаём таймер показа окна ожидания соединения
            timerShowConnect = new Timer
            {
                Interval = ShowFormConnectInterval
            };
            timerShowConnect.Tick += TimerShowConnect_Tick;
        }

        /// <summary>
        /// Обработчик события щелчка мыши при её курсоре, установленном на ячейку игрового поля
        /// </summary>
        private void Cell_Click(object sender, EventArgs e)
        {
            // Назначаем клетку, по которой был произведён щелчок, выделенной
            SelectedCell = (FormCellSudoku)(((Button)sender).Tag);
        }

        /// <summary>
        /// Обработчик нажатия кнопки НАЧАТЬ
        /// </summary>
        private void ButtonStart_Click(object sender, EventArgs e)
        {
            // Попытка соединиться с сервером в режиме StartGame
            FirstRequestToServer(TypeOfFirstRequest.StartGame);
        }

        /// <summary>
        /// Обработчик нажатия кнопки ПОБЕДИТЕЛИ
        /// </summary>
        private void ButtonShowTop_Click(object sender, EventArgs e)
        {
            // Попытка соединиться с сервером в режиме WinnerList
            FirstRequestToServer(TypeOfFirstRequest.WinnerList);
        }

        /// <summary>
        /// Обработчик срабатывания таймер ожидания соединения
        /// </summary>
        private void TimerShowConnect_Tick(object sender, EventArgs e)
        {
            try
            {
                // Остановка таймера
                timerShowConnect.Stop();

                // Отображение окна ожидания соединения с сервером
                formConnect = new FormConnect();
                if (formConnect.ShowDialog() == DialogResult.Cancel)
                {
                    // Если пользователь в окне ожидания нажал кнопку ОТМЕНА, то прекращаем попытку соединения с сервером
                    csClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки показываем её текст пользователю
                ShowError(ex);
            }
        }

        /// <summary>
        /// Заполнение отображаемого игрового поля значениями, полученными с сервера
        /// </summary>
        /// <param name="cSudokuState">Текущее состояние игры</param>
        private void FillPlayTable(CSudokuState cSudokuState)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    // Установка отображаемого значания в каждую из 9х9 клеток
                    SetValueToCell(cSudokuState.GetValue(i, j));
                }
            }
        }

        /// <summary>
        /// Установка отображаемого значения конкретной клетки игрового поля
        /// </summary>
        /// <param name="cell">Ячейка игрового поля, значение которой необходимо отобразить для пользователя</param>
        private void SetValueToCell(CSudokuCell cell)
        {
            GetCellByCoordinates(cell.X, cell.Y).Value = cell;
        }

        /// <summary>
        /// Обработка закрытия окна программы
        /// </summary>
        private void FormCSudokuClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (csClient != null)
            {
                // Прерываем соединение и удаляем WebSocket-клиента
                csClient.Dispose();
                csClient = null;
            }
        }

        /// <summary>
        /// Обработчик выбора варианта хода из списка (отображаемые на форме кнопки 1..9 и СТЕРЕТЬ)
        /// </summary>
        private void LabelSudokuNum_Click(object sender, EventArgs e)
        {
            byte num;
            try
            {
                // Определяем желаемое игроком значение по имени выбранного элемента. У кнопок с цифрами это 1..9, у кнопки СТЕРЕТЬ - это 0.
                num = Convert.ToByte(((Label)sender).Name.Split('_')[1]);
            }
            catch
            {
                // Если не удалось, ничего не делаем
                return;
            }

            // Посылаем на сервер сообщение о произведённом ходе
            SetValueByUser(num);
        }

        /// <summary>
        /// Обработка нажатия кнопки на выделенной ячейке игрового поля
        /// </summary>
        /// <param name="sender">Ячейка игрового поля, на котором произошло нажатие кнопки</param>
        /// <param name="e">Кнопка, которая была нажата и отпущена</param>
        private void Cell_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back ||
                e.KeyCode == Keys.Delete)
            {
                // Если была нажата кнопа Delete или BackSpace, считаем, что игрок стёр значение ячейки, передаём 0
                SetValueByUser(0);

                // Выход
                return;
            }

            if ((e.KeyValue >= 49 && e.KeyValue <= 57) || // D1..D9
                (e.KeyValue >= 97 && e.KeyValue <= 105))  // NumPad1..NumPad9
            {
                // Если нажата одна из кнопок с цифрами на основной или цифровой клавиатуре

                // Определяем название клавиши, которая была нажата
                string keyCodeStr = e.KeyCode.ToString();

                // Последний символ названия цифровых клавиш содержит ту чифру, которая соответствует этой клавише
                if (int.TryParse(keyCodeStr.Substring(keyCodeStr.Length - 1), out int num))
                {
                    // Если удалось определить выбранное значение 1..9, переадём его на сервер
                    SetValueByUser(Convert.ToByte(num));
                }

                // Выход
                return;
            }

            // Если была нажата клавиша СО СТРЕЛКОЙ
            switch (e.KeyCode)
            {
                case Keys.Left:
                    // Влево

                    if (SelectedCell.Index_X == 0)
                    {
                        // Выделена первая ячейка, выделить вместо неё последнюю в той же строке
                        SelectedCell = GetCellByCoordinates(((byte)8), SelectedCell.Index_Y);
                    }
                    else
                    {
                        // Выделить ячейку слева от текущей
                        SelectedCell = GetCellByCoordinates((byte)(SelectedCell.Index_X - 1), SelectedCell.Index_Y);
                    }
                    return;

                case Keys.Right:
                    // Вправо

                    if (SelectedCell.Index_X == 8)
                    {
                        // Выделена последняя ячейка, выделить вместо неё первую в той же строке
                        SelectedCell = GetCellByCoordinates(((byte)0), SelectedCell.Index_Y);
                    }
                    else
                    {
                        // Выделить ячейку слева от текущей
                        SelectedCell = GetCellByCoordinates((byte)(SelectedCell.Index_X + 1), SelectedCell.Index_Y);
                    }
                    return;

                case Keys.Up:
                    // Вверх

                    if (SelectedCell.Index_Y == 0)
                    {
                        // Выделена верхняя ячейка, выделить вместо неё нижнюю в том же столбце
                        SelectedCell = GetCellByCoordinates(SelectedCell.Index_X, ((byte)8));
                    }
                    else
                    {
                        // Выделить ячейку сверху от текущей
                        SelectedCell = GetCellByCoordinates(SelectedCell.Index_X, (byte)(SelectedCell.Index_Y - 1));
                    }
                    return;

                case Keys.Down:
                    // Вниз

                    if (SelectedCell.Index_Y == 8)
                    {
                        // Выделена первая ячейка, выделить вместо неё последнюю в той же строке
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

        /// <summary>
        /// Передача на сервер того значения, которое задал игрок для ячейки игрового поля (сделал ход)
        /// </summary>
        /// <param name="num">Значение, выбранное игроком: 1..9 или 0 для стирания предыдущего значения</param>
        private void SetValueByUser(byte num)
        {
            // Передача координат выделенной ячейки игрового поля и введённого пользователем значения для этой ячейки
            csClient?.TrySetValue(SelectedCell.Index_X, SelectedCell.Index_Y, num);
        }

        /// <summary>
        /// Установка связи с сервером
        /// </summary>
        /// <param name="startOrWinnerList">То событие, которое привело к установлению связи</param>
        private async void FirstRequestToServer(TypeOfFirstRequest startOrWinnerList)
        {
            if (null == csClient)
            {
                // Если экземпляра CsClient до сих пор не было, создаём его
                csClient = new CsClient(this);
            }

            // Попытка установить связь
            await ConnectToServer();

            if (csClient.Connected)
            {
                // Если попытка установть связь оказалась удачной

                // Выключаем доступность кнопки НАЧАТЬ
                textBoxServer.Enabled = false;

                // В зависимости от места, откуда была произведена ппопытка установки связи...
                switch (startOrWinnerList)
                {
                    case TypeOfFirstRequest.StartGame:
                        // Начало игры

                        // Лишаем игрока возможности изменить своё имя и вновь нажать кнопку НАЧАТЬ
                        textBoxPlayerName.Enabled = false;
                        buttonStart.Enabled = false;

                        // Запрашиваем уровень сложности текущей игры (если она идёт)
                        csClient.RequestGameState();
                        break;

                    case TypeOfFirstRequest.WinnerList:
                        // Список победителей

                        // Выключаем кнопку ПОБЕДИТЕЛИ
                        buttonShowTop.Enabled = false;

                        // Запрашиваем на сервере список победителей
                        csClient.GetWinnersAsync();
                        break;

                    default:
                        // Если набор вариантов установки связи с сервером будет расширен, необходимо не забыть обработать новые

                        MessageBox.Show("Внутренняя ошибка: не определено место вызова ConnectToServer().", "Ошибка программиста", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
        }

        /// <summary>
        /// Попытка установить связь с WebSocket-сервером
        /// </summary>
        /// <returns>Задача, ожидающая установление связи</returns>
        private async Task ConnectToServer()
        {
            // Запуск таймера показа окошка ожидания соединения
            timerShowConnect.Start();

            try
            {
                // Попытка соединения с WebSocket-сервером
                await csClient.TryConnectToServerAsync(ServerName);
            }
            catch (Exception ex)
            {
                // В случае ошибки показываем её пользователю
                ShowError(ex);
            }
            finally
            {
                // Останавливаем таймер ожидания
                timerShowConnect.Stop();

                // Закрываем окошко ожидания соединения, если оно было показано
                formConnect?.Close();
            }
        }
    }
}
