using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
// 소켓과 관련덴 네임스페이스

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

            string[] inputIP = textBox1.Text.Split('.');

            if(inputIP.Count()<4)
            {
                MessageBox.Show("IP 주소를 제대로 입력하십시오.");
                return;
            }
            ip_Address = textBox1.Text;

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

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
       
        }
    }
}
