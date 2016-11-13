using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using GEM_NET_LIB;


public class CClientHandleMessage : ILogicHandleMessage
{
    private OnHandleOneMessage[] m_HandleMap;

    void ILogicHandleMessage.ClientHandleMessage(int msgID, MemoryStream data)
    {
        CNetRecvMsg clientNetMsg = new CNetRecvMsg();

        clientNetMsg.m_nMsgID = msgID;
        clientNetMsg.m_DataMsg = data;

        ClientHandleMessage(clientNetMsg);
    }

    void ClientHandleMessage(CNetRecvMsg msg)
    {
        if (msg.m_nMsgID >= 0 && msg.m_nMsgID < m_HandleMap.Length)
        {
            OnHandleOneMessage handler = m_HandleMap[msg.m_nMsgID];
            if (handler != null)
            {
                handler(msg);
            }
            else
            {
                LKDebug.LogError("This Message could not be processed :" + msg.m_nMsgID);
            }
        }
    }

    public CClientHandleMessage()
    {
        m_HandleMap = new OnHandleOneMessage[(int)ENetMsgID_S2C.MAX];

        m_HandleMap[(int)ENetMsgID_S2C.S2C_PING_TIME] = this.S2C_PING_TIME_HANDLER;
    }

    private void S2C_PING_TIME_HANDLER(CNetRecvMsg msg)
    {
        PingManager.Instance.ResetPing();
    }
}