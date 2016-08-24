#ifdef WIN32
#include <winsock2.h>
#else
#include <netinet/tcp.h>
#include <signal.h>
#endif
#include <errno.h>
#include <stdio.h>
#include <string.h>
#include "socketbase.h"


#define TIME_OUT_TIME 5		// connect out time 5 second

bool InitSocketSystem()
{
#ifdef WIN32
	WSADATA wsadata;
	unsigned short winsock_version=MAKEWORD( 1, 1 );
	if(WSAStartup(winsock_version,&wsadata))
	{
		return false;
	}
#endif
	return true;
}

void clearSocketSystem()
{
#ifdef WIN32
	WSACleanup();
#endif
}

SocketBase::SocketBase()
{
	m_sockfd	= -1;
	m_connected	= false;
	m_hostport	= 0;
}

SocketBase::~SocketBase()
{
	Close();
}

void SocketBase::Close()
{
	if (m_sockfd != -1)
	{
#ifdef WIN32
		shutdown(m_sockfd, SD_BOTH);
		closesocket(m_sockfd);
#else
		shutdown(m_sockfd, SHUT_RDWR);
		close(m_sockfd);
#endif
		m_sockfd = -1;
	}
	m_connected = false;
}

bool SocketBase::Create()
{
	if (!InitSocketSystem())
	{
		fprintf(stderr, "windoes init sokcet system error.");
		return false;
	}
	m_sockfd = socket(AF_INET, SOCK_STREAM, 0);
	if (m_sockfd == -1)
	{
		fprintf(stderr, "create socket error: %s.", strerror(errno));
		return false;
	}
	return true;
}

bool SocketBase::Bind(unsigned short port)
{
	if (m_sockfd == -1)
	{
		return false;
	}
	struct sockaddr_in sin;
	sin.sin_family = AF_INET;
#ifdef WIN32
	sin.sin_addr.S_un.S_addr = 0;
#else
	sin.sin_addr.s_addr = 0;
#endif
	memset(sin.sin_zero, 0, 8);
	sin.sin_port = htons(port);
	if (bind(m_sockfd, (sockaddr* )&sin, sizeof(sockaddr_in)) != 0)
	{
		fprintf(stderr, "bind error: %s.", strerror(errno));
		return false;
	}
	
	if (listen(m_sockfd, 1024) == -1)
	{
		fprintf(stderr, "server listen error.");
		return false;
	}
	m_connected = true;

	return true;
}

bool SocketBase::Accept(SocketBase& client)
{
	if (m_sockfd == -1)
	{
		fprintf(stderr, "socket descriptor is illegal.1");
		return false;
	}
	client.m_sockfd = accept(m_sockfd, NULL, NULL);
	if (client.m_sockfd == -1)
	{
		fprintf(stderr, "socket accept error.");
		return false;
	}
	else
	{
		printf("new connect in.\n");
		client.m_connected = true;
		return true;
	}
}

