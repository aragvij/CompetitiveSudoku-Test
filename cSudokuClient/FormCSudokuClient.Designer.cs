namespace cSudokuClient
{
    partial class FormCSudokuClient
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

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelServer = new System.Windows.Forms.Label();
            this.textBoxServer = new System.Windows.Forms.TextBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.panelControl = new System.Windows.Forms.Panel();
            this.labelPlayerName = new System.Windows.Forms.Label();
            this.textBoxPlayerName = new System.Windows.Forms.TextBox();
            this.buttonShowTop = new System.Windows.Forms.Button();
            this.panelNumbers = new System.Windows.Forms.Panel();
            this.labelRules = new System.Windows.Forms.Label();
            this.label_0 = new System.Windows.Forms.Label();
            this.label_7 = new System.Windows.Forms.Label();
            this.label_6 = new System.Windows.Forms.Label();
            this.label_5 = new System.Windows.Forms.Label();
            this.label_2 = new System.Windows.Forms.Label();
            this.label_3 = new System.Windows.Forms.Label();
            this.label_4 = new System.Windows.Forms.Label();
            this.label_8 = new System.Windows.Forms.Label();
            this.label_9 = new System.Windows.Forms.Label();
            this.label_1 = new System.Windows.Forms.Label();
            this.panelControl.SuspendLayout();
            this.panelNumbers.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelServer
            // 
            this.labelServer.AutoSize = true;
            this.labelServer.Location = new System.Drawing.Point(3, 15);
            this.labelServer.Name = "labelServer";
            this.labelServer.Size = new System.Drawing.Size(109, 20);
            this.labelServer.TabIndex = 0;
            this.labelServer.Text = "Сервер игры:";
            // 
            // textBoxServer
            // 
            this.textBoxServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxServer.Location = new System.Drawing.Point(118, 12);
            this.textBoxServer.Name = "textBoxServer";
            this.textBoxServer.Size = new System.Drawing.Size(179, 26);
            this.textBoxServer.TabIndex = 2;
            this.textBoxServer.Text = "ws://localhost:80/";
            this.textBoxServer.TextChanged += new System.EventHandler(this.TextBoxServer_TextChanged);
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.Enabled = false;
            this.buttonStart.Location = new System.Drawing.Point(303, 44);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(128, 26);
            this.buttonStart.TabIndex = 5;
            this.buttonStart.Text = "Начать";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // panelControl
            // 
            this.panelControl.Controls.Add(this.labelPlayerName);
            this.panelControl.Controls.Add(this.textBoxPlayerName);
            this.panelControl.Controls.Add(this.labelServer);
            this.panelControl.Controls.Add(this.buttonStart);
            this.panelControl.Controls.Add(this.buttonShowTop);
            this.panelControl.Controls.Add(this.textBoxServer);
            this.panelControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl.Location = new System.Drawing.Point(0, 0);
            this.panelControl.Name = "panelControl";
            this.panelControl.Size = new System.Drawing.Size(443, 83);
            this.panelControl.TabIndex = 0;
            // 
            // labelPlayerName
            // 
            this.labelPlayerName.AutoSize = true;
            this.labelPlayerName.Location = new System.Drawing.Point(3, 47);
            this.labelPlayerName.Name = "labelPlayerName";
            this.labelPlayerName.Size = new System.Drawing.Size(99, 20);
            this.labelPlayerName.TabIndex = 1;
            this.labelPlayerName.Text = "Имя игрока:";
            // 
            // textBoxPlayerName
            // 
            this.textBoxPlayerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPlayerName.Location = new System.Drawing.Point(118, 44);
            this.textBoxPlayerName.Name = "textBoxPlayerName";
            this.textBoxPlayerName.Size = new System.Drawing.Size(179, 26);
            this.textBoxPlayerName.TabIndex = 4;
            this.textBoxPlayerName.TextChanged += new System.EventHandler(this.TextBoxPlayerName_TextChanged);
            // 
            // buttonShowTop
            // 
            this.buttonShowTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonShowTop.Location = new System.Drawing.Point(303, 12);
            this.buttonShowTop.Name = "buttonShowTop";
            this.buttonShowTop.Size = new System.Drawing.Size(128, 26);
            this.buttonShowTop.TabIndex = 3;
            this.buttonShowTop.Text = "Победители";
            this.buttonShowTop.UseVisualStyleBackColor = true;
            this.buttonShowTop.Click += new System.EventHandler(this.ButtonShowTop_Click);
            // 
            // panelNumbers
            // 
            this.panelNumbers.Controls.Add(this.labelRules);
            this.panelNumbers.Controls.Add(this.label_0);
            this.panelNumbers.Controls.Add(this.label_7);
            this.panelNumbers.Controls.Add(this.label_6);
            this.panelNumbers.Controls.Add(this.label_5);
            this.panelNumbers.Controls.Add(this.label_2);
            this.panelNumbers.Controls.Add(this.label_3);
            this.panelNumbers.Controls.Add(this.label_4);
            this.panelNumbers.Controls.Add(this.label_8);
            this.panelNumbers.Controls.Add(this.label_9);
            this.panelNumbers.Controls.Add(this.label_1);
            this.panelNumbers.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelNumbers.Location = new System.Drawing.Point(0, 83);
            this.panelNumbers.Name = "panelNumbers";
            this.panelNumbers.Size = new System.Drawing.Size(443, 43);
            this.panelNumbers.TabIndex = 4;
            // 
            // labelRules
            // 
            this.labelRules.AutoSize = true;
            this.labelRules.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelRules.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelRules.Location = new System.Drawing.Point(0, 30);
            this.labelRules.Name = "labelRules";
            this.labelRules.Size = new System.Drawing.Size(429, 13);
            this.labelRules.TabIndex = 10;
            this.labelRules.Text = "Для игры используйте мышь или клавиши: 1 ,2, 3, 4, 5 ,6 ,7 ,8, 9, Delete, Backspa" +
    "ce";
            // 
            // label_0
            // 
            this.label_0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_0.Location = new System.Drawing.Point(318, 3);
            this.label_0.Name = "label_0";
            this.label_0.Size = new System.Drawing.Size(74, 22);
            this.label_0.TabIndex = 9;
            this.label_0.Text = "стереть";
            this.label_0.Click += new System.EventHandler(this.LabelSudokuNum_Click);
            // 
            // label_7
            // 
            this.label_7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_7.Location = new System.Drawing.Point(180, 3);
            this.label_7.Name = "label_7";
            this.label_7.Size = new System.Drawing.Size(22, 22);
            this.label_7.TabIndex = 8;
            this.label_7.Text = "7";
            this.label_7.Click += new System.EventHandler(this.LabelSudokuNum_Click);
            // 
            // label_6
            // 
            this.label_6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_6.Location = new System.Drawing.Point(152, 3);
            this.label_6.Name = "label_6";
            this.label_6.Size = new System.Drawing.Size(22, 22);
            this.label_6.TabIndex = 7;
            this.label_6.Text = "6";
            this.label_6.Click += new System.EventHandler(this.LabelSudokuNum_Click);
            // 
            // label_5
            // 
            this.label_5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_5.Location = new System.Drawing.Point(124, 3);
            this.label_5.Name = "label_5";
            this.label_5.Size = new System.Drawing.Size(22, 22);
            this.label_5.TabIndex = 6;
            this.label_5.Text = "5";
            this.label_5.Click += new System.EventHandler(this.LabelSudokuNum_Click);
            // 
            // label_2
            // 
            this.label_2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_2.Location = new System.Drawing.Point(40, 3);
            this.label_2.Name = "label_2";
            this.label_2.Size = new System.Drawing.Size(22, 22);
            this.label_2.TabIndex = 5;
            this.label_2.Text = "2";
            this.label_2.Click += new System.EventHandler(this.LabelSudokuNum_Click);
            // 
            // label_3
            // 
            this.label_3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_3.Location = new System.Drawing.Point(68, 3);
            this.label_3.Name = "label_3";
            this.label_3.Size = new System.Drawing.Size(22, 22);
            this.label_3.TabIndex = 4;
            this.label_3.Text = "3";
            this.label_3.Click += new System.EventHandler(this.LabelSudokuNum_Click);
            // 
            // label_4
            // 
            this.label_4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_4.Location = new System.Drawing.Point(96, 3);
            this.label_4.Name = "label_4";
            this.label_4.Size = new System.Drawing.Size(22, 22);
            this.label_4.TabIndex = 3;
            this.label_4.Text = "4";
            this.label_4.Click += new System.EventHandler(this.LabelSudokuNum_Click);
            // 
            // label_8
            // 
            this.label_8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_8.Location = new System.Drawing.Point(208, 3);
            this.label_8.Name = "label_8";
            this.label_8.Size = new System.Drawing.Size(22, 22);
            this.label_8.TabIndex = 2;
            this.label_8.Text = "8";
            this.label_8.Click += new System.EventHandler(this.LabelSudokuNum_Click);
            // 
            // label_9
            // 
            this.label_9.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_9.Location = new System.Drawing.Point(236, 3);
            this.label_9.Name = "label_9";
            this.label_9.Size = new System.Drawing.Size(22, 22);
            this.label_9.TabIndex = 1;
            this.label_9.Text = "9";
            this.label_9.Click += new System.EventHandler(this.LabelSudokuNum_Click);
            // 
            // label_1
            // 
            this.label_1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_1.Location = new System.Drawing.Point(12, 3);
            this.label_1.Name = "label_1";
            this.label_1.Size = new System.Drawing.Size(22, 22);
            this.label_1.TabIndex = 0;
            this.label_1.Text = "1";
            this.label_1.Click += new System.EventHandler(this.LabelSudokuNum_Click);
            // 
            // FormCSudokuClient
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(443, 572);
            this.Controls.Add(this.panelNumbers);
            this.Controls.Add(this.panelControl);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(459, 611);
            this.Name = "FormCSudokuClient";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Конкурентное судоку - КЛИЕНТ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCSudokuClient_FormClosing);
            this.Load += new System.EventHandler(this.FormCSudokuClient_Load);
            this.panelControl.ResumeLayout(false);
            this.panelControl.PerformLayout();
            this.panelNumbers.ResumeLayout(false);
            this.panelNumbers.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelServer;
        private System.Windows.Forms.TextBox textBoxServer;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Panel panelControl;
        private System.Windows.Forms.Panel panelNumbers;
        private System.Windows.Forms.Label label_0;
        private System.Windows.Forms.Label label_7;
        private System.Windows.Forms.Label label_6;
        private System.Windows.Forms.Label label_5;
        private System.Windows.Forms.Label label_2;
        private System.Windows.Forms.Label label_3;
        private System.Windows.Forms.Label label_4;
        private System.Windows.Forms.Label label_8;
        private System.Windows.Forms.Label label_9;
        private System.Windows.Forms.Label label_1;
        private System.Windows.Forms.Label labelPlayerName;
        private System.Windows.Forms.TextBox textBoxPlayerName;
        private System.Windows.Forms.Button buttonShowTop;
        private System.Windows.Forms.Label labelRules;
    }
}

