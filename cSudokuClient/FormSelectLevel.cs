using System;
using System.Drawing;
using System.Windows.Forms;
using cSudokuCommonLib;

namespace cSudokuClient
{
    /// <summary>
    /// Форма выбора игроком уровня сложности игры
    /// </summary>
    public partial class FormSelectLevel : Form
    {
        /// <summary>
        /// Расстояние в пикселях от верхнего края формы строки с первым вариантом уровня сложности
        /// </summary>
        const int coordinateY_firstRadioButton = 32;

        /// <summary>
        /// Высота элемента RadioButton, показывающего вариант уровня сложности игры
        /// </summary>
        const int heightRadioButton = 30;

        /// <summary>
        /// Выбранный уровень сложности игры
        /// </summary>
        public SudokuLevel Level { get; internal set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public FormSelectLevel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Отображение начального состояния формы
        /// </summary>
        private void FormSelectLevel_Load(object sender, EventArgs e)
        {
            // Шрифт для отображения вариантов - такй же, как у имеющейся надписи
            Font font = labelSelectLevel.Font;

            // Счётчик вариантов выбора
            int i = 0;

            // Предотвращение видимого изменения формы
            SuspendLayout();

            // Перебор всех вариантов уровней сложности
            foreach (SudokuLevel level in Enum.GetValues(typeof(SudokuLevel)))
            {
                if (i++ == 0)
                {
                    // Пропуск первого элемента: noGame 
                    continue;
                }

                // Увеличение высоты формы на высоту добавляемого элемента
                this.Height += heightRadioButton;

                // Название очередного уровня
                string levelStr = level.ToString();

                // Создание визуального элемента единственного выбора
                RadioButton radioButton = new RadioButton
                {
                    Font = font,
                    AutoSize = true,
                    AutoCheck = false,
                    Location = new Point(12, coordinateY_firstRadioButton + (i - 1) * heightRadioButton),
                    Name = $"radioButton_{i}",
                    Checked = false,
                    TabIndex = i + 1,
                    TabStop = true,
                    Text = levelStr,
                    UseVisualStyleBackColor = true,
                    Tag = level,
                };
                radioButton.MouseClick += RadioButton_Click;

                Controls.Add(radioButton);
            }

            // Реализация всех призведённых в этом методе изменений формы
            ResumeLayout();
        }

        /// <summary>
        /// Обработка выбора одного из уровней сложности игры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Click(object sender, EventArgs e)
        {
            // Сохранение выбора
            Level = (SudokuLevel)(((RadioButton)sender).Tag);

            // Закрытие формы выбора с возвратом положительного решения
            DialogResult = DialogResult.OK;
        }
    }
}
