using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KonbiBrain.RfidTable.TakeAwayPrice
{
    public partial class FormMessageBox : Form
    {
        /// <summary>
        /// InitializeComponent
        /// </summary>
        /// <param name="_isError"></param>
        /// <param name="_message"></param>
        public FormMessageBox(bool _isError, string _message)
        {
            InitializeComponent();
            labelMessage.Text = _message;
            if (_isError)
            {
                iconError.Visible = true;
                iconSuccess.Visible = false;
            }
            else
            {
                iconError.Visible = false;
                iconSuccess.Visible = true;
            }
        }

        /// <summary>
        /// Click button OK.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public static void showMessageBox(bool _isError, string _message)
        {
            FormMessageBox _form = new KonbiBrain.RfidTable.TakeAwayPrice.FormMessageBox(_isError, _message);
            _form.ShowDialog();
        }

        /// <summary>
        /// Enter key press.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check click enter.
            if (e.KeyChar == (char)13)
            {
                this.Close();
            }
        }
    }
}
