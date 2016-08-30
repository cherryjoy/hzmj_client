//======================================================
//  NetWork Stream Reader
//  2011.12.14 created by Wangnannan
//======================================================
/*
 Message Stream Format:Size(4),MsgID(4),NOTUSE(4),PB(0~)
 */
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Net;
using UniLua;
using System.Runtime.InteropServices;

namespace GEM_NET_LIB
{
	public class CNetRecvMsg
	{
		public int m_nMsgID = 0;
		public MemoryStream m_DataMsg;
		//-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
		//public T DeSerializeProtocol<T>()
		//{
		//    m_DataMsg.Position = 0;
		//    return ProtoBuf.Serializer.Deserialize<T>(m_DataMsg);
		//}
	}

	public delegate void OnHandleOneMessage(CNetRecvMsg msg);
	public interface ILogicHandleMessage
	{
		void ClientHandleMessage(int msgID, MemoryStream data);
	}

	internal class CNetStreamBuffer
	{
		private ArrayList m_streamList = new ArrayList ();
		private int m_nActivedStreamPosition = 0;
		private const int m_nMaxStreamCount = 2;
		public CNetStreamBuffer ()
		{
			for (int i = 0; i < m_nMaxStreamCount; i++)
				m_streamList.Add (new MemoryStreamEx ());
		}
		public MemoryStreamEx GetActivedStream ()
		{
			return (MemoryStreamEx)m_streamList[m_nActivedStreamPosition];
		}
		public MemoryStreamEx MoveStream (int index)
		{
			MemoryStreamEx oldStream = (MemoryStreamEx)m_streamList[m_nActivedStreamPosition];
			if (index > 0) {
				if (index < oldStream.Length) {
					m_nActivedStreamPosition = (m_nActivedStreamPosition + 1) % m_nMaxStreamCount;
					MemoryStreamEx newStream = (MemoryStreamEx)m_streamList[m_nActivedStreamPosition];
					newStream.Clear();
					newStream.Write (oldStream.GetBuffer (), (int)index, (int)(oldStream.Length - index));
					oldStream.Clear();
					return newStream;
				} else {
					oldStream.Clear();
				}
			}
			return oldStream;
		}
		public void Reset ()
		{
			for (int i = 0; i < m_nMaxStreamCount; i++)
				((MemoryStreamEx)m_streamList[i]).Clear();
		}
	}
	public class CNetStreamReader : INetMessageReader
	{
		private int m_nProgress = 0;
		private short m_nStreamSize = 0;
		private int m_nMsgID = 0;
		private int m_nDataType = 0;
		//Except progress = 0;
		private static readonly int m_nMaxDataSize = 200 * 1024;
		private CNetStreamBuffer m_NetBuffer = new CNetStreamBuffer ();
		private MemoryStreamEx m_MsgDataBody = new MemoryStreamEx ();

		private ILogicHandleMessage m_Logic;
		public ILogicHandleMessage LogicHandleMessage
		{
			set { m_Logic = value; }
		}

        private LuaState lua_;
        private int func_handle_ref_;
        private GCHandle userdata_;

        public CNetStreamReader() {
            lua_ = LuaInstance.instance.Get();
            lua_.GetGlobal("ClientHandleMsg");
            lua_.GetField(-1, "Handle");
            func_handle_ref_ = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
            lua_.Pop(1);
        }

