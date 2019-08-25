using System;
using System.Drawing;
using System.Windows.Forms;
using cSudokuCommonLib;

namespace cSudokuClient
{
    /// <summary>
    /// Пользовательский визуальный элемент управления, представляющий собой одну клетку игрового поля
    /// </summary>
    public partial class FormCellSudoku : UserControl
    {
        /// <summary>
        /// Флаг, хранящий состояние клетки: выбрана она или нет
        /// </summary>
        private bool selected;

        /// <summary>
        /// Значение клетки, полученное с сервера
        /// </summary>
        private CSudokuCell cellValue;

        /// <summary>
        /// Номер столбца (начиная с 0), в котором находится эта клетка
        /// </summary>
        public byte Index_X { get; internal set; }

        /// <summary>
        /// Номер строки (начиная с 0), в которой находится эта клетка
        /// </summary>
        public byte Index_Y { get; internal set; }

        /// <summary>
        /// Признак выбранности клетки на игровом поле
        /// </summary>
        public bool Selected
        {
            // При запросе просто выдаём сохранённое значение признака
            get => selected;

            // При установке меняем внешний вид клетки
            set
            {
                // Определение того, содержит ли клетка ошибочное значение
                bool isError = null != cellValue && cellValue.Error;

                if (value)
                {
                    // Если клетка выбрана
                    button.Select();
                    button.BackColor = isError
                        // Ошибочная клетка будет розовой
                        ? Color.Pink
                        // Клетка без ошибки будет голубой
                        : Color.LightBlue;
                }
                else
                {
                    // Если клетка НЕ выбрана
                    button.BackColor = isError
                        // Ошибочная клетка будет красной
                        ? Color.Red
                        // Клетка без ошибки будет "естественного цвета" - как остальное окно
                        : SystemColors.Control;
                }

                // Сохраняем признак выбранности
                selected = value;
            }
        }

        /// <summary>
        /// Игровое значение клетки
        /// </summary>
        public CSudokuCell Value
        {
            // При запросе просто выдаём сохранённое игровое значение
            get => cellValue;

            // При установке сохраняем значение и определяем внешний вид клетки после проверки
            set
            {
                cellValue = value;

                // Если числовое значение, помещённое в клетку, находится в диапазоне 1..9, то помещаем изображение этого числа на кнопку
                button.Text = cellValue.Value > 0 && cellValue.Value < 10
                    ? value.ToString()
                    // Если числовле значение вне диапазоне 1..9, то очищаем изображение клетки
                    : string.Empty;

                // Определение внешнего вида клетки
                SetCellColor();
            }
        }

        /// <summary>
        /// Собтие выделения этой клетки мышкой
        /// </summary>
        public EventHandler SudokuClick
        {
            set => button.Click += value;
        }

        /// <summary>
        /// Событие нажатия клавиши при клетке, находящейся в фокусе ввода
        /// </summary>
        public KeyEventHandler SudokuKey
        {
            set => button.KeyUp += value;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public FormCellSudoku()
        {
            InitializeComponent();

            // Привязываем экземпляр этого класса к находящейся на нём кнопке
            button.Tag = this;
        }

        /// <summary>
        /// Определение внешнего вида клетки
        /// </summary>
        private void SetCellColor()
        {
            if (null == cellValue)
            {
                // Если у клетки нет значения, ничего не делаем
                return;
            }

            // Выделение шрифтом условия задачи
            if (cellValue.IsTask)
            {
                // Вариант, когда клетка содержит значение, являющееся условием судоку

                // Делаем шрифт на кнопке полужирным синего цвета
                button.Font = new Font(button.Font, FontStyle.Bold);
                button.ForeColor = Color.Blue;
            }
            else
            {
                // Вариант, когда клетка не является условием

                // Делаем шрифт обычного вида и чёрным
                button.Font = new Font(button.Font, FontStyle.Regular);
                button.ForeColor = Color.Black;
            }

            // Если клетка содержит значение, являющееся ошибочным для судоку, делаем её фон красным
            button.BackColor = cellValue.Error
                ? Color.Red
                : SystemColors.Control;
        }
    }
}
