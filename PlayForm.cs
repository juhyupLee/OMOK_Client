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
    public partial class PlayForm : Form
    {
        readonly int LineCount = 15;
        
        public PlayForm()
        {
            InitializeComponent();
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            Graphics gp = this.pictureBox1.CreateGraphics();


            MessageBox.Show("X:" + e.X + " Y:" + e.Y);

            int cellWidth = pictureBox1.Width / LineCount;

            if ((e.X % cellWidth )<15  && (e.Y % cellWidth)<15)
            {
                SolidBrush brush = new SolidBrush(Color.Black);
                gp.FillEllipse(brush, e.X-5, e.Y-5, 10, 10);
            }
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {

            Graphics gp = e.Graphics;
            Color lineColor = Color.Black;
            Pen pen = new Pen(lineColor, 2);
            int picX = pictureBox1.Width;
            int picY = pictureBox1.Width;
           

            for(int i=0;i<=pictureBox1.Width;i+=(pictureBox1.Width/LineCount))
            {
                gp.DrawLine(pen, 0, i, 300, i); // 가로줄 긋기
                gp.DrawLine(pen, i, 0, i, 300); // 세로줄 긋기
            }

        }
    }
}