        //-=-=-=-=-=-=-=-=-=Message Stream Format:Size(2),MsgID(4),NOTUSE(4),PB(0~)=-=-=-=-=-=-=-=-=-=-=-=-=
		void INetMessageReader.DidReadData (byte[] data, int size)
		{
			MemoryStream activedStream = m_NetBuffer.GetActivedStream ();
			//Get Current Active Stream Buffer
			activedStream.Write (data, 0, size);
			byte[] nowData = activedStream.GetBuffer ();
            /*for (int i = 0; i < size; ++i)
            {
                Debug.Log("recv byte " + i + " : " + (int)nowData[i]);
            }*/

			long nowStreamLength = activedStream.Length;
            Debug.Log("nowStreamLength: " + nowStreamLength);
			while (true) {
				try {
					if (m_nProgress != 4) 
                    {
						while (m_nProgress < 3) 
                        {
							int tmpvalue = 0;
							if (nowStreamLength < (sizeof(short) + sizeof(int)))
								//throw new Exception ("Data Not Enough");
							{
								if (m_nProgress != 4)
									m_nProgress = 0;
								return;
							}
							
							switch (m_nProgress) 
                            {
                                case 0:
                                    {
                                        m_nStreamSize = BitConverter.ToInt16(nowData, 0);
                                        m_nStreamSize = IPAddress.NetworkToHostOrder(m_nStreamSize);
                                        nowStreamLength -= sizeof(short);
                                        Debug.Log("size: " + m_nStreamSize);
                                        break;
                                    }
                                case 1:
                                    {
                                        tmpvalue = BitConverter.ToInt32(nowData, 2);
                                        tmpvalue = IPAddress.NetworkToHostOrder(tmpvalue);
                                        m_nMsgID = tmpvalue;
                                        nowStreamLength -= sizeof(int);
                                        Debug.Log("m_nMsgID: " + m_nMsgID);
                                        break;
                                    }
                                case 2:
                                    {
                                        tmpvalue = BitConverter.ToInt32(nowData, 6);
                                        tmpvalue = IPAddress.NetworkToHostOrder(tmpvalue);
                                        m_nDataType = tmpvalue;
                                        nowStreamLength -= sizeof(int);
                                        Debug.Log("m_nDataType: " + m_nDataType);
                                        break;
                                    }
							}
							m_nProgress++;
						}
						if (CheckHead ()) {
							m_nProgress = 4;
						} else {
							activedStream.SetLength (0);
							//clearAll
							return;
						}
					}
					int nDataSize = m_nStreamSize - 10;
					//Must >= 0
					bool bReturn = false;
					m_MsgDataBody.Clear();
					//ClearData
					if (nDataSize > 0) 
                    {
                        Debug.Log("nowStreamLength: " + nowStreamLength + ", nDataSize: " + nDataSize);
						if (nowStreamLength >= nDataSize) 
                        {
							m_MsgDataBody.Write (nowData, (int)(activedStream.Length - nowStreamLength), nDataSize);
							nowStreamLength -= nDataSize;
						} 
                        else 
                        {
							bReturn = true;
							//Can't Process any more.
						}
					}
					activedStream = m_NetBuffer.MoveStream ((int)(activedStream.Length - nowStreamLength));
					if (bReturn)
						return;
					else {
						try {
							if (m_nMsgID >= 1 && m_nMsgID < 1024)
							{
								m_Logic.ClientHandleMessage(m_nMsgID, m_MsgDataBody);
							}
							else
							{
                                for (int i = 0; i < m_MsgDataBody.Length; ++i)
                                {
                                    Debug.Log("msgData " + i + " : " + (int)(m_MsgDataBody.GetBuffer()[i]));
                                }

								lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, func_handle_ref_);
								lua_.PushInteger(m_nMsgID);
								userdata_ = lua_.NewUnManageMem(m_MsgDataBody.GetBuffer());
                                lua_.PushNumber((double)m_MsgDataBody.Length);
#if DEBUG
                                if (m_MsgDataBody.Length != nDataSize)
                                {
                                    Debug.LogWarning("msg length is 0" + m_nMsgID);
                                }
#endif
								lua_.PCall(3, 0, 0);
								userdata_.Free();
							}
						} catch (Exception e) {
							Debug.LogError(e + "\nMsg " + m_nMsgID + " has error!");
						}
					}
					nowData = activedStream.GetBuffer ();
					nowStreamLength = activedStream.Length;
					m_nProgress = 0;
				} catch {
					if (m_nProgress != 4) {
						m_nProgress = 0;
					}
					break;
				}
			}
		}
		void INetMessageReader.Reset ()
		{
			m_nProgress = 0;
			m_nStreamSize = 0;
			m_nMsgID = 0;
			m_nDataType = 0;
			m_NetBuffer.Reset ();
			m_MsgDataBody.Clear();
		}
		//-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
		private bool CheckHead ()
		{
			if (m_nStreamSize < 10 || m_nStreamSize > CNetStreamReader.m_nMaxDataSize)
				return false;
			return true;
		}
	}
}
