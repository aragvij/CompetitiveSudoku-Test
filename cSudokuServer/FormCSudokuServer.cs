using cSudokuCommonLib;
using cSudokuServerLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace cSudokuServer
{
    /// <summary>
    /// Пользовательский интерфейс серверной части. Позволяет запускать и останавливать сервер WebSocket, а также производить необходимые установки. Кроме этого, демонстрирует список текущих пользователей.
    /// </summary>
    public partial class FormCSudokuServer : Form
    {
        /// <summary>
        /// Прослушиваемый порт
        /// </summary>
        int? port = 80;

        /// <summary>
        /// Признак работы WebSocket-сервера
        /// </summary>
        bool isRun = false;

        /// <summary>
        /// Экземпляр класса WebSocket-сервера "конкурентного судоку"
        /// </summary>
        CsServer server;

        /// <summary>
        /// Конструктор
        /// </summary>
        public FormCSudokuServer()
        {
            InitializeComponent();
            textBoxPort.Text = port.ToString();
        }

        /// <summary>
        /// Действия, выполняемые при запуске этого приложения
        /// </summary>
        private void FormCSudokuServer_Load(object sender, EventArgs e)
        {
            // Получение имён и адресов данного компьютера
            string[] addr = CsServer.GetAddressList();

            // Наполнение списка адресов имеющимися
            checkedListBoxAddresses.Items.AddRange(addr);
        }

        /// <summary>
        /// Обработка события отметки одного из адресов в списке
        /// </summary>
        private void CheckedListBoxAddresses_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Определение того, что было сделано с адресом: был ли он выделен, или с него была снята пометка
            int delta = e.NewValue == CheckState.Checked ? 1 : -1;

            // Определение возможности запуска WebSocket-сервера
            CheckForStart(checkedListBoxAddresses.CheckedItems.Count + delta);
        }

        /// <summary>
        /// Обработка события изменения IP-порта
        /// </summary>
        private void TextBoxPort_TextChanged(object sender, EventArgs e)
        {
            // Попытка привести введённое значение к числу (номер IP-порта)
            if (int.TryParse(textBoxPort.Text, out int portInt))
            {
                port = portInt;
            }
            else
                port = null;

            // Определение возможности запуска WebSocket-сервера
            CheckForStart(checkedListBoxAddresses.CheckedItems.Count);
        }

        /// <summary>
        /// Определение возможности запуска WebSocket-сервера
        /// </summary>
        /// <param name="checkedAddrNum">Количество адресов, выделенных для прослушивания</param>
        private void CheckForStart(int checkedAddrNum)
        {
            // Запуск сервера возможен только если определён IP-порт и количество адресов больше нуля
            if (port != null && checkedAddrNum > 0)
            {
                buttonStartStop.Enabled = true;
                buttonStartStop.BackColor = Color.Green;
            }
            else
            {
                buttonStartStop.Enabled = false;
                buttonStartStop.BackColor = Color.Gray;
            }
        }

        /// <summary>
        /// Обработка нажания на кнопку запуска/остановки WebSocket-сервера
        /// </summary>
        private void ButtonStartStop_Click(object sender, EventArgs e)
        {
            if (null == port)
            {
                MessageBox.Show("Не определён порт!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (isRun)
                {
                    // Если сервер работал, уничтожаем его
                    server?.Dispose();
                }
                else
                {
                    // Если сервер не работал, запускае его

                    // Определение массива адресов, по которым WebSocket-сервер будет вести "прослушивание"
                    string[] addresses = new string[checkedListBoxAddresses.CheckedItems.Count];
                    checkedListBoxAddresses.CheckedItems.CopyTo(addresses, 0);

                    // Создание экземпляра WebSocket-сервера игры CompetitiveSudoku
                    server = new CsServer((int)port, addresses, ShowError, AddPlayer);

                    server.Start();
                }

                // Установка вида и доступности элементов управления на форме после нажатия кнопики СТАРТ/СТОП
                SetControlsStartStop();
            }
            catch (Exception ex)
            {
                ShowError(ex.AllMessages());
            }
        }

        /// <summary>
        /// Отображение сообщения об ошибке
        /// </summary>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        private void ShowError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Вывод на экран текущего списка игроков
        /// </summary>
        /// <param name="playerNames">Список имён игроков</param>
        private void AddPlayer(List<string> playerNames)
        {
            listBoxCurrentPlayers.BeginUpdate();
            listBoxCurrentPlayers.Items.Clear();
            playerNames.ForEach(
                playerName => listBoxCurrentPlayers.Items.Add(playerName));
            listBoxCurrentPlayers.EndUpdate();
        }

        /// <summary>
        /// Установка вида и доступности элементов управления на форме, в зависимости от того, была ли нажата кнопка СТАРТ или СТОП
        /// </summary>
        private void SetControlsStartStop()
        {
            if (isRun)
            {
                // Сервер остановлен
                buttonStartStop.Text = "С т а р т";
                buttonStartStop.BackColor = Color.Green;
            }
            else
            {
                // Сервер запущен в работу
                buttonStartStop.Text = "С т о п";
                buttonStartStop.BackColor = Color.Red;
            }

            // Пока сервер работает, не позволяем менять его адреса и порт
            textBoxPort.Enabled = groupBoxAddresses.Enabled = isRun;

            // Смена значения признака работы WebSocket-сервер
            isRun = !isRun;
        }

        /// <summary>
        /// Доступность кнопок работы со списком адресов
        /// </summary>
        private void CheckedListBoxAddresses_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Удалить или изменить адрес можно только тогда, когда в списке выбран единственный вариант
            buttonAddressChange.Enabled =
            buttonAddressDelete.Enabled =
                checkedListBoxAddresses.SelectedItems.Count == 1;
        }

        /// <summary>
        /// Обработка нажатия кнопки "Добавить" в списке адресов
        /// </summary>
        private void ButtonAddressAdd_Click(object sender, EventArgs e)
        {
            // Показ пустой формы редактирования адреса
            ShowAddressEdit(null);
        }

        /// <summary>
        /// Обработка нажатия кнопки "Изменить" в списке адресов
        /// </summary>
        private void ButtonAddressChange_Click(object sender, EventArgs e)
        {
            // Показ формы редактирования адреса с помещённым на неё выделенным в списке адресом
            checkedListBoxAddresses.BeginUpdate();
            ShowAddressEdit(checkedListBoxAddresses.SelectedItem as string);
            checkedListBoxAddresses.EndUpdate();
        }

        /// <summary>
        /// Вызов формы редактирования  адреса
        /// </summary>
        /// <param name="IpAddress">Тот адрес, который нужно изменить, или null для нового адреса</param>
        private void ShowAddressEdit(string IpAddress)
        {
            // Создание и показ диалогового окна
            var formAdd = new FormAddChangeAddress(IpAddress);
            if (formAdd.ShowDialog() == DialogResult.OK)
            {
                // Окно закрыто нажатием кнопки "Сохранить"

                var address = formAdd.Address;
                if (address.Length > 0)
                {
                    // Адрес в окне редактирования не пуст

                    if (null == IpAddress)
                    {
                        // Добавление нового адреса в список
                        checkedListBoxAddresses.Items.Add(address, true);
                    }
                    else
                    {
                        // Изменение существующего в списке адреса
                        checkedListBoxAddresses.Items[checkedListBoxAddresses.SelectedIndex] = address;
                    }
                }
            }
        }

        /// <summary>
        /// Обработка удаления адреса из списка адресов
        /// </summary>
        private void ButtonAddressDelete_Click(object sender, EventArgs e)
        {
            // Удаление одного из адресов из списка
            checkedListBoxAddresses.Items.RemoveAt(checkedListBoxAddresses.SelectedIndex);

            // Определение возможности запуска WebSocket-сервера
            CheckForStart(checkedListBoxAddresses.CheckedItems.Count);
        }

        /// <summary>
        /// Обработка закрытия формы (выхода из приложения)
        /// </summary>
        private void FormCSudokuServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Экземпляр класса уничтожается, если он есть
            server?.Dispose();
        }
    }
}
