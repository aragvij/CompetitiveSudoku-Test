using System.Windows.Forms;

namespace cSudokuServer
{
    public partial class FormAddChangeAddress : Form
    {
        public string Address => textBoxAddress.Text.Trim();

        public FormAddChangeAddress(string ipAddress = null)
        {
            InitializeComponent();

            if (ipAddress != null)
                textBoxAddress.Text = ipAddress;
        }
    }
}
