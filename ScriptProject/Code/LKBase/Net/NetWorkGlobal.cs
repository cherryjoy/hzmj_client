//======================================================
//  NetWork Conmunicate Interface
//  2011.12.13 created by Wangnannan
//======================================================
using GEM_NET_LIB;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
public class CNetWorkGlobal
{
	private CClientNetworkCtrl m_Ctrl;

	public CNetWorkGlobal ()
	{
		CNetStreamReader reader = new CNetStreamReader();
		reader.LogicHandleMessage = new CClientHandleMessage();
		CNetStreamWriter writer = new CNetStreamWriter();

		m_Ctrl = new CClientNetworkCtrl();
		m_Ctrl.MsgReader = reader;
		m_Ctrl.MsgWriter = writer;
	}

	public bool IsConnected ()
	{
		return m_Ctrl.IsConnected ();
	}
	public bool Connect (string a_strRomoteIP, ushort a_uPort)
	{
		return m_Ctrl.Connect (a_strRomoteIP, a_uPort);
	}
	public void DisConnect()
	{
		m_Ctrl.DisConnect();
	}

	public bool SendNetMessage (int msgID,byte[]  data)
	{
		if (IsConnected() == true)
		{
            return m_Ctrl.SendMessage(msgID, data);
		}
		return false;
	}
    public bool SendCastNetMessage(int msgID, int dataTypeId, byte[] bytes)
    {
		if (IsConnected() == true)
		{
	        return m_Ctrl.SendMessage(msgID, dataTypeId, bytes);
		}
        return false;
    }
	public bool SendNetEmptyMessage (int msgID)
	{
		if (IsConnected() == true)
		{
			return m_Ctrl.SendMessage (msgID, null);
		}
		return false;
	}

    public bool SendByteThisFrame() { 
        if (IsConnected() == true)
		{
			return m_Ctrl.SendByteThisFrame ();
		}
		return false;
    }

	public void Update ()
	{
		m_Ctrl.Update ();
	}
	//public static string GetAdapterMAC()
	//{
	//    return CClientNetworkCtrl.GetAdapterMAC(); 
	//}
	//public static string GetAdapterMACIOS()
	//{
	//    return CClientNetworkCtrl.GetAdapterMacIOS();
	//}
	//public static List<string> GetAllMacAddress()
	//{
	//    return CClientNetworkCtrl.GetAllMacAddress();
	//}
	public void SetSocketSendNoDeley (bool nodelay)
	{
		m_Ctrl.SetSocketSendNoDeley(nodelay);
	}
}
