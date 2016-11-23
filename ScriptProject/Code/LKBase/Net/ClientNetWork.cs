//======================================================
//  NetWork Stream Control Layer
//  2011.12.13 created by Wangnannan
//======================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
using UniLua;
using System.Runtime.InteropServices;


namespace GEM_NET_LIB
{
    public interface INetMessageReader
    {
        void DidReadData(byte[] data, int size);

        void Reset();
    }

    public interface INetMessageWriter
    {
        byte[] MakeStream(int msgID, byte[] data);
        byte[] MakeDataStream(int msgID, byte[] data);
        void Reset();
    }

    public class MemoryStreamEx : MemoryStream
    {
        public void Clear()
        {
            SetLength(0);
        }
    }

    public enum EClientNetWorkState
    {
        E_CNWS_NOT_UNABLE,
        E_CNWS_NORMAL,
        E_CNWS_ON_CONNECTED_FAILED,
        E_CNWS_ON_DISCONNECTED,
    }

    public enum EClientConnectState
    { 
        E_NOT_CONNECT,
        E_CONNECT,
        E_NOTICE_CONNECT,
    }

    public delegate void dNetWorkStateCallBack(EClientNetWorkState a_eState, string ip, ushort port);

    public class CClientNetworkCtrl
    {
        [DllImport("__Internal")]
        private static extern string getIPv6(string host,string port);
        private Socket m_ClientSocket = null;
        private SocketAsyncEventArgs mAsyncArgs = null;
        private SocketAsyncEventArgs mReceiveArgs = null;

        private string m_strRomoteIP = "127.0.0.1";
        private ushort m_uRemotePort = 0;

        private INetMessageReader m_Reader = null;
        private INetMessageWriter m_Writer = null;
        public INetMessageReader MsgReader { set { m_Reader = value; } }
        public INetMessageWriter MsgWriter { set { m_Writer = value; } }

        private MemoryStreamEx m_ComunicationMem = new MemoryStreamEx();
        private object m_eNetWorkState = EClientNetWorkState.E_CNWS_NOT_UNABLE;
        private readonly int CONNECT_TIME_OUT = 10;
        private float connect_timeout = 0;
        private EClientConnectState connectState = EClientConnectState.E_NOT_CONNECT;

        private LuaState lua_;
        private int net_state_func_handle_ref_;

        private object sendLockObj = new object();
        private bool isSending = false;
        private List<byte> mByteToSend = new List<byte>();

        public CClientNetworkCtrl()
        {
            lua_ = LuaInstance.instance.Get();
            lua_.GetGlobal("Client");
            lua_.GetField(-1, "ListenNetwork");
            net_state_func_handle_ref_ = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
            lua_.Pop(1);
        }

        public bool IsConnected()
        {
            return m_ClientSocket != null ? m_ClientSocket.Connected : false;
        }

        
        public bool Connect(string a_strRomoteIP, ushort a_uPort)
        {
            AddressFamily ipType = AddressFamily.InterNetwork;
            string ipStr = GetIPv6(a_strRomoteIP, a_uPort.ToString());
            if (!string.IsNullOrEmpty(ipStr))
            {
                string[] ipAndType = ipStr.Split(new char[] {'='});
                if (ipAndType != null && ipAndType.Length == 2)
                {
                    string ipTypeStr = ipAndType[1];
                    if (ipTypeStr.Equals("ipv6"))
                    {
                        a_strRomoteIP = ipAndType[0];
                        ipType = AddressFamily.InterNetworkV6; 
                    }
                }
            }
            if (m_ClientSocket == null)
            {
                try
                {
                    m_ClientSocket = new Socket(ipType, SocketType.Stream, ProtocolType.Tcp);
                }
                catch (Exception e)
                {
                    MonoBehaviour.print(e);
                    return false;
                }
                m_strRomoteIP = a_strRomoteIP;
                m_uRemotePort = a_uPort;
                m_eNetWorkState = EClientNetWorkState.E_CNWS_NORMAL;
                connect_timeout = 0;

                //mByteToSend.Clear();
                isSending = false;

                IPAddress ip = IPAddress.Parse(a_strRomoteIP);
                mAsyncArgs = new SocketAsyncEventArgs();
                mAsyncArgs.RemoteEndPoint = new IPEndPoint(ip, a_uPort);
                mAsyncArgs.UserToken = m_ClientSocket;
                mAsyncArgs.Completed += SocketEventArg_Completed;
                m_ClientSocket.ConnectAsync(mAsyncArgs);
                connectState = EClientConnectState.E_NOT_CONNECT;
                return true;
            }
            return false;
        }

