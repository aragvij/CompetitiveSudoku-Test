namespace cSudokuClient
{
    partial class SquareSudoku
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelSquare = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelSquare
            // 
            this.labelSquare.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelSquare.Location = new System.Drawing.Point(0, 0);
            this.labelSquare.Name = "labelSquare";
            this.labelSquare.Size = new System.Drawing.Size(140, 140);
            this.labelSquare.TabIndex = 58;
            // 
            // SquareSudoku
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.labelSquare);
            this.Name = "SquareSudoku";
            this.Size = new System.Drawing.Size(140, 140);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelSquare;
    }
}
