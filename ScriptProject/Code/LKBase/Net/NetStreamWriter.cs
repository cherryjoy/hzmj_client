
/*
 Message Stream Format:Size(2),MsgID(4),NOTUSE(4),SessionId(8),PB(0~)
 */
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Net;
namespace GEM_NET_LIB
{
	public class CNetStreamWriter : INetMessageWriter
	{
		private MemoryStreamEx m_Buffer = new MemoryStreamEx ();
		private byte[] m_NotUseByte = new byte[4]{0,0,0,0};
		private byte[] m_SessionByte = new byte[8]{0, 0, 0, 0, 0, 0, 0, 0};
		private long sessionId = 0;
		for (int i = 0; i < 8; i++)
		{
			m_SessionByte[i] = (byte)(sessionId >> (8 * (7 - i)));
		}
		
		private byte mCounter = 0;
		//-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
		byte[] INetMessageWriter.MakeStream (int msgID, byte[] data)
		{
			m_NotUseByte[0] = mCounter;
			m_Buffer.Clear();
			int net_msgID = IPAddress.HostToNetworkOrder (msgID);
			byte[] net_MsgID_byte = BitConverter.GetBytes(net_msgID);			
			int net_data_size = IPAddress.HostToNetworkOrder (/*sizeof(short) +*/ net_MsgID_byte.Length + m_NotUseByte.Length + (data != null ?data.Length : 0));
			byte[] net_Data_Size_byte = BitConverter.GetBytes(net_data_size);
			m_Buffer.Write(net_Data_Size_byte,0,net_Data_Size_byte.Length);
			m_Buffer.Write(net_MsgID_byte,0,net_MsgID_byte.Length);
			m_Buffer.Write(m_NotUseByte,0,m_NotUseByte.Length);
			m_Buffer.Write(m_SessionByte,0,m_SessionByte.Length);
			mCounter ++;
            if (data != null)
                m_Buffer.Write(data, 0, data.Length);
			return m_Buffer.ToArray();
		}
        byte[] INetMessageWriter.MakeDataStream(int msgID, byte[] data)
        {
            m_Buffer.Clear();
            //int net_msgID = IPAddress.HostToNetworkOrder(msgID);
            byte[] net_MsgID_byte = BitConverter.GetBytes(msgID);
            m_Buffer.Write(net_MsgID_byte, 0, net_MsgID_byte.Length);
            if (data != null)
                m_Buffer.Write(data, 0, data.Length);
            return m_Buffer.ToArray();
        }

		void INetMessageWriter.Reset ()
		{
			m_Buffer.Clear ();
			mCounter = 0;
		}
	}
}
