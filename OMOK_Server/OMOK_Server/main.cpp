#include <WinSock2.h>
#include <iostream>
#include <stack>
#include <string>

using namespace std;

enum
{
	BUF_SIZE = 1024,
	PORT = 1234,
	SOCKET_QUEUE = 1000 // Client 1000명까지 대기자 수용
};
int main()
{

	
	WSADATA wsaData;
	SOCKET hServSock, hClntSock;
	SOCKADDR_IN servAddr, clntAddr;
	TIMEVAL timeout;
	fd_set read,cpyRead;

	//const char* PORT = "1234";
	char buf[BUF_SIZE];

	int strLen = 0;
	int szAddr=0;
	
	int packetLength = 10;


	stack<const char*> turnStack;
	turnStack.push("W");
	turnStack.push("B");


	if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
	{
		cout << "WSAStartup() Error" << endl;
		return -1;

	}
	hServSock = socket(PF_INET, SOCK_STREAM, 0);

	//if (hServSock == SOCKET_ERROR)
	//{
	//	cout << "Socket Creating Fail" << endl;
	//	return -1;
	//}

	//---아랫부분 다시공부해야함----
	servAddr.sin_family = AF_INET;
	servAddr.sin_addr.s_addr = htonl(INADDR_ANY);
	//servAddr.sin_port = htons(atoi(PORT));
	servAddr.sin_port = htons(PORT);
	///-------------------------

	if (bind(hServSock, (sockaddr*)&servAddr, sizeof(servAddr)) == SOCKET_ERROR)
	{
		cout << "bind() error";
	}

	FD_ZERO(&read);
	FD_SET(hServSock, &read);

	listen(hServSock, SOCKET_QUEUE);

	while (1)
	{

		timeout.tv_sec = 5;
		timeout.tv_usec = 5000;
		cpyRead = read;

		int fdNum = select(0, &cpyRead, 0, 0, &timeout);
		if (fdNum == -1)
		{
			cout << "select Error" << endl;
			break;
		}
		if (fdNum == 0)
		{
			cout << "TimeOut" << endl;
			continue;
		}

		for (unsigned int i = 0; i < cpyRead.fd_count; ++i)
		{
			
			if (FD_ISSET(cpyRead.fd_array[i], &cpyRead))//등록한 소켓중 혹시 수신할만한것이 있니?
			{
				if (cpyRead.fd_array[i] == hServSock)
				{
					szAddr = sizeof(clntAddr);

					hClntSock = accept(hServSock, (sockaddr*)&clntAddr, &szAddr);
					FD_SET(hClntSock, &read);

					cout << "Connected Client " << hClntSock << endl;
					//C: Connected , + Clinet Socket
					buf[0] = 'C';
					buf[1] = (hClntSock & 0x000000ff);
					buf[2] = ((hClntSock & 0x0000ff00) >> 8);
					buf[3] = ((hClntSock & 0x00ff0000) >> 16);
					buf[4] = ((hClntSock & 0xff000000) >> 24);

					send(hClntSock, buf, 5, 0);
					////연결되자마자 흑//백 할당???
					//if (!turnStack.empty())
					//{
					//	//buf = turnStack.top();
					//	sprintf_s(buf, BUF_SIZE -1,turnStack.top());
					//	turnStack.pop();
					//	
					//	send(hClntSock, buf, strlen(buf), 0);

					//}
					

				}
				else
				{
					strLen = recv(cpyRead.fd_array[i], buf, BUF_SIZE, 0);

					if (strLen <0)
					{
						cout << "Client["<<cpyRead.fd_array[i]<<"]"<< "disconnected" << endl;
						FD_CLR(cpyRead.fd_array[i], &read);
						closesocket(cpyRead.fd_array[i]);
						//break;
					}
					else
					{ 
							
						for (int i = 0; i < read.fd_count; ++i)
						{
							
							if (read.fd_array[i] != hServSock)
							{
								if (buf[0] == 'M')
								{
									char tempBuf[BUF_SIZE];
									int bufIDX = 0;
									//Packet 0~4 
									for (bufIDX = 0; bufIDX<=4 ; ++bufIDX)
									{
										tempBuf[bufIDX] = buf[bufIDX];
									}
									// Packet 5~~~널문자까지
									for (bufIDX; buf[bufIDX] != '\0'; ++bufIDX)
									{
										tempBuf[bufIDX] = buf[bufIDX];
									}
									tempBuf[bufIDX] = '\0';

									send(read.fd_array[i], tempBuf, bufIDX +1, 0);
									cout << "Send to Client[Message] : " << read.fd_array[i] << endl;
									
								}
								else if(buf[1]=='P')
								{
									char* tempBuf = new char[packetLength];
									for (int i = 0; i < packetLength; ++i)
									{
										tempBuf[i] = buf[i];
									}
									send(read.fd_array[i], tempBuf, packetLength, 0);
									cout << "Send to Client[Position] : " << read.fd_array[i] << endl;
									delete[]tempBuf;
								}
								

								
							}
						}
								
							
					}

				}
			}
		}

		

	}
	closesocket(hServSock);
	WSACleanup();



	return 0;

}