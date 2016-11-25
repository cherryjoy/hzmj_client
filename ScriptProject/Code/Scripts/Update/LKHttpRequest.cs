using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using UnityEngine;
using System.Threading;
using UniLua;
class LKHttpRequest
{
    protected HttpWebResponse httpResponse;
    protected long downloadLength;
//    protected string downLoadBeginCallBack;
    protected HttpWebRequest httpRequest;
//     protected string downLoadProgressCallback;
//     protected string downLoadErrorCallback;
//     protected string downLoadEndCallback;
//     protected int luaVoiceHanderRef;
    protected string errorMsg="";
    protected Action<int, int> downLoadBeginCallBack;
    protected Action<float> downLoadProgressCallback;
    protected Action<string> downLoadErrorCallback;
    protected Action<string> downLoadEndCallback;
    protected enum HTTPEQUESTSTATE
    {
        HTTPEQUESTNONE,
        HTTPEQUESTERROR,
        HTTPEQUESTBEGIAN,
        HTTPEQUESTDOWNING,
        HTTPEQUESTEND,
        HTTPEQUESTBEGIANDOWNLOAD,//确认下载的回调状态 之后开始下载
    }
    protected object httpRequestState=HTTPEQUESTSTATE.HTTPEQUESTNONE;
    public LKHttpRequest(Action<int, int> downloadbegin, Action<float> downloading, Action<string> downloadEnd, Action<string> downloadError)
    {
        downLoadBeginCallBack = downloadbegin;
        downLoadEndCallback = downloadEnd;
        downLoadProgressCallback = downloading;
        downLoadErrorCallback = downloadError;
    }
    public virtual void Update()
    {
    }

    public virtual void Download(string url, string path=null, string writeSizeFilePath=null)
    {

        System.Net.ServicePointManager.DefaultConnectionLimit = 1000;
        httpRequest = (HttpWebRequest)WebRequest.Create(url + "?id=" + DateTime.Now.Ticks.ToString());
        httpRequest.ReadWriteTimeout = 5000;
        httpRequest.Method = "GET";
        httpRequest.Timeout = 5000;
        try
        {
            httpRequest.BeginGetResponse(new AsyncCallback (BeginGetResponseCallback), httpRequest);
        }
        catch (System.Exception ex)
        {
            errorMsg = ex.Message;
            Debug.Log(ex.Message);
            lock (httpRequestState)
            {
                httpRequestState = HTTPEQUESTSTATE.HTTPEQUESTERROR;
            }
        }
    }
    public  void BeginGetResponseCallback(IAsyncResult ar)
    {
        try
        {
            httpRequest = ar.AsyncState as HttpWebRequest;
            httpResponse = httpRequest.EndGetResponse(ar) as HttpWebResponse;

        }
        catch (WebException ex)
        {
            if (ex.Response != null && (ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
            {

                lock (httpRequestState)
                {
                    httpRequestState = HTTPEQUESTSTATE.HTTPEQUESTEND;

                }
                return;
            }

            else
            {
                lock (httpRequestState)
                {
                    httpRequestState = HTTPEQUESTSTATE.HTTPEQUESTERROR;
                }
                Debug.Log("http error " + ex.Message);
                return;
            }

        }
        if ((int)httpResponse.StatusCode / 100 != 2)
        {
            lock (httpRequestState)
            {
                httpRequestState = HTTPEQUESTSTATE.HTTPEQUESTERROR;
            }
            return;
        }

        Debug.Log("httpResponse.ContentLength: " + httpResponse.ContentLength);
        downloadLength = httpResponse.ContentLength;
        ReadFromStream();

    }

    public virtual void ReadFromStream()
    {
  
    }

    public virtual void Close()
    {
        if (httpResponse != null)
        {
            httpResponse.Close();
        }
        if (httpRequest != null)
        {
            httpRequest.Abort();
        }
        httpRequest = null;
        httpResponse = null;
    }

    public void SetBegianDownload()
    {
        lock (httpRequestState)
        {
            httpRequestState = HTTPEQUESTSTATE.HTTPEQUESTBEGIANDOWNLOAD;
        }
    }

}
  
