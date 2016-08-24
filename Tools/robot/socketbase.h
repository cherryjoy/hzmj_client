#ifndef _SOCKET_BASE_H
#define _SOCKET_BASE_H

#define MAX_RECV_BUFFERSIZE 65535

#ifdef WIN32
#include <winsock2.h>
#include <windows.h>
#pragma comment(lib, "ws2_32.lib")
#define sock_fd SOCKET
#else
#include <unistd.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <sys/select.h>
#include <sys/time.h>
#include <sys/types.h>
#include <unistd.h>
#include <netdb.h>
#include <sys/ioctl.h>
#define sock_fd int
#endif

#include <errno.h>
#include <string>

bool InitSocketSystem();
void ClearSocketSystem();

class SocketBase
{
public:
	SocketBase();
	virtual ~SocketBase();

public:
	bool	Create();
	virtual bool Connect(const std::string& host, unsigned short port);
	virtual bool IsConnected() { return m_connected; }
	virtual bool Bind(unsigned short port);
	virtual bool Accept(SocketBase& client);
	virtual void Close();
	virtual long Send(const char* buff, long len);
	virtual long Recv(char* buff, long len);

	virtual bool ReConnect();
	virtual bool Connect();

	sock_fd GetHandle() { return m_sockfd; }

	int GetSocketState() { return m_socketstate; }

	void Initialize(const std::string& host, unsigned short port) { m_hostaddress = host; m_hostport = port; }
private:
	sock_fd			m_sockfd;
	bool			m_connected;
	std::string		m_hostaddress;
	unsigned short	m_hostport;
	int				m_socketstate;
};

#endif