bool SocketBase::Connect(const std::string& host, unsigned short port)
{
	if (m_sockfd == -1)
	{
		fprintf(stderr, "socket descriptor is illegal.2");
		return false;
	}
	m_hostaddress = host;
	m_hostport = port;
	struct hostent * he = gethostbyname(host.c_str());
	if (he == NULL)
	{
		return false;
	}
	struct sockaddr_in sin;
	sin.sin_family = AF_INET;
	sin.sin_addr = *((struct in_addr* )he->h_addr_list[0]);
	//sin.sin_addr = *((struct in_addr*) he->h_addr);
	memset(sin.sin_zero, 0, 8);
	sin.sin_port = htons(port);

#ifdef WIN32
	int ntimeout = TIME_OUT_TIME * 1000;
	if (setsockopt(m_sockfd, SOL_SOCKET, SO_RCVTIMEO, (char *)&ntimeout, sizeof(int)) == SOCKET_ERROR)
	{
		Close();
		return false;
	}
	if (setsockopt(m_sockfd, SOL_SOCKET, SO_SNDTIMEO, (char *)&ntimeout, sizeof(int)) == SOCKET_ERROR)
	{
		Close();
		return false;
	}
#else

	timeval tv;
	int nlen = sizeof(tv);
	tv.tv_sec = TIME_OUT_TIME;
	tv.tv_usec = 5;
	if (setsockopt(m_sockfd, SOL_SOCKET, SO_RCVTIMEO, (char *)&tv, sizeof(tv)) == -1)
	{
		Close();
		return false;
	}
	if (setsockopt(m_sockfd, SOL_SOCKET, SO_SNDTIMEO, (char *)&tv, sizeof(tv)) == -1)
	{
		Close();
		return false;
	}
#endif
#ifdef __APPLE__
	int set = 1;
	setsockopt(m_sockfd, SOL_SOCKET, SO_NOSIGPIPE, (void*)&set, sizeof(int));
#endif

	if (connect(m_sockfd, (struct sockaddr*)&sin, sizeof(sin)) == -1)
	{
		// connect timeout.
		fprintf(stderr, "connect timeout error: %s, close socket.", strerror(errno));
		Close();
		return false;
	}
	//int flag = 1;
	//setsockopt(m_sockfd, IPPROTO_TCP, TCP_NODELAY, (char *)flag, sizeof(flag));
	unsigned long rb = 1;
#ifdef WIN32
	ioctlsocket(m_sockfd, FIONBIO, &rb);
#else
	ioctl(m_sockfd, FIONBIO, &rb);
#endif
	m_connected = true;
	return true;

	/*
	// set socket non blocking
	unsigned long rb = 1;
#ifdef WIN32
	//ioctlsocket(m_sockfd, FIONBIO, &rb);
#else
	ioctl(m_sockfd, FIONBIO, &rb);
#endif



	if (connect(m_sockfd, (struct sockaddr* )&sin, sizeof(sin)) == -1)
	{
#ifdef WIN32
		// connect timeout.
		fprintf(stderr, "connect timeout error: %s, close socket.", strerror(errno));
		Close();
		return false;
#else
		int error = -1, len;
		len = sizeof(int);
		timeval tm;
		fd_set set;
		bool ret = false;
		tm.tv_sec = TIME_OUT_TIME;
		tm.tv_usec = 0;
		FD_ZERO(&set);
		FD_SET(m_sockfd, &set);
		if (select(m_sockfd + 1, nullptr, &set, nullptr, &tm) > 0)
		{
			getsockopt(m_sockfd, SOL_SOCKET, SO_ERROR, &error, (socklen_t*)len);
			if (error != 0)
			{
				return false;
			}
		}
		else
		{
			return false;
		}
#endif
	}

#ifdef WIN32
	ioctlsocket(m_sockfd, FIONBIO, &rb);
#endif

	int bufsize = MAX_RECV_BUFFERSIZE;
	setsockopt(m_sockfd, SOL_SOCKET, SO_RCVBUF, (char*)&bufsize, sizeof(bufsize));
	setsockopt(m_sockfd, SOL_SOCKET, SO_SNDBUF, (char*)&bufsize, sizeof(bufsize));

	m_connected = true;

	return true;
	*/
}

bool SocketBase::ReConnect()
{
	Close();

	if (Create())
	{
		return Connect(m_hostaddress, m_hostport);
	}
	else
	{
		return false;
	}
}

bool SocketBase::Connect()
{
	if (Create())
	{
		return Connect(m_hostaddress, m_hostport);
	}
	else
	{
		return false;
	}
}

long SocketBase::Send(const char* buff, long len)
{
	if (m_sockfd == -1)
	{
		fprintf(stderr, "socket descriptor is illegal.3");
		return -1;
	}
	int sended = 0;
	do
	{
		int sz = send(m_sockfd, buff+sended, len-sended, 0);
		if (sz < 0)
		{
			// network abnormal.
			return -1;
		}
		sended += sz;
	} while(sended < len);

	return sended;
}

long SocketBase::Recv(char* buff, long len)
{
	if (m_sockfd == -1)
	{
		//fprintf(stderr, "socket descriptor is illegal.4");
		return -1;
	}

	int sz = recv(m_sockfd, buff, len, 0);

	return sz;
}