        void SocketEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    DisConnect();
                    break;
            }
        }

        void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                m_eNetWorkState = EClientNetWorkState.E_CNWS_NORMAL;

                mReceiveArgs = new SocketAsyncEventArgs();
                mReceiveArgs.SetBuffer(new byte[4096], 0, 4096);
                mReceiveArgs.Completed += SocketEventArg_Completed;

                /*if (m_ClientSocket != null)
                {
                    m_ClientSocket.ReceiveTimeout = PluginTool.SharedInstance().ReceiveTimeout;
                    m_ClientSocket.SendTimeout = PluginTool.SharedInstance().SendTimeout;
                }*/

                connectState = EClientConnectState.E_CONNECT;
                connect_timeout = 0;
                Receive();
            }
            else
            {
                DidConnectError();
            }
        }

        void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                lock(sendLockObj)
                {
                    isSending = false;
                }
            }
            else
            {
                DidDisconnect();
            }
        }
        void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    lock (m_ComunicationMem)
                    {
                        m_ComunicationMem.Write(e.Buffer, 0, e.BytesTransferred);
                    }
                    Receive();
                }
                else
                {
                    DidDisconnect();
                }
            }
            else
            {
                DidDisconnect();
            }
        }


        public void DisConnect()
        {
            ReleaseSocket();
        }

        private bool ReConnect()
        {
            if (m_strRomoteIP != null)
            {
                ReleaseSocket();
                return Connect(m_strRomoteIP, m_uRemotePort);
            }
            return false;
        }

        public bool SendMessage(int msgID, byte[] data)
        {
            if (m_Writer != null)
            {
                byte[] stream = m_Writer.MakeStream(msgID, data);
                mByteToSend.AddRange(stream);
                return true;
            }
            return false;
        }

        // type + dataType + data
        public bool SendMessage(int msgID, int dataType, byte[] data)
        {
            if (m_Writer != null)
            {
                byte[] newData = m_Writer.MakeDataStream(dataType, data);
                byte[] stream = m_Writer.MakeStream(msgID, newData);
                mByteToSend.AddRange(stream);
                return true;
            }
            return false;
        }

        public void ReleaseSocket()
        {
            if (mReceiveArgs != null) mReceiveArgs.Completed -= SocketEventArg_Completed;
            if (mAsyncArgs != null) mAsyncArgs.Completed -= SocketEventArg_Completed;
            m_eNetWorkState = EClientNetWorkState.E_CNWS_NOT_UNABLE;

            if (m_ClientSocket != null)
            {
                if (m_ClientSocket.Connected == true)
                {
                    try
                    {
                        m_ClientSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception e)
                    {
                        MonoBehaviour.print(e);
                    }
                    finally
                    {
                        m_ClientSocket.Close();
                        m_ClientSocket = null;
                    }
                }
                else
                {
                    m_ClientSocket = null;
                }
            }

            if (m_Reader != null)
                m_Reader.Reset();
            if (m_Writer != null)
                m_Writer.Reset();
        }

        public void Update()
        {
            lock (m_ComunicationMem)
            {
                if (m_ComunicationMem.Length > 0)
                {
                    if (m_Reader != null)
                    {
                        m_Reader.DidReadData(m_ComunicationMem.GetBuffer(), (int)(m_ComunicationMem.Length));
                    }
                    m_ComunicationMem.Clear();
                }
            }
            lock (m_eNetWorkState)
            {
                EClientNetWorkState eState = (EClientNetWorkState)m_eNetWorkState;
                if (eState > EClientNetWorkState.E_CNWS_NORMAL)
                {
                    if (m_ClientSocket != null)
                    {
                        ReleaseSocket();
                        CallBackNetState(eState);
                        eState = EClientNetWorkState.E_CNWS_NOT_UNABLE;
                    }
                }
                else if (connectState == EClientConnectState.E_NOT_CONNECT)
                {
                    connect_timeout += Time.deltaTime;
                    if (connect_timeout > CONNECT_TIME_OUT)
                    {
                        connect_timeout = 0;
                        DidDisconnect();
                    }
                }
                else if (connectState == EClientConnectState.E_CONNECT)
                {
                    // 调用lua接口
                    CallBackNetState(EClientNetWorkState.E_CNWS_NORMAL);
                    connectState = EClientConnectState.E_NOTICE_CONNECT;
                }
            }
        }

        private void CallBackNetState(EClientNetWorkState a_eState)
        {
            try
            {
                lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, net_state_func_handle_ref_);
                lua_.PushInteger((int)a_eState);
                lua_.PushString(m_strRomoteIP);
                lua_.PushInteger(m_uRemotePort);
                lua_.PCall(3, 0, 0);
            }
            catch
            {
                DidDisconnect();
            }
        }

        public void SetSocketSendNoDeley(bool nodelay)
        {
            if (m_ClientSocket != null)
            {
                m_ClientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, nodelay ? 1 : 0);
            }
        }

        private void DidConnectError()
        {
            lock (m_eNetWorkState)
            {
                m_eNetWorkState = EClientNetWorkState.E_CNWS_ON_CONNECTED_FAILED;
            }
        }

        private void DidDisconnect()
        {
            lock (m_eNetWorkState)
            {
                m_eNetWorkState = EClientNetWorkState.E_CNWS_ON_DISCONNECTED;
            }
        }

        private void Receive()
        {
            try
            {
                if (m_ClientSocket.ReceiveAsync(mReceiveArgs))
                {

                }
            }
            catch
            {
                DidDisconnect();
            }
        }

        public bool SendByteThisFrame()
        {
            if (mByteToSend.Count > 0)
            {
                try
                {
                    lock (sendLockObj)
                    {
                        if (!isSending)
                        {
                            mAsyncArgs.SetBuffer(mByteToSend.ToArray(), 0, mByteToSend.Count);
                            mByteToSend.Clear();
                            /*for (int i = 0; i < mAsyncArgs.Buffer.Length; ++i)
                            {
                                Debug.Log("byte " + i + " = " + (int)mAsyncArgs.Buffer[i]);
                            }*/
                            if (m_ClientSocket.SendAsync(mAsyncArgs))
                            {

                            }
                            isSending = true;
                        }
                    }
                    
                    return true;
                }
                catch
                {
                    DidDisconnect();
                }
                return false;
            }
            return true;
        }


        public static IPAddress GetLocalIP()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] ips = Dns.GetHostAddresses(hostName);
            return ips.Length > 0 ? ips[0] : null;
        }

        public static string GetLocalIPString()
        {
            IPAddress ip = GetLocalIP();
            return ip != null ? ip.ToString() : "127.0.0.1";
        }

        public static string GetAdapterMAC()
        {
            IPAddress local = GetLocalIP();
            if (local != null)
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adaper in nics)
                {
                    IPInterfaceProperties ipPro = adaper.GetIPProperties();
                    UnicastIPAddressInformationCollection unicastColl = ipPro.UnicastAddresses;
                    for (int i = 0; i < unicastColl.Count; i++)
                    {
                        if (unicastColl[i].Address.Equals(local))
                        {
                            return adaper.GetPhysicalAddress().ToString();
                        }
                    }
                }
            }
            return "000000000000";
        }

        public static string GetAdapterMacIOS()
        {
            string macAdress = "";
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface adaper in nics)
            {

                PhysicalAddress address = adaper.GetPhysicalAddress();
                if (address.ToString() != "")
                {

                    macAdress = address.ToString();
                    return macAdress;
                }
            }

            return "000000000000";
        }

        public static List<string> GetAllMacAddress()
        {
            List<string> outmac = new List<string>();
            IPAddress local = GetLocalIP();
            if (local != null)
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface adaper in nics)
                {
                    IPInterfaceProperties ipPro = adaper.GetIPProperties();
                    UnicastIPAddressInformationCollection unicastColl = ipPro.UnicastAddresses;
                    for (int i = 0; i < unicastColl.Count; i++)
                    {
                        outmac.Add(adaper.GetPhysicalAddress().ToString());
                    }
                }
            }
            return outmac;
        }

        public void Ping(string server)
        {

        }

        public static string GetIPv6(string host,string port)
        {
#if UNITY_IPHONE
            return getIPv6(host,port);
#else
            return null;
#endif
        }
    }
}
