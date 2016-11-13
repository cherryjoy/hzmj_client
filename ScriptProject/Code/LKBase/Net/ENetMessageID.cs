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
	S2C_PING_TIME = 1000,

	MAX = 1001,
}

