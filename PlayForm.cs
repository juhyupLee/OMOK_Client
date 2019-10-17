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


        class TargetPoint
        {

            public TargetPoint(int x , int y)
            {
                mX = x;
                mY = y;
            }
            public bool IsSame(int x , int y)
            {
                return x == mX && y == mY;
            }
            public int GetX()
            {
                return mX;
            }
            public int GetY()
            {
                return mY;
            }
            public int mX;
            public int mY;
        }
        List<TargetPoint> BlackReposit;
        List<TargetPoint> WhiteReposit;
        Stack<TargetPoint> TotalReposit;

        readonly int LineCount = 15; // 15 X 15 의 바둑판 배열
        readonly int Radian = 10;
        bool bTurn = true;// true 백 false 흑

        public PlayForm()
        {
            InitializeComponent();
            BlackReposit = new List<TargetPoint>();
            WhiteReposit = new List<TargetPoint>();
            TotalReposit = new Stack<TargetPoint>();

        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Graphics gp = this.pictureBox1.CreateGraphics();

           // MessageBox.Show("X:" + e.X + " Y:" + e.Y);

            int cellWidth = pictureBox1.Width / LineCount;

            int restX = e.X % cellWidth;
            int mokX = e.X / cellWidth;

            int restY = e.Y % cellWidth;
            int mokY = e.Y / cellWidth;

            int targetX = -100;
            int targetY = -100;

            //찍을 점 위치 정하기
            if(restX<= Radian)  //ex X가 23이고, 제일가까운 교점이 20일때 
            {
                targetX = e.X - restX;
            }
            else if(restX >=cellWidth- Radian) //ex X가 17이고 제일 가까운 교점이 20일때
            {
                targetX = e.X + (cellWidth - restX);
            }
            else
            {
                return;
            }

            if(restY <= Radian)
            {
                targetY = e.Y - restY;
            }
            else if(restY>= cellWidth - Radian)
            {
                targetY = e.Y + (cellWidth - restY);
            }
            else
            {
                return;
            }
            //이미 선택된 위치에 돌을 놓는것을 방지.
            for(int i=0;i<BlackReposit.Count;++i)
            {
                if(BlackReposit[i].IsSame(targetX,targetY))
                {
                    return;
                }
            }
            for (int i = 0; i < WhiteReposit.Count; ++i)
            {
                if (WhiteReposit[i].IsSame(targetX, targetY))
                {
                    return;
                }
            }

            SolidBrush brush;

            if (bTurn) // 백돌 차례
            {
                brush = new SolidBrush(Color.White);
                bTurn = false;
                WhiteReposit.Add(new TargetPoint(targetX, targetY));
            }
            else    //흑돌차례
            {
                brush = new SolidBrush(Color.Black);
                bTurn = true;
                BlackReposit.Add(new TargetPoint(targetX, targetY));
            }
            TotalReposit.Push(new TargetPoint(targetX, targetY));

            gp.FillEllipse(brush, targetX- (Radian / 2), targetY-(Radian / 2), Radian, Radian);
           if( WinnerCheck())
            {
                MessageBox.Show("이겼습니다");
            }

        }
        private bool WinnerCheck()
        {
            //bool bWin = false;

            if(BlackReposit.Count<5)
            {
                return false;
            }
            int cellWidth = pictureBox1.Width / LineCount;
            int[] rowCount = new int[15];
            int[] colCount = new int[15];
            int[] lrCount =  new int[15];
            int[] rlCount = new int[15];

            List<TargetPoint>[] rowPoint = new List<TargetPoint>[15];
            List<TargetPoint>[] colPoint = new List<TargetPoint>[15];
            List<TargetPoint>[] lrPoint = new List<TargetPoint>[15];
            List<TargetPoint>[] rlPoint = new List<TargetPoint>[15];
            
            for (int i=0;i<15;++i)
            {
                rowPoint[i] = new List<TargetPoint>();
                colPoint[i] = new List<TargetPoint>();
                lrPoint[i] = new List<TargetPoint>();
                rlPoint[i] = new List<TargetPoint>();
            }
         
            List<int> rowIndex = new List<int>();
            List<int> colIndex = new List<int>();
            List<int> lrIndex = new List<int>();
            List<int> rlIndex = new List<int>();

            //5개 바둑돌있는 라인 조사해서 5개있는 라인 기록
            int lineIndex = 0;

            for(int i=0; i<=pictureBox1.Width;i+=cellWidth,++lineIndex)
            {
                for(int j=0; j<BlackReposit.Count;++j)
                {
                    if(BlackReposit[j].GetX() ==i)
                    {
                        ++colCount[lineIndex];
                        colPoint[lineIndex].Add(BlackReposit[j]);
                    }
                    if(BlackReposit[j].GetY()==i)
                    {
                        ++rowCount[lineIndex];
                        rowPoint[lineIndex].Add(BlackReposit[j]);
                    }
                    if(BlackReposit[j].GetX()==i && BlackReposit[j].GetY()==i)
                    {
                        ++lrCount[lineIndex];
                        lrPoint[lineIndex].Add(BlackReposit[j]);
                    }
                    if(BlackReposit[j].GetX()==(pictureBox1.Width-i) && BlackReposit[j].GetY()==i)
                    {
                        ++rlCount[lineIndex];
                        rlPoint[lineIndex].Add(BlackReposit[j]);
                    }
                }
            }

            int seriesCount = 0;

            for(int index1 = 0;index1 < LineCount;++index1)
            {
                if(rowPoint[index1].Count>=5)
                {
                    rowPoint[index1].Sort(delegate (TargetPoint A, TargetPoint B)
                    {
                        return A.mX.CompareTo(B.mX);
                    });
                    for(int index2=0; index2 < rowPoint[index1].Count-1;++index2)
                    {
                        for(int index3=index2;index3<rowPoint[index1].Count-1;++index3)
                        {
                            if((rowPoint[index1][index3+1]).GetX() - (rowPoint[index1][index3]).GetX() == cellWidth)
                            {
                                ++seriesCount;
                            }
                            else
                            {
                                seriesCount = 0;
                                continue;
                            }
                        }
                        if (seriesCount == 4)
                        {
                            return true;
                        }
                        else
                        {
                            seriesCount = 0;
                        }
                    }
                }
                if(colPoint[index1].Count>=5)
                {
                    colPoint[index1].Sort(delegate (TargetPoint A, TargetPoint B)
                    {
                        return A.mY.CompareTo(B.mY);
                    });
                    for (int index2 = 0; index2 < colPoint[index1].Count - 1; ++index2)
                    {
                        for (int index3 = index2; index3 < colPoint[index1].Count - 1; ++index3)
                        {
                            if ((colPoint[index1][index3 + 1]).GetY() - (colPoint[index1][index3]).GetY() == cellWidth)
                            {
                                ++seriesCount;
                            }
                            else
                            {
                                seriesCount = 0;
                                continue;
                            }
                        }
                        if(seriesCount==4)
                        {
                            return true;
                        }
                        else
                        {
                            seriesCount = 0;
                        }
                    }
                }
                if(lrPoint[index1].Count>=5)
                {
                    //lrIndex.Add(i);
                }
                if(rlPoint[index1].Count>=5)
                {
                    //rlIndex.Add(i);
                }
            }

            return false ;

        }

        private bool SeriesCheck(string line,int index,int count)
        {
            //bool isSeries = false;
            TargetPoint[] targetArray = new TargetPoint[count];


            switch(line)
            {
                case "Row":
                    for(int i=0;i<=pictureBox1.Width;i+=(pictureBox1.Width / LineCount))
                    {
                        
                    }
                    break;
                case "Col":
                    break;
                case "RL":
                    break;
                case "LR":
                    break;
                default:
                    break;
            }
            return false;
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {

            Graphics gp = e.Graphics;
            Color lineColor = Color.Black;
            Pen pen = new Pen(lineColor, 2);

            for(int i=0;i<=pictureBox1.Width;i+=(pictureBox1.Width/LineCount))
            {
                gp.DrawLine(pen, 0, i, pictureBox1.Width, i); // 가로줄 긋기
                gp.DrawLine(pen, i, 0, i, pictureBox1.Width); // 세로줄 긋기
            }

        }
    }
}
