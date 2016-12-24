using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

class LKHttpRequest2String : LKHttpRequest
{
   
    private string str;
    public override void ReadFromStream()
    {
        try
        {
            Stream stream = httpResponse.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            str = sr.ReadToEnd();
            stream.Close();
        }
        catch (Exception e)
        {
            errorMsg = e.Message;
            lock (httpRequestState)
            {
                httpRequestState = HTTPEQUESTSTATE.HTTPEQUESTERROR;
            }
            return;
        }
        lock (httpRequestState)
        {
            httpRequestState = HTTPEQUESTSTATE.HTTPEQUESTEND;
        }
    }
    public override void Update()
    {
        switch ((HTTPEQUESTSTATE)httpRequestState)
        {
            case HTTPEQUESTSTATE.HTTPEQUESTEND:
                //UnityEngine.Debug.Log("str  " + str);
                downLoadEndCallback(str);    
            //CallLuaFun(downLoadEndCallback, str);
                lock (httpRequestState)
                {
                    httpRequestState = LKHttpRequest.HTTPEQUESTSTATE.HTTPEQUESTNONE;
                }
                Close();
                break;
            case HTTPEQUESTSTATE.HTTPEQUESTERROR:
                downLoadErrorCallback(errorMsg);
                //CallLuaFun(downLoadErrorCallback, "");
                lock (httpRequestState)
                {
                    httpRequestState = LKHttpRequest.HTTPEQUESTSTATE.HTTPEQUESTNONE;
                }
                Close();
                break;
        }
    }
    public LKHttpRequest2String( Action<string> downloadEnd, Action<string> downloadError)
        : base( null, null, downloadEnd, downloadError)
    {
    }
}
