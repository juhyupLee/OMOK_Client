using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OMOK_Client
{
    public partial class MenuForm : Form
    {
        public MenuForm()
        {
            InitializeComponent();
        }

        private void SinglePlayBtn_Click(object sender, EventArgs e)
        {
            Hide();
            PlayForm playForm = new PlayForm();
            playForm.FormClosed += new FormClosedEventHandler(childForm_Closed);
            playForm.Show();
        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
        void childForm_Closed(object sender, FormClosedEventArgs e)
        {
            Show();
        }

     
    }
}
