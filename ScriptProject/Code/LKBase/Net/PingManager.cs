using System;
using UnityEngine;
using UniLua;


public class PingManager : Singleton<PingManager>
{
    public int sTimeOut = 180;

	private float mTimeCount = 0;
	private bool mStartCount = true;
	private float mPingTimgCount = 0;
	private int mLastPingTime = 0;
    private float sendPingInterval = 5f;
    private bool isCallBackLua = false;

    public long sessionId = 0;

	public int netPing
	{
		get
		{
			return Mathf.Max((int)(mPingTimgCount * 1000), mLastPingTime);
		}
	}

	public void UpdatePingServer()
	{
		//LKDebug.Log("PING = " + netPing);
		if (LuaNetwork.Net.IsConnected() == true)
		{
			mTimeCount += Time.deltaTime;
            if (mTimeCount >= sendPingInterval)
			{
				SendPing();
			}

			if (mStartCount == false)
			{
				mPingTimgCount += Time.deltaTime;
				if (mPingTimgCount >= sTimeOut)
				{
                    LuaState lua = LuaInstance.instance.Get();
                    lua.LuaFuncCall(LuaInstance.instance.client_table_ref,"NetDisconnect");
					Debug.LogError("断开服务器连接");
					mPingTimgCount = 0;
				}
			}
		}
	}

    public void SetPingInfo(float pingInterval,bool isCallBack)
    {
        ResetPing();
        sendPingInterval = pingInterval;
        isCallBackLua = isCallBack;
    }

	void SendPing()
	{
		CClientSendMessage.SendPing(netPing);
        mTimeCount = 0;
		mStartCount = false;
        LuaState lua = LuaInstance.instance.Get();
        if (isCallBackLua)
            lua.LuaStaticFuncCall(LuaInstance.instance.client_table_ref, "PingCallBack", netPing);
	}

	public void ResetPing()
	{
		mLastPingTime = (int)(mPingTimgCount * 1000);
		mPingTimgCount = 0f;
		mStartCount = true;

	}


}


