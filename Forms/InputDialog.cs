using System;
using System.Windows.Forms;

namespace HyperVMs
{
    public partial class InputDialog : Form
    {
        public InputDialog()
        {
            InitializeComponent();
        }

        private void CloseInputDialog(object sender, EventArgs e)
        {
            Close();
        }

        public static string ShowInputDialog()
        {
            var inputDialog = new InputDialog();
            return inputDialog.ShowDialog() == DialogResult.OK ? inputDialog.textInput.Text : "";
        }
    }
}
