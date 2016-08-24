#include <stdio.h>
#include <thread>
#include <errno.h>
#include "basecore.h"
#include "socketserver.h"

SocketServer::SocketServer()
{
	m_connectcmd = 0;
	m_closecmd = 0;
}

SocketServer::~SocketServer()
{
}

void SocketServer::Run()
{
	std::thread socket_thread(&SocketServer::execute, this);
	socket_thread.detach();
}

void SocketServer::execute()
{
	printf("socket server thread start.\n");

	ConnInfo conn;
	conn.m_type = 1;
	if (m_socket.Connect())
	{
		conn.m_error = 0;
		fprintf(stdout, "connect server: success.");
	}
	else
	{
		conn.m_error = -1;
		fprintf(stderr, "connect server: failed.");
	}
	OnConnectCallback(conn);

	while (true)
	{
		if (m_closecmd == 1)
		{
			m_socket.Close();
			m_closecmd = 0;
		}

		if (m_connectcmd == 1)
		{
			m_socket.Close();

			ConnInfo conn;
			conn.m_type = 1;
			if (m_socket.Connect())
			{
				conn.m_error = 0;
				fprintf(stdout, "connect server: success.");
			}
			else
			{
				conn.m_error = -1;
				fprintf(stderr, "connect server: failed.");
			}
			OnConnectCallback(conn);
			m_connectcmd = 0;
		}

		//if (m_socket.IsConnected())
		//{
		//fprintf(stdout, "recv begin call.");
		RecvCacheMsg();
		//fprintf(stdout, "recv end call.");
		SendCacheMsg();
		//}
		//else
		//{

		//}

#ifdef WIN32
		Sleep(100);
#else
		usleep(100);
#endif
		//char buff[1024] = {0};
		//Recv(buff, 0);
	}
	printf("socket server thread exit.\n");
	return;
}

void SocketServer::Dump()
{
	printf("socket server read.\n");
	//m_ringbuffer.Read();
	return;
}

bool SocketServer::Create(unsigned short port)
{
	if (!InitSocketSystem())
	{
		fprintf(stderr, "windoes init sokcet system error.");
		return false;
	}
	if (m_socket.Create())
	{
		if (m_socket.Bind(port))
		{
			printf("server start success!\n");
			return true;
		}
	}

	return false;
}

bool SocketServer::Connect(const char* host, unsigned short port)
{
	if (!InitSocketSystem())
	{
		fprintf(stderr, "windoes init sokcet system error.");
		return false;
	}
	if (!m_socket.Create())
	{
		fprintf(stderr, "create socket descriptor error.");
		return false;
	}
	if (m_socket.Connect(host, port))
	{
		printf("connect server success!");
		return true;
	}
	else
	{
		fprintf(stderr, "connect server failed!");
		return false;
	}
}

void SocketServer::Tick(long int interval)
{
	return;
}

void SocketServer::Close()
{
	m_socket.Close();
	return;
}

bool SocketServer::Accept()
{
	return m_socket.Accept(m_client);
}

bool SocketServer::Initialize(const std::string& host, unsigned short port)
{
	//return m_socket.Create();
	m_socket.Initialize(host, port);
	return true;
}

void SocketServer::RecvCacheMsg()
{
	long int head = 2;
	long int sz = 0;
	long int rvsz = 0;
	char buff[1024*16] = {0};
	while (true)
	{
		rvsz = m_socket.Recv(buff + sz, 1024 * 16 - sz);
		if (rvsz < 0)
		{
			rvsz = 0;
			sz = 0;
			int error = errno;
			//CCLOG("the errono is: %s", strerror(error));
			break;
		}
		else if (rvsz == 0)
		{
			rvsz = 0;
			sz = 0;
			int error = errno;
			//CCLOG("the errono is: %s", strerror(error));
			break;
		}

		sz += rvsz;
		if (sz >= head)
		{
			long int pksz = PKGSIZE(buff);
			if ((pksz + head) <= sz)
			{
				if (m_recvbuffer.Write(buff, sz) == 0)
				{
					fprintf(stderr, "recv buffer is full. please handle.");
				}
				rvsz = 0;
				sz = 0;
				break;
			}
		}
	}
}

