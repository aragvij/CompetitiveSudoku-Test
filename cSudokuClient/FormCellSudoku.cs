using System;
using System.Drawing;
using System.Windows.Forms;
using cSudokuCommonLib;

namespace cSudokuClient
{
    public partial class FormCellSudoku : UserControl
    {
        private bool selected;
        private CSudokuCell cellValue;

        public byte Index_X { get; internal set; }
        public byte Index_Y { get; internal set; }

        public bool Selected
        {
            get => selected;
            set
            {
                bool isError = null != cellValue && cellValue.Error;
                if (value)
                {
                    button.Select();
                    button.BackColor = isError
                        ? Color.Pink
                        : Color.LightBlue;
                }
                else
                {
                    button.BackColor = isError
                        ? Color.Red
                        : SystemColors.Control;
                }
                selected = value;
            }
        }
        public CSudokuCell Value
        {
            get => cellValue;
            set
            {
                cellValue = value;

                // 1..9
                button.Text = cellValue.Value > 0 && cellValue.Value < 10 ?
                    value.ToString() :
                    string.Empty;

                SetCellColor();
            }
        }
        public EventHandler SudokuClick
        {
            set
            {
                button.Click += value;
            }
        }
        public KeyEventHandler SudokuKey
        {
            set
            {
                button.KeyUp += value;
            }
        }

        public FormCellSudoku()
        {
            InitializeComponent();
            button.Tag = this;
        }
        private void SetCellColor()
        {
            if (null == cellValue)
            {
                return;
            }

            // выделение шрифтом условия задачи
            if (cellValue.IsTask)
            {
                button.Font = new Font(button.Font, FontStyle.Bold);
                button.ForeColor = Color.Blue;
            }
            else
            {
                button.Font = new Font(button.Font, FontStyle.Regular);
                button.ForeColor = Color.Black;
            }

            // обозначение ошибки
            button.BackColor = cellValue.Error
                ? Color.Red
                : SystemColors.Control;
        }
    }
}
