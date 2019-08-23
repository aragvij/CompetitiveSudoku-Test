using cSudokuCommonLib;
using cSudokuServerLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace cSudokuServer
{
    public partial class FormCSudokuServer : Form
    {
        int? port = 80;
        bool isRun = false;
        CsServer server;

        public FormCSudokuServer()
        {
            InitializeComponent();
            textBoxPort.Text = port.ToString();
        }

        private void FormCSudokuServer_Load(object sender, EventArgs e)
        {
            string[] addr = CsServer.GetAddressList();
            checkedListBoxAddresses.Items.AddRange(addr);
        }
        private void CheckedListBoxAddresses_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            int delta = e.NewValue == CheckState.Checked ? 1 : -1;
            CheckForStart(checkedListBoxAddresses.CheckedItems.Count + delta);
        }
        private void TextBoxPort_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxPort.Text, out int portInt))
            {
                port = portInt;
            }
            else
                port = null;

            CheckForStart(checkedListBoxAddresses.CheckedItems.Count);
        }
        private void CheckForStart(int checkedAddrNum)
        {
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
                    server?.Dispose();
                }
                else
                {
                    string[] addresses = new string[checkedListBoxAddresses.CheckedItems.Count];
                    checkedListBoxAddresses.CheckedItems.CopyTo(addresses, 0);
                    server = new CsServer((int)port, addresses, ShowError, AddPlayer);

                    server.Start();
                }

                SetControlsStartStop();
            }
            catch (Exception ex)
            {
                ShowError(ex.AllMessages());
            }
        }
        private void ShowError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void AddPlayer(List<string> playerNames)
        {
            listBoxCurrentPlayers.BeginUpdate();
            listBoxCurrentPlayers.Items.Clear();
            playerNames.ForEach(
                playerName => listBoxCurrentPlayers.Items.Add(playerName));
            listBoxCurrentPlayers.EndUpdate();
        }
        private void SetControlsStartStop()
        {
            if (isRun)
            {
                buttonStartStop.Text = "С т а р т";
                buttonStartStop.BackColor = Color.Green;
            }
            else
            {
                buttonStartStop.Text = "С т о п";
                buttonStartStop.BackColor = Color.Red;
            }
            textBoxPort.Enabled = groupBoxAddresses.Enabled = isRun;
            isRun = !isRun;
        }
        private void CheckedListBoxAddresses_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonAddressChange.Enabled =
            buttonAddressDelete.Enabled =
                checkedListBoxAddresses.SelectedItems.Count == 1;
        }
        private void ButtonAddressAdd_Click(object sender, EventArgs e)
        {
            ShowAddressEdit(null);
        }
        private void ButtonAddressChange_Click(object sender, EventArgs e)
        {
            checkedListBoxAddresses.BeginUpdate();
            ShowAddressEdit(checkedListBoxAddresses.SelectedItem as string);
            checkedListBoxAddresses.EndUpdate();
        }
        private void ShowAddressEdit(string IpAddress)
        {
            var formAdd = new FormAddChangeAddress(IpAddress);
            if (formAdd.ShowDialog() == DialogResult.OK)
            {
                var address = formAdd.Address;
                if (address.Length > 0)
                {
                    if (null == IpAddress)
                    {
                        // добавление нового
                        checkedListBoxAddresses.Items.Add(address, true);
                    }
                    else
                    {
                        // изменение существующего
                        checkedListBoxAddresses.Items[checkedListBoxAddresses.SelectedIndex] = address;
                    }
                }
            }
        }
        private void ButtonAddressDelete_Click(object sender, EventArgs e)
        {
            checkedListBoxAddresses.Items.RemoveAt(checkedListBoxAddresses.SelectedIndex);
            CheckForStart(checkedListBoxAddresses.CheckedItems.Count);
        }
        private void FormCSudokuServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            server?.Dispose();
        }
    }
}
