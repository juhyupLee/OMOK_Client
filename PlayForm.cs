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

            int radian = 10;

           // MessageBox.Show("X:" + e.X + " Y:" + e.Y);

            int cellWidth = pictureBox1.Width / LineCount;

            int restX = e.X % cellWidth;
            int mokX = e.X / cellWidth;

            int restY = e.Y % cellWidth;
            int mokY = e.Y / cellWidth;

            int targetX = -100;
            int targetY = -100;

            if(restX<= radian)  //ex X: 23 ---
            {
                targetX = e.X - restX;
            }
            else if(restX >=cellWidth-radian)
            {
                targetX = e.X + (20 - restX);

            }

            if(restY <= radian)
            {
                targetY = e.Y - restY;
            }
            else if(restY>= cellWidth - radian)
            {
                targetY = e.Y + (20 - restY);

            }
                SolidBrush brush = new SolidBrush(Color.Black);
                gp.FillEllipse(brush, targetX- (radian/2), targetY-(radian/2), radian, radian);
            
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
