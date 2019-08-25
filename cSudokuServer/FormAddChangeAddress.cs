using System.Windows.Forms;

namespace cSudokuServer
{
    /// <summary>
    /// Диалоговое окно изменения IP-адреса
    /// </summary>
    public partial class FormAddChangeAddress : Form
    {
        /// <summary>
        /// Выдаёт находящуюся в поле ввода строку без лидирующих и финальных пробелов
        /// </summary>
        public string Address => textBoxAddress.Text.Trim();

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="ipAddress">Тот адрес, который нужно изменить, или null для добавления нового адреса</param>
        public FormAddChangeAddress(string ipAddress = null)
        {
            InitializeComponent();

            if (ipAddress != null)
                textBoxAddress.Text = ipAddress;
        }
    }
}
