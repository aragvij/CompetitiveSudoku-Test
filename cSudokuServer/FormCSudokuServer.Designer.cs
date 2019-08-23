namespace cSudokuServer
{
    partial class FormCSudokuServer
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
            this.checkedListBoxAddresses = new System.Windows.Forms.CheckedListBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.groupBoxAddresses = new System.Windows.Forms.GroupBox();
            this.panelAddressControl = new System.Windows.Forms.Panel();
            this.buttonAddressDelete = new System.Windows.Forms.Button();
            this.buttonAddressChange = new System.Windows.Forms.Button();
            this.buttonAddressAdd = new System.Windows.Forms.Button();
            this.buttonStartStop = new System.Windows.Forms.Button();
            this.splitContainerServer = new System.Windows.Forms.SplitContainer();
            this.panelServerControl = new System.Windows.Forms.Panel();
            this.groupBoxCurrentPlayers = new System.Windows.Forms.GroupBox();
            this.listBoxCurrentPlayers = new System.Windows.Forms.ListBox();
            this.groupBoxAddresses.SuspendLayout();
            this.panelAddressControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerServer)).BeginInit();
            this.splitContainerServer.Panel1.SuspendLayout();
            this.splitContainerServer.Panel2.SuspendLayout();
            this.splitContainerServer.SuspendLayout();
            this.panelServerControl.SuspendLayout();
            this.groupBoxCurrentPlayers.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkedListBoxAddresses
            // 
            this.checkedListBoxAddresses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListBoxAddresses.FormattingEnabled = true;
            this.checkedListBoxAddresses.Location = new System.Drawing.Point(3, 22);
            this.checkedListBoxAddresses.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkedListBoxAddresses.Name = "checkedListBoxAddresses";
            this.checkedListBoxAddresses.ScrollAlwaysVisible = true;
            this.checkedListBoxAddresses.Size = new System.Drawing.Size(343, 404);
            this.checkedListBoxAddresses.TabIndex = 0;
            this.checkedListBoxAddresses.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CheckedListBoxAddresses_ItemCheck);
            this.checkedListBoxAddresses.SelectedIndexChanged += new System.EventHandler(this.CheckedListBoxAddresses_SelectedIndexChanged);
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(158, 9);
            this.labelPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(69, 20);
            this.labelPort.TabIndex = 1;
            this.labelPort.Text = "IP-порт:";
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(234, 6);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(103, 26);
            this.textBoxPort.TabIndex = 2;
            this.textBoxPort.TextChanged += new System.EventHandler(this.TextBoxPort_TextChanged);
            // 
            // groupBoxAddresses
            // 
            this.groupBoxAddresses.Controls.Add(this.checkedListBoxAddresses);
            this.groupBoxAddresses.Controls.Add(this.panelAddressControl);
            this.groupBoxAddresses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxAddresses.Location = new System.Drawing.Point(0, 37);
            this.groupBoxAddresses.Name = "groupBoxAddresses";
            this.groupBoxAddresses.Size = new System.Drawing.Size(349, 466);
            this.groupBoxAddresses.TabIndex = 1;
            this.groupBoxAddresses.TabStop = false;
            this.groupBoxAddresses.Text = "IP адреса или имена:";
            // 
            // panelAddressControl
            // 
            this.panelAddressControl.Controls.Add(this.buttonAddressDelete);
            this.panelAddressControl.Controls.Add(this.buttonAddressChange);
            this.panelAddressControl.Controls.Add(this.buttonAddressAdd);
            this.panelAddressControl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelAddressControl.Location = new System.Drawing.Point(3, 426);
            this.panelAddressControl.Name = "panelAddressControl";
            this.panelAddressControl.Size = new System.Drawing.Size(343, 37);
            this.panelAddressControl.TabIndex = 1;
            // 
            // buttonAddressDelete
            // 
            this.buttonAddressDelete.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonAddressDelete.Enabled = false;
            this.buttonAddressDelete.ForeColor = System.Drawing.Color.Red;
            this.buttonAddressDelete.Location = new System.Drawing.Point(220, 0);
            this.buttonAddressDelete.Name = "buttonAddressDelete";
            this.buttonAddressDelete.Size = new System.Drawing.Size(110, 37);
            this.buttonAddressDelete.TabIndex = 2;
            this.buttonAddressDelete.Text = "Удалить";
            this.buttonAddressDelete.UseVisualStyleBackColor = true;
            this.buttonAddressDelete.Click += new System.EventHandler(this.ButtonAddressDelete_Click);
            // 
            // buttonAddressChange
            // 
            this.buttonAddressChange.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonAddressChange.Enabled = false;
            this.buttonAddressChange.ForeColor = System.Drawing.Color.Blue;
            this.buttonAddressChange.Location = new System.Drawing.Point(110, 0);
            this.buttonAddressChange.Name = "buttonAddressChange";
            this.buttonAddressChange.Size = new System.Drawing.Size(110, 37);
            this.buttonAddressChange.TabIndex = 1;
            this.buttonAddressChange.Text = "Изменить";
            this.buttonAddressChange.UseVisualStyleBackColor = true;
            this.buttonAddressChange.Click += new System.EventHandler(this.ButtonAddressChange_Click);
            // 
            // buttonAddressAdd
            // 
            this.buttonAddressAdd.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonAddressAdd.ForeColor = System.Drawing.Color.Green;
            this.buttonAddressAdd.Location = new System.Drawing.Point(0, 0);
            this.buttonAddressAdd.Name = "buttonAddressAdd";
            this.buttonAddressAdd.Size = new System.Drawing.Size(110, 37);
            this.buttonAddressAdd.TabIndex = 0;
            this.buttonAddressAdd.Text = "Добавить";
            this.buttonAddressAdd.UseVisualStyleBackColor = true;
            this.buttonAddressAdd.Click += new System.EventHandler(this.ButtonAddressAdd_Click);
            // 
            // buttonStartStop
            // 
            this.buttonStartStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.buttonStartStop.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonStartStop.Enabled = false;
            this.buttonStartStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.buttonStartStop.ForeColor = System.Drawing.Color.White;
            this.buttonStartStop.Location = new System.Drawing.Point(0, 0);
            this.buttonStartStop.Name = "buttonStartStop";
            this.buttonStartStop.Size = new System.Drawing.Size(151, 37);
            this.buttonStartStop.TabIndex = 0;
            this.buttonStartStop.Text = "С т а р т";
            this.buttonStartStop.UseVisualStyleBackColor = false;
            this.buttonStartStop.Click += new System.EventHandler(this.ButtonStartStop_Click);
            // 
            // splitContainerServer
            // 
            this.splitContainerServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerServer.Location = new System.Drawing.Point(0, 0);
            this.splitContainerServer.Name = "splitContainerServer";
            // 
            // splitContainerServer.Panel1
            // 
            this.splitContainerServer.Panel1.Controls.Add(this.groupBoxAddresses);
            this.splitContainerServer.Panel1.Controls.Add(this.panelServerControl);
            // 
            // splitContainerServer.Panel2
            // 
            this.splitContainerServer.Panel2.Controls.Add(this.groupBoxCurrentPlayers);
            this.splitContainerServer.Size = new System.Drawing.Size(684, 503);
            this.splitContainerServer.SplitterDistance = 349;
            this.splitContainerServer.TabIndex = 6;
            // 
            // panelServerControl
            // 
            this.panelServerControl.Controls.Add(this.buttonStartStop);
            this.panelServerControl.Controls.Add(this.labelPort);
            this.panelServerControl.Controls.Add(this.textBoxPort);
            this.panelServerControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelServerControl.Location = new System.Drawing.Point(0, 0);
            this.panelServerControl.Name = "panelServerControl";
            this.panelServerControl.Size = new System.Drawing.Size(349, 37);
            this.panelServerControl.TabIndex = 0;
            // 
            // groupBoxCurrentPlayers
            // 
            this.groupBoxCurrentPlayers.Controls.Add(this.listBoxCurrentPlayers);
            this.groupBoxCurrentPlayers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCurrentPlayers.Location = new System.Drawing.Point(0, 0);
            this.groupBoxCurrentPlayers.Name = "groupBoxCurrentPlayers";
            this.groupBoxCurrentPlayers.Size = new System.Drawing.Size(331, 503);
            this.groupBoxCurrentPlayers.TabIndex = 1;
            this.groupBoxCurrentPlayers.TabStop = false;
            this.groupBoxCurrentPlayers.Text = "Текущие игроки:";
            // 
            // listBoxCurrentPlayers
            // 
            this.listBoxCurrentPlayers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxCurrentPlayers.FormattingEnabled = true;
            this.listBoxCurrentPlayers.ItemHeight = 20;
            this.listBoxCurrentPlayers.Location = new System.Drawing.Point(3, 22);
            this.listBoxCurrentPlayers.Name = "listBoxCurrentPlayers";
            this.listBoxCurrentPlayers.Size = new System.Drawing.Size(325, 478);
            this.listBoxCurrentPlayers.TabIndex = 0;
            // 
            // FormCSudokuServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 503);
            this.Controls.Add(this.splitContainerServer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(700, 180);
            this.Name = "FormCSudokuServer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Конкурентное судоку - СЕРВЕР";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCSudokuServer_FormClosing);
            this.Load += new System.EventHandler(this.FormCSudokuServer_Load);
            this.groupBoxAddresses.ResumeLayout(false);
            this.panelAddressControl.ResumeLayout(false);
            this.splitContainerServer.Panel1.ResumeLayout(false);
            this.splitContainerServer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerServer)).EndInit();
            this.splitContainerServer.ResumeLayout(false);
            this.panelServerControl.ResumeLayout(false);
            this.panelServerControl.PerformLayout();
            this.groupBoxCurrentPlayers.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBoxAddresses;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.GroupBox groupBoxAddresses;
        private System.Windows.Forms.Panel panelAddressControl;
        private System.Windows.Forms.Button buttonAddressDelete;
        private System.Windows.Forms.Button buttonAddressChange;
        private System.Windows.Forms.Button buttonAddressAdd;
        private System.Windows.Forms.Button buttonStartStop;
        private System.Windows.Forms.SplitContainer splitContainerServer;
        private System.Windows.Forms.Panel panelServerControl;
        private System.Windows.Forms.GroupBox groupBoxCurrentPlayers;
        private System.Windows.Forms.ListBox listBoxCurrentPlayers;
    }
}

