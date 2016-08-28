using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum ENetMsgID_C2S
{
	//Common msg
    C2S_PING_TIME = 1,
}

public enum ENetMsgID_S2C
{
	//server 1 - 20
#if OPEN_BROAD_PING
	S2C_BROAD_PING_TIME1,	//房间内广播用，用来测试战斗的网络环境
	S2C_BROAD_PING_TIME2,
#endif
	S2C_PING_TIME = 1000,

	MAX = 1001,
}

