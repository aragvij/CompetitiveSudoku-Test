using System;
using System.Drawing;
using System.Windows.Forms;
using cSudokuCommonLib;

namespace cSudokuClient
{
    public partial class FormSelectLevel : Form
    {
        const int coordinateY_firstRadioButton = 32;
        const int heightRadioButton = 30;
        public SudokuLevel Level { get; internal set; }

        public FormSelectLevel()
        {
            InitializeComponent();
        }

        private void FormSelectLevel_Load(object sender, EventArgs e)
        {
            Font font = labelSelectLevel.Font;
            int i = 0;

            SuspendLayout();
            foreach (SudokuLevel level in Enum.GetValues(typeof(SudokuLevel)))
            {
                if (i++ == 0)
                {
                    // Пропуск первого элемента: noGame 
                    continue;
                }
                this.Height += heightRadioButton;
                string levelStr = level.ToString();
                RadioButton radioButton = new RadioButton
                {
                    Font = font,
                    AutoSize = true,
                    AutoCheck = false,
                    Location = new Point(12, coordinateY_firstRadioButton + (i - 1) * heightRadioButton),
                    Name = $"radioButton_{i}",
                    //Size = new System.Drawing.Size(120, 24),
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
            ResumeLayout();
        }
        private void RadioButton_Click(object sender, EventArgs e)
        {
            Level = (SudokuLevel)(((RadioButton)sender).Tag);
            DialogResult = DialogResult.OK;
        }
    }
}