void SocketServer::SendCacheMsg()
{
	if (m_sendbuffer.Empty())
	{
		return;
	}

	long int head = 2;
	long int hdsz = 0;
	char buff[1024] = {0};
	hdsz = m_sendbuffer.Read(buff, head);			// package size
	if (hdsz != head)
	{
		fprintf(stderr, "package head is incomplete, waiting.");
		return;
	}
	unsigned int pksz = PKGSIZE(buff);
	long int sz = 0;
	sz = m_sendbuffer.Read(buff+head, pksz);
	if (sz != pksz)
	{
		fprintf(stderr, "package head is incomplete, waiting.");
		return;
	}
	else
	{
		int res = m_socket.Send(buff, pksz+2);
		if (res < 0)
		{
			int i = 0;
			do
			{
				m_socket.Close();
				if (m_socket.ReConnect())
				{
					m_socket.Send(buff, pksz+2);
					break;
				}
				else
				{
					i++;
				}
			} while (i < 3);
		}
		return;
	}
}

void SocketServer::HandleMsg()
{
	long int head = 2;			// package head size.
	long int rvcur = 0;
	long int rvtol = 0;
	char rvbuff[1024] = {0};

	while (true)
	{
		if (m_sendbuffer.Available())
		{
			long int hdsz = 0;
			char buff[1024] = {0};
			hdsz = m_sendbuffer.Read(buff, head);			// package size
			if (hdsz != head)
			{
				fprintf(stderr, "package head is incomplete, waiting.");
				continue;
			}
			unsigned int pksz = PKGSIZE(buff);
			long int sz = 0;
			sz = m_sendbuffer.Read(buff+head, pksz);
			if (sz != pksz)
			{
				fprintf(stderr, "package head is incomplete, waiting.");
				continue;
			}
			else
			{
				m_socket.Send(buff, pksz);
				continue;
			}
		}

		// optimization later
		if (rvtol >= head)
		{
			long int pksz = PKGSIZE(rvbuff);
			rvcur = m_socket.Recv(rvbuff+rvtol, pksz-head);
			rvtol += rvcur;
			if (pksz == rvtol)
			{
				if (m_recvbuffer.Write(rvbuff, rvtol) == 0)
				{
					fprintf(stderr, "recv buffer is full. please handle.");
				}
				rvcur = 0;
				rvtol = 0;
			}
		}
		else
		{
			rvcur = m_socket.Recv(rvbuff+rvtol, head);
			rvtol += rvcur;
		}
	}

	return;
}

long SocketServer::Send(const char* buffer, long int sz)
{
	return m_socket.Send(buffer, sz);
}

long SocketServer::Recv(char* buffer, long int sz)
{
	long int head = 2;
	long int rvsz = 0;
	long int rvto = 0;
	//char buff[1024] = {0};
	while (true)
	{
		rvsz = m_socket.Recv(buffer+rvto, 1024-rvto);
		if (rvsz == 0)
		{
			// close 
			return rvsz;
		}
		rvto += rvsz;
		if (rvsz >= head)
		{
			long int pksz = PKGSIZE(buffer);
			if ((pksz + head) == rvsz)
			{
				return pksz;
			}
		}
	}

	return rvto;
}

void SocketServer::SetEventCallback(std::function<int(unsigned int, const char*)>& func)
{
	m_eventcallback = func;
}

int SocketServer::OnEventCallback(unsigned int sz, const char* buffer)
{
	if (m_eventcallback)
	{
		return m_eventcallback(sz, buffer);
	}

	return 0;
}

void SocketServer::SetConnectCallback(std::function<int(ConnInfo)>& func)
{
	m_connectcallback = func;
}

int SocketServer::OnConnectCallback(ConnInfo conninfo)
{
	if (m_connectcallback)
	{
		return m_connectcallback(conninfo);
	}

	return 0;
}