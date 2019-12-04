﻿using System;
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
    public partial class PlayForm : Form
    {


        class TargetPoint
        {
            public TargetPoint(int x, int y)
            {
                mX = x;
                mY = y;
            }
            public bool IsSame(int x, int y)
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

        enum STONE
        {
            WHITE =1,
            BLACK =2
        };
        List<TargetPoint> BlackReposit;
        List<TargetPoint> WhiteReposit;
        Stack<TargetPoint> TotalReposit;

        //소켓관련변수
        Socket hClntSock;
        IPEndPoint servAddr;
        byte[] buf;

        readonly int BUF_SIZE = 1024;

        readonly int LineCount = 15; // 15 X 15 의 바둑판 배열
        readonly int Radian = 10;
        bool bTurn = true;// true 백 false 흑
        STONE stone = 0;
        BackgroundWorker worker;

        public PlayForm()
        {
            InitializeComponent();
            BlackReposit = new List<TargetPoint>();
            WhiteReposit = new List<TargetPoint>();
            TotalReposit = new Stack<TargetPoint>();
            //TCP IP 소켓통신
            hClntSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            servAddr = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);

            buf = new byte[1024];

            worker = new BackgroundWorker();

            worker.DoWork += new DoWorkEventHandler(OtherTurn);
            worker.RunWorkerAsync();

    

            try
            {
                hClntSock.Connect(servAddr);
               
            }
            catch
            {
                MessageBox.Show("Unable to conntect......");

            }

            int strLen = hClntSock.Receive(buf, 0, BUF_SIZE-1, 0);


            string recvStr = Encoding.UTF8.GetString(buf);
            recvStr = recvStr.Replace("\0", "");


            //서버와 연결시, 서버로부터 흰/백을 부여받는다.
            //string recvStr = Encoding.UTF8.GetString(buf).ToString();



            if(recvStr != "")
            {
                if(recvStr == "White")
                {
                    stone = STONE.WHITE; // Whilte 1 Black 2
                    bTurn = false;
                }
                else if(recvStr == "Black")
                {
                    stone = STONE.BLACK;
                    bTurn = true; // Black is First!

                }
            }

        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            if(bTurn==false) // 차례를 부여받지 못하면 바둑돌을 놓을수 없다.
            {
                return;
            }
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
            if (restX <= Radian)  //ex X가 23이고, 제일가까운 교점이 20일때 
            {
                targetX = e.X - restX;
            }
            else if (restX >= cellWidth - Radian) //ex X가 17이고 제일 가까운 교점이 20일때
            {
                targetX = e.X + (cellWidth - restX);
            }
            else
            {
                return;
            }

            if (restY <= Radian)
            {
                targetY = e.Y - restY;
            }
            else if (restY >= cellWidth - Radian)
            {
                targetY = e.Y + (cellWidth - restY);
            }
            else
            {
                return;
            }
            //이미 선택된 위치에 돌을 놓는것을 방지.
            for (int i = 0; i < BlackReposit.Count; ++i)
            {
                if (BlackReposit[i].IsSame(targetX, targetY))
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

            if (stone ==STONE.WHITE ) // 클라가 백돌이고, 차례까지 받았다면
            {
                brush = new SolidBrush(Color.White);
                //bTurn = false;
                //string temp = "W," + targetX.ToString() + "," + targetY.ToString();
                //buf = Encoding.Default.GetBytes(temp);
                buf[0] = (byte)'W';
                buf[1] = (byte)targetX;
                buf[5] = (byte)targetY;

                hClntSock.Send(buf);
                bTurn = false;

                WhiteReposit.Add(new TargetPoint(targetX, targetY));
            }
            else    //흑돌차례
            {
                brush = new SolidBrush(Color.Black);
                //bTurn = true;
                //string temp = "B," + targetX.ToString() + "," + targetY.ToString();

                buf[0] = (byte)'B';
                buf[1] = (byte)targetX;
                buf[5] = (byte)targetY;
                //1+4+4 = 9byte

                //buf = Encoding.Default.GetBytes(temp);
                hClntSock.Send(buf);

                //int strLen = hClntSock.Receive(buf);
                //string recvStr = Encoding.Default.GetString(buf);
                //string [] recvData = recvStr.Split(','); 

                //if(recvData[0]=="B")
                //{
                    bTurn = false;
                //}
                
                BlackReposit.Add(new TargetPoint(targetX, targetY));

            }
            TotalReposit.Push(new TargetPoint(targetX, targetY));

            gp.FillEllipse(brush, targetX - (Radian / 2), targetY - (Radian / 2), Radian, Radian);
            if (WinnerCheck(ref BlackReposit))
            {
                MessageBox.Show("흑이 이겼습니다");
            }
            if (WinnerCheck(ref WhiteReposit))
            {
                MessageBox.Show("백이 이겼습니다");
            }

        }
        private bool WinnerCheck(ref List<TargetPoint> reposit)
        {
            if (reposit.Count < 5)
            {
                return false;
            }
            int cellWidth = pictureBox1.Width / LineCount;

            List<TargetPoint>[] rowPoint = new List<TargetPoint>[LineCount + 1];
            List<TargetPoint>[] colPoint = new List<TargetPoint>[LineCount + 1];
            List<TargetPoint>[] lrPoint_High = new List<TargetPoint>[LineCount + 1];
            List<TargetPoint>[] lrPoint_Low = new List<TargetPoint>[LineCount + 1];
            List<TargetPoint>[] rlPoint_High = new List<TargetPoint>[LineCount + 1];
            List<TargetPoint>[] rlPoint_Low = new List<TargetPoint>[LineCount + 1];

            for (int i = 0; i < LineCount + 1; ++i)
            {
                rowPoint[i] = new List<TargetPoint>();
                colPoint[i] = new List<TargetPoint>();
                lrPoint_High[i] = new List<TargetPoint>();
                lrPoint_Low[i] = new List<TargetPoint>();
                rlPoint_High[i] = new List<TargetPoint>();
                rlPoint_Low[i] = new List<TargetPoint>();
            }

            List<int> rowIndex = new List<int>();
            List<int> colIndex = new List<int>();
            List<int> lrIndex = new List<int>();
            List<int> rlIndex = new List<int>();

            //5개 바둑돌있는 라인 조사해서 5개있는 라인 기록
            int lineIndex = 0;

            for (int i = 0; i <= pictureBox1.Width; i += cellWidth, ++lineIndex)
            {
                for (int j = 0; j < reposit.Count; ++j)
                {
                    if (reposit[j].GetX() == i) // Row
                    {
                        colPoint[lineIndex].Add(reposit[j]);
                    }
                    if (reposit[j].GetY() == i) // Col
                    {
                        rowPoint[lineIndex].Add(reposit[j]);
                    }

                    //LR 대각선위에  조사
                    for (int k = 0; k <= lineIndex; ++k)
                    {
                        int tempX = lineIndex - k;
                        int tempY = k * cellWidth;
                        if (reposit[j].GetX() == pictureBox1.Width - (cellWidth * tempX)
                           && reposit[j].GetY() == tempY)
                        {
                            lrPoint_High[lineIndex].Add(reposit[j]);
                        }
                    }
                    //LR 대각선아래 조사
                    for (int k = 0; k <= lineIndex; ++k)
                    {
                        int tempX = k * cellWidth;
                        int tempY = lineIndex - k;
                        if (reposit[j].GetX() == tempX
                           && reposit[j].GetY() == pictureBox1.Width - (cellWidth * tempY))
                        {
                            lrPoint_Low[lineIndex].Add(reposit[j]);
                        }
                    }
                    //RL 대각선위에 조사
                    for (int k = 0; k <= lineIndex; ++k)
                    {
                        int tempX = k * cellWidth;
                        int tempY = lineIndex - k;
                        if (reposit[j].GetX() == tempX
                           && reposit[j].GetY() == cellWidth * tempY)
                        {
                            rlPoint_High[lineIndex].Add(reposit[j]);
                        }
                    }
                    //RL 대각선 아래 조사
                    for (int k = 0; k <= lineIndex; ++k)
                    {
                        int tempX = k * cellWidth;
                        int tempY = lineIndex - k;
                        if (reposit[j].GetX() == pictureBox1.Width - tempX
                           && reposit[j].GetY() == pictureBox1.Width - (cellWidth * tempY))
                        {
                            rlPoint_Low[lineIndex].Add(reposit[j]);
                        }
                    }
                }


            }

            int seriesCount = 0;
            int firstDoll = -1;

            for (int index1 = 0; index1 < LineCount + 1; ++index1)
            {
                if (rowPoint[index1].Count >= 5)
                {
                    rowPoint[index1].Sort(delegate (TargetPoint A, TargetPoint B) // 놓인순서대로 되있는 rowPoint를 x값을 기준으로 소팅하기
                    {
                        return A.mX.CompareTo(B.mX);
                    });
                    for (int index2 = 0; index2 < rowPoint[index1].Count - 1; ++index2) //해당 라인에서 놓인 돌0번에서 ~ 마지막 전까지
                    {
                        for (int index3 = index2; index3 < rowPoint[index1].Count - 1; ++index3)//ㅇㅇㅇㅇㅇ5개 놓였다고했을때, 
                        {
                            if ((rowPoint[index1][index3 + 1]).GetX() - (rowPoint[index1][index3]).GetX() == cellWidth) //연속된 돌의차이가 cellwidth면 시리즈카운트를 올린다.
                            {
                                ++seriesCount;
                            }
                            else
                            {
                                seriesCount = 0;
                                continue;
                            }
                            if (seriesCount == 1)
                            {
                                firstDoll = index3;  //첫번째 돌의 index 기억하기.
                            }
                            if (seriesCount == 4)
                            {
                                if (firstDoll == 0)
                                {
                                    if (index3 + 2 > rowPoint[index1].Count - 1)//인덱스 범위를 초과하면 뒷돌이 없다는뜻임.
                                    {
                                        return true;
                                    }
                                    else //그렇지 않으면
                                    {
                                        if (rowPoint[index1][index3 + 2].GetX() - rowPoint[index1][index3 + 1].GetX() == cellWidth) //뒷돌이 더있어 6목이라면
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }
                                        else
                                        {
                                            return true; // Win
                                        }
                                    }
                                }
                                else
                                {
                                    if (index3 + 2 > rowPoint[index1].Count - 1)//인덱스 범위를 초과하면, 뒷돌이 없다는뜻임. 앞돌만 조사
                                    {

                                        //5개돌 앞돌 조사해서 아무것도 없다면 오목!!
                                        if ((rowPoint[index1][firstDoll].GetX()) - (rowPoint[index1][firstDoll - 1].GetX()) != cellWidth)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }
                                    }
                                    else //그렇지 않다면 뒷돌이 있을 가능성이 있음.
                                    {
                                        //5개돌 뒷돌, 앞돌 조사해서 아무것도 없다면 오목!!
                                        if ((rowPoint[index1][index3 + 2].GetX()) - (rowPoint[index1][index3 + 1].GetX()) != cellWidth
                                           && (rowPoint[index1][firstDoll].GetX()) - (rowPoint[index1][firstDoll - 1].GetX()) != cellWidth)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }

                                    }
                                }


                            }
                        }
                        seriesCount = 0;
                        firstDoll = -1;


                    }
                }
                if (colPoint[index1].Count >= 5)
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
                            if (seriesCount == 1)
                            {
                                firstDoll = index3;  //첫번째 돌의 index 기억하기.
                            }
                            if (seriesCount == 4)
                            {
                                if (firstDoll == 0)
                                {
                                    if (index3 + 2 > colPoint[index1].Count - 1)//인덱스 범위를 초과하면 뒷돌이 없다는뜻임.
                                    {
                                        return true;
                                    }
                                    else //그렇지 않으면
                                    {
                                        if (colPoint[index1][index3 + 2].GetY() - colPoint[index1][index3 + 1].GetY() == cellWidth) //뒷돌이 더있어 6목이라면
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }
                                        else
                                        {
                                            return true; // Win
                                        }
                                    }
                                }
                                else
                                {
                                    if (index3 + 2 > colPoint[index1].Count - 1)//인덱스 범위를 초과하면, 뒷돌이 없다는뜻임. 앞돌만 조사
                                    {

                                        //5개돌 앞돌 조사해서 아무것도 없다면 오목!!
                                        if ((colPoint[index1][firstDoll].GetY()) - (colPoint[index1][firstDoll - 1].GetY()) != cellWidth)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }
                                    }
                                    else //그렇지 않다면 뒷돌이 있을 가능성이 있음.
                                    {
                                        //5개돌 뒷돌, 앞돌 조사해서 아무것도 없다면 오목!!
                                        if ((colPoint[index1][index3 + 2].GetY()) - (colPoint[index1][index3 + 1].GetY()) != cellWidth
                                           && (colPoint[index1][firstDoll].GetY()) - (colPoint[index1][firstDoll - 1].GetY()) != cellWidth)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }

                                    }
                                }


                            }
                        }
                        seriesCount = 0;
                        firstDoll = -1;

                    }
                }
                if (lrPoint_High[index1].Count >= 5)
                {
                    lrPoint_High[index1].Sort(delegate (TargetPoint A, TargetPoint B) // 놓인순서대로 되있는 lrPoint를 x값을 기준으로 소팅하기
                    {
                        return A.mX.CompareTo(B.mX);
                    });
                    for (int index2 = 0; index2 < lrPoint_High[index1].Count - 1; ++index2) //해당 라인에서 놓인 돌0번에서 ~ 마지막 전까지
                    {
                        for (int index3 = index2; index3 < lrPoint_High[index1].Count - 1; ++index3)//ㅇㅇㅇㅇㅇ5개 놓였다고했을때, 
                        {
                            if ((lrPoint_High[index1][index3 + 1]).GetX() - (lrPoint_High[index1][index3]).GetX() == cellWidth) //연속된 돌의차이가 cellwidth면 시리즈카운트를 올린다.
                            {
                                ++seriesCount;
                            }
                            else
                            {
                                seriesCount = 0;
                                continue;
                            }
                            if (seriesCount == 1)
                            {
                                firstDoll = index3;  //첫번째 돌의 index 기억하기.
                            }
                            if (seriesCount == 4)
                            {
                                if (firstDoll == 0)
                                {
                                    if (index3 + 2 > lrPoint_High[index1].Count - 1)//인덱스 범위를 초과하면 뒷돌이 없다는뜻임.
                                    {
                                        return true;
                                    }
                                    else //그렇지 않으면
                                    {
                                        if (lrPoint_High[index1][index3 + 2].GetX() - lrPoint_High[index1][index3 + 1].GetX() == cellWidth) //뒷돌이 더있어 6목이라면
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }
                                        else
                                        {
                                            return true; // Win
                                        }
                                    }
                                }
                                else
                                {
                                    if (index3 + 2 > lrPoint_High[index1].Count - 1)//인덱스 범위를 초과하면, 뒷돌이 없다는뜻임. 앞돌만 조사
                                    {

                                        //5개돌 앞돌 조사해서 아무것도 없다면 오목!!
                                        if ((lrPoint_High[index1][firstDoll].GetX()) - (lrPoint_High[index1][firstDoll - 1].GetX()) != cellWidth)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }
                                    }
                                    else //그렇지 않다면 뒷돌이 있을 가능성이 있음.
                                    {
                                        //5개돌 뒷돌, 앞돌 조사해서 아무것도 없다면 오목!!
                                        if ((lrPoint_High[index1][index3 + 2].GetX()) - (lrPoint_High[index1][index3 + 1].GetX()) != cellWidth
                                           && (lrPoint_High[index1][firstDoll].GetX()) - (lrPoint_High[index1][firstDoll - 1].GetX()) != cellWidth)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }

                                    }
                                }


                            }
                        }
                        seriesCount = 0;
                        firstDoll = -1;


                    }
                }
                if (lrPoint_Low[index1].Count >= 5)
                {
                    lrPoint_Low[index1].Sort(delegate (TargetPoint A, TargetPoint B) // 놓인순서대로 되있는 lrPoint를 x값을 기준으로 소팅하기
                    {
                        return A.mX.CompareTo(B.mX);
                    });
                    for (int index2 = 0; index2 < lrPoint_Low[index1].Count - 1; ++index2) //해당 라인에서 놓인 돌0번에서 ~ 마지막 전까지
                    {
                        for (int index3 = index2; index3 < lrPoint_Low[index1].Count - 1; ++index3)//ㅇㅇㅇㅇㅇ5개 놓였다고했을때, 
                        {
                            if ((lrPoint_Low[index1][index3 + 1]).GetX() - (lrPoint_Low[index1][index3]).GetX() == cellWidth) //연속된 돌의차이가 cellwidth면 시리즈카운트를 올린다.
                            {
                                ++seriesCount;
                            }
                            else
                            {
                                seriesCount = 0;
                                continue;
                            }
                            if (seriesCount == 1)
                            {
                                firstDoll = index3;  //첫번째 돌의 index 기억하기.
                            }
                            if (seriesCount == 4)
                            {
                                if (firstDoll == 0)
                                {
                                    if (index3 + 2 > lrPoint_Low[index1].Count - 1)//인덱스 범위를 초과하면 뒷돌이 없다는뜻임.
                                    {
                                        return true;
                                    }
                                    else //그렇지 않으면
                                    {
                                        if (lrPoint_Low[index1][index3 + 2].GetX() - lrPoint_Low[index1][index3 + 1].GetX() == cellWidth) //뒷돌이 더있어 6목이라면
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }
                                        else
                                        {
                                            return true; // Win
                                        }
                                    }
                                }
                                else
                                {
                                    if (index3 + 2 > lrPoint_Low[index1].Count - 1)//인덱스 범위를 초과하면, 뒷돌이 없다는뜻임. 앞돌만 조사
                                    {

                                        //5개돌 앞돌 조사해서 아무것도 없다면 오목!!
                                        if ((lrPoint_Low[index1][firstDoll].GetX()) - (lrPoint_Low[index1][firstDoll - 1].GetX()) != cellWidth)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }
                                    }
                                    else //그렇지 않다면 뒷돌이 있을 가능성이 있음.
                                    {
                                        //5개돌 뒷돌, 앞돌 조사해서 아무것도 없다면 오목!!
                                        if ((lrPoint_Low[index1][index3 + 2].GetX()) - (lrPoint_Low[index1][index3 + 1].GetX()) != cellWidth
                                           && (lrPoint_Low[index1][firstDoll].GetX()) - (lrPoint_Low[index1][firstDoll - 1].GetX()) != cellWidth)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }

                                    }
                                }


                            }
                        }
                        seriesCount = 0;
                        firstDoll = -1;


                    }
                }

                if (rlPoint_High[index1].Count >= 5)
                {
                    rlPoint_High[index1].Sort(delegate (TargetPoint A, TargetPoint B) // 놓인순서대로 되있는 lrPoint를 x값을 기준으로 소팅하기
                    {
                        return A.mX.CompareTo(B.mX);
                    });
                    for (int index2 = 0; index2 < rlPoint_High[index1].Count - 1; ++index2) //해당 라인에서 놓인 돌0번에서 ~ 마지막 전까지
                    {
                        for (int index3 = index2; index3 < rlPoint_High[index1].Count - 1; ++index3)//ㅇㅇㅇㅇㅇ5개 놓였다고했을때, 
                        {
                            if ((rlPoint_High[index1][index3 + 1]).GetX() - (rlPoint_High[index1][index3]).GetX() == cellWidth) //연속된 돌의차이가 cellwidth면 시리즈카운트를 올린다.
                            {
                                ++seriesCount;
                            }
                            else
                            {
                                seriesCount = 0;
                                continue;
                            }
                            if (seriesCount == 1)
                            {
                                firstDoll = index3;  //첫번째 돌의 index 기억하기.
                            }
                            if (seriesCount == 4)
                            {
                                if (firstDoll == 0)
                                {
                                    if (index3 + 2 > rlPoint_High[index1].Count - 1)//인덱스 범위를 초과하면 뒷돌이 없다는뜻임.
                                    {
                                        return true;
                                    }
                                    else //그렇지 않으면
                                    {
                                        if (rlPoint_High[index1][index3 + 2].GetX() - rlPoint_High[index1][index3 + 1].GetX() == cellWidth) //뒷돌이 더있어 6목이라면
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }
                                        else
                                        {
                                            return true; // Win
                                        }
                                    }
                                }
                                else
                                {
                                    if (index3 + 2 > rlPoint_High[index1].Count - 1)//인덱스 범위를 초과하면, 뒷돌이 없다는뜻임. 앞돌만 조사
                                    {

                                        //5개돌 앞돌 조사해서 아무것도 없다면 오목!!
                                        if ((rlPoint_High[index1][firstDoll].GetX()) - (rlPoint_High[index1][firstDoll - 1].GetX()) != cellWidth)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }
                                    }
                                    else //그렇지 않다면 뒷돌이 있을 가능성이 있음.
                                    {
                                        //5개돌 뒷돌, 앞돌 조사해서 아무것도 없다면 오목!!
                                        if ((rlPoint_High[index1][index3 + 2].GetX()) - (rlPoint_High[index1][index3 + 1].GetX()) != cellWidth
                                           && (rlPoint_High[index1][firstDoll].GetX()) - (rlPoint_High[index1][firstDoll - 1].GetX()) != cellWidth)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }

                                    }
                                }


                            }
                        }
                        seriesCount = 0;
                        firstDoll = -1;


                    }
                }

                if (rlPoint_Low[index1].Count >= 5)
                {
                    rlPoint_Low[index1].Sort(delegate (TargetPoint A, TargetPoint B) // 놓인순서대로 되있는 lrPoint를 x값을 기준으로 소팅하기
                    {
                        return A.mX.CompareTo(B.mX);
                    });
                    for (int index2 = 0; index2 < rlPoint_Low[index1].Count - 1; ++index2) //해당 라인에서 놓인 돌0번에서 ~ 마지막 전까지
                    {
                        for (int index3 = index2; index3 < rlPoint_Low[index1].Count - 1; ++index3)//ㅇㅇㅇㅇㅇ5개 놓였다고했을때, 
                        {
                            if ((rlPoint_Low[index1][index3 + 1]).GetX() - (rlPoint_Low[index1][index3]).GetX() == cellWidth) //연속된 돌의차이가 cellwidth면 시리즈카운트를 올린다.
                            {
                                ++seriesCount;
                            }
                            else
                            {
                                seriesCount = 0;
                                continue;
                            }
                            if (seriesCount == 1)
                            {
                                firstDoll = index3;  //첫번째 돌의 index 기억하기.
                            }
                            if (seriesCount == 4)
                            {
                                if (firstDoll == 0)
                                {
                                    if (index3 + 2 > rlPoint_Low[index1].Count - 1)//인덱스 범위를 초과하면 뒷돌이 없다는뜻임.
                                    {
                                        return true;
                                    }
                                    else //그렇지 않으면
                                    {
                                        if (rlPoint_Low[index1][index3 + 2].GetX() - rlPoint_Low[index1][index3 + 1].GetX() == cellWidth) //뒷돌이 더있어 6목이라면
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }
                                        else
                                        {
                                            return true; // Win
                                        }
                                    }
                                }
                                else
                                {
                                    if (index3 + 2 > rlPoint_Low[index1].Count - 1)//인덱스 범위를 초과하면, 뒷돌이 없다는뜻임. 앞돌만 조사
                                    {

                                        //5개돌 앞돌 조사해서 아무것도 없다면 오목!!
                                        if ((rlPoint_Low[index1][firstDoll].GetX()) - (rlPoint_Low[index1][firstDoll - 1].GetX()) != cellWidth)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }
                                    }
                                    else //그렇지 않다면 뒷돌이 있을 가능성이 있음.
                                    {
                                        //5개돌 뒷돌, 앞돌 조사해서 아무것도 없다면 오목!!
                                        if ((rlPoint_Low[index1][index3 + 2].GetX()) - (rlPoint_Low[index1][index3 + 1].GetX()) != cellWidth
                                           && (rlPoint_Low[index1][firstDoll].GetX()) - (rlPoint_Low[index1][firstDoll - 1].GetX()) != cellWidth)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            seriesCount = 0;
                                            firstDoll = -1;
                                            continue;
                                        }

                                    }
                                }


                            }
                        }
                        seriesCount = 0;
                        firstDoll = -1;


                    }
                }
            }

            return false;

        }

        private bool SeriesCheck(string line, int index, int count)
        {
            //bool isSeries = false;
            TargetPoint[] targetArray = new TargetPoint[count];


            switch (line)
            {
                case "Row":
                    for (int i = 0; i <= pictureBox1.Width; i += (pictureBox1.Width / LineCount))
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

            for (int i = 0; i <= pictureBox1.Width; i += (pictureBox1.Width / LineCount))
            {
                gp.DrawLine(pen, 0, i, pictureBox1.Width, i); // 가로줄 긋기
                gp.DrawLine(pen, i, 0, i, pictureBox1.Width); // 세로줄 긋기
            }

        }

        private void PlayForm_FormClosed(object sender, FormClosedEventArgs e)
        {

            hClntSock.Close();
            

        }

        private void Other_Draw()
        {
            

        }

        private void OtherTurn(object sender, DoWorkEventArgs e)
        {

            while(true)
            {
                if (bTurn == true)
                {
                    continue;
                }


                //if (stone == STONE.BLACK)
                //{
                //    MessageBox.Show("Im black :Prev Receive");
                //    System.Threading.Thread.Sleep(5000);
                //}
                //else if (stone == STONE.WHITE)
                //{
                //    MessageBox.Show("Im White :Prev Receive");
                //    System.Threading.Thread.Sleep(5000);
                //}

                hClntSock.Receive(buf);

                if (stone == STONE.BLACK)
                {
                    MessageBox.Show("Im black :After Receive");
                    System.Threading.Thread.Sleep(5000);
                }
                else if (stone == STONE.WHITE)
                {
                    MessageBox.Show("Im White :After Receive");
                    System.Threading.Thread.Sleep(5000);
                }



                if (stone == STONE.BLACK)
                {
                    MessageBox.Show("Im black:"+buf[1].ToString()+","+buf[5].ToString());
                    System.Threading.Thread.Sleep(5000);
                }
                else if (stone == STONE.WHITE)
                {
                    MessageBox.Show("Im White :" + buf[1].ToString() + "," + buf[5].ToString());
                    System.Threading.Thread.Sleep(5000);
                }
                SolidBrush brush;
                Graphics gp = this.pictureBox1.CreateGraphics();

                if ((char)buf[0] == ' ')
                {
                    continue;
                }
                //string[] recvData = recvStr.Split(',');
                if ((char)buf[0] == 'W')
                {
                    if (stone == STONE.BLACK)
                    {
                        brush = new SolidBrush(Color.White);
                        int targetX = (int)buf[1];
                        int targetY = (int)buf[5];

                        WhiteReposit.Add(new TargetPoint(targetX, targetY));
                        MessageBox.Show(" White Pos" +"X:"+targetX.ToString() + "Y:" + targetY.ToString());
                        gp.FillEllipse(brush, targetX - (Radian / 2), targetY - (Radian / 2), Radian, Radian);
                        bTurn = true;

                    }
                    else
                    {
                        continue;
                    }

                }
                else if ((char)buf[0] == 'B')
                {
                    if (stone == STONE.BLACK)
                    {

                        continue;

                    }
                    else
                    {
                        brush = new SolidBrush(Color.Black);
                        int targetX = (int)buf[1];
                        int targetY = (int)buf[5];

                        BlackReposit.Add(new TargetPoint(targetX, targetY));
                        MessageBox.Show(" Black Pos" + "X:" + targetX.ToString() + "Y:" + targetY.ToString());
                        gp.FillEllipse(brush, targetX - (Radian / 2), targetY - (Radian / 2), Radian, Radian);
                        bTurn = true;

                    }
                }
                
            }

        }
    }
}
