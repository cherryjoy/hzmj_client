#ifndef _SOCKET_SERVER_H
#define _SOCKET_SERVER_H
#include <functional>
#include "socketbase.h"
#include "ringbuffer.h"
#include "basecore.h"

class SocketServer
{
public:
	SocketServer();
	~SocketServer();

	bool Connect(const char* host, unsigned short port);
	void Close();
	bool Create(unsigned short port);
	bool Initialize(const std::string& host, unsigned short port);
	void Tick(long int interval);
	bool Accept();
	void Run();			// start network thread.
	void Dump();
	bool PrepareRun(int flag);	// flag: 0, server; 1 client

	bool GetSocketState() { return m_socket.IsConnected(); }

	void RecvCacheMsg();
	void SendCacheMsg();

	long Recv(char* buffer, long int sz);
	long Send(const char* buffer, long int sz);

	void HandleMsg();

	RingBuffer& GetSendBuffer() { return m_sendbuffer; }
	RingBuffer& GetRecvBuffer() { return m_recvbuffer; }

	// function callback
	std::function<int(unsigned int, const char*)> m_eventcallback;
	void SetEventCallback(std::function<int(unsigned int, const char*)>& func);
	int OnEventCallback(unsigned int sz, const char* buffer);

	// asyn connect callback
	std::function<int(ConnInfo)> m_connectcallback;
	void SetConnectCallback(std::function<int(ConnInfo)>& func);
	int OnConnectCallback(ConnInfo conninfo);

	// socket server command
	int GetSocketCmd() { return m_connectcmd; }
	void SetSocketCmd(int cmd) { m_connectcmd = cmd; }

	int GetCloseCmd() { return m_closecmd; }
	void SetCloseCmd(int cmd) { m_closecmd = cmd; }

private:
	void execute();

private:
	// socket 
	SocketBase m_socket;
	SocketBase m_client;

	// test ringbuffer.
	RingBuffer m_ringbuffer;

	// send and recv buffer. 
	RingBuffer m_sendbuffer;
	RingBuffer m_recvbuffer;

	// cmd 
	int m_connectcmd;
	int m_closecmd;
};

#endif;