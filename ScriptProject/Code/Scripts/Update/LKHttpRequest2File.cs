using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using System.Threading;

class LKHttpRequest2File : LKHttpRequest
{
    private string savePath;
    private float progress;
    private FileStream writeSizeFile;
    AutoResetEvent waitEvent = new AutoResetEvent(false);
    int offset=0;

    private string writeSizeFilePath = "";
    public override void Download(string url, string path,string wSizeFilePath)
    {
        savePath = path;
        writeSizeFilePath = wSizeFilePath;
        System.Net.ServicePointManager.DefaultConnectionLimit = 100;
        url =  url + "?id=" + DateTime.Now.Ticks.ToString();
        //Debug.Log(url);
        Debug.Log(writeSizeFilePath);
        httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
        httpRequest.ReadWriteTimeout = 5000;
        httpRequest.Timeout = 5000;
        httpRequest.Method = "GET";
        if (File.Exists(writeSizeFilePath)&&File.Exists(path))
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(writeSizeFilePath);
                offset = System.BitConverter.ToInt32(bytes, 0);
            }
            catch (Exception e)
            {
                File.Delete(writeSizeFilePath);
                offset = 0;
            }
        }
        Debug.Log(offset);
        httpRequest.AddRange(offset);
        httpRequest.BeginGetResponse(new AsyncCallback(BeginGetResponseCallback), httpRequest);
    }

    int  GetFileSize()
    {
        if (writeSizeFile != null)
        {
            byte[] bytes = new byte[128];
            int l = writeSizeFile.Read(bytes, 0, bytes.Length);
            int size=0;
            size = System.BitConverter.ToInt32(bytes, 0);
            return size;
           
        }
        else
        {
            return 0;
        }
    }

    public override void Update()
    {
        switch ((HTTPEQUESTSTATE)httpRequestState)
        {
            case HTTPEQUESTSTATE.HTTPEQUESTBEGIAN:
                if (downLoadBeginCallBack != null)
                {
                    Debug.Log("downloadLength: " + (int)downloadLength);
                    downLoadBeginCallBack(offset,(int)downloadLength);
                }
                //CallLuaFun(downLoadBeginCallBack, downloadLength.ToString());
                lock (httpRequestState)
                {
                    httpRequestState = LKHttpRequest.HTTPEQUESTSTATE.HTTPEQUESTNONE;
                }
                //waitEvent.Set();
                break;
            case HTTPEQUESTSTATE.HTTPEQUESTBEGIANDOWNLOAD:
                 lock (httpRequestState)
                {
                    httpRequestState = LKHttpRequest.HTTPEQUESTSTATE.HTTPEQUESTNONE;
                }
                waitEvent.Set();
                break;
            case HTTPEQUESTSTATE.HTTPEQUESTDOWNING:
                if (downLoadProgressCallback != null)
                {
                    downLoadProgressCallback(progress);
                }
               // CallLuaFun(downLoadProgressCallback, progress);
                break;
            case HTTPEQUESTSTATE.HTTPEQUESTEND:
                if (downLoadEndCallback != null)
                {
                    downLoadEndCallback("");
                }
                //CallLuaFun(downLoadEndCallback, "");
                lock (httpRequestState)
                {
                    httpRequestState = LKHttpRequest.HTTPEQUESTSTATE.HTTPEQUESTNONE;
                }
                Close();
                break;
            case HTTPEQUESTSTATE.HTTPEQUESTERROR:
                if (downLoadErrorCallback != null)
                {
                    downLoadErrorCallback(errorMsg);
                }    
            //CallLuaFun(downLoadErrorCallback, "");
                lock (httpRequestState)
                {
                    httpRequestState = LKHttpRequest.HTTPEQUESTSTATE.HTTPEQUESTNONE;
                }
                Close();
                break;
        }
    }

    public override void ReadFromStream()
    {
        lock (httpRequestState)
        {
            httpRequestState = LKHttpRequest.HTTPEQUESTSTATE.HTTPEQUESTBEGIAN;
        }
        waitEvent.WaitOne();
        lock (httpRequestState)
        {
            httpRequestState = LKHttpRequest.HTTPEQUESTSTATE.HTTPEQUESTDOWNING;
        }
        FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        fs.Seek(offset, SeekOrigin.Begin);
        byte[] buffer = new byte[4096*10];
        Stream stream = null;
        try
        {
             stream = httpResponse.GetResponseStream();
            long readSize = 0;
            long curReadSize = 0;
            int loopCount = 0;
            while (true)
            {
                curReadSize = stream.Read(buffer, 0, buffer.Length);
                if (curReadSize > 0)
                {
                    fs.Write(buffer, 0, (int)curReadSize);
                    fs.Flush();
                    readSize += curReadSize;
                    if (loopCount == 10)
                    {
                        write2SizeFile(offset + (int)readSize);
                        loopCount = 0;
                    }
                    loopCount++;
                    progress = (float)(readSize+offset) /(offset+ downloadLength);
                }
                else
                {
                    break;
                    
                }
            }
            fs.Close();
            stream.Close();
            if (readSize != downloadLength)
            {
                lock (httpRequestState)
                {
                    httpRequestState = LKHttpRequest.HTTPEQUESTSTATE.HTTPEQUESTERROR;
                }
            }
            else
            {
                lock (httpRequestState)
                {
                    httpRequestState = LKHttpRequest.HTTPEQUESTSTATE.HTTPEQUESTEND;
                }
            }
        }
        catch (Exception ex)
        {
            fs.Close();
            if (stream != null)
            {
                stream.Close();
            }
            Debug.Log(ex.Message);
            lock (httpRequestState)
            {
                httpRequestState = LKHttpRequest.HTTPEQUESTSTATE.HTTPEQUESTERROR;
            }
        }
        
    }

    public LKHttpRequest2File( Action<int,int> downloadbegin, Action<float> downloading, Action<string> downloadEnd, Action<string> downloadError)
        : base( downloadbegin, downloading, downloadEnd, downloadError)
    { }
    void write2SizeFile(int size)
    {
        try
        {
            byte[] bytes = System.BitConverter.GetBytes(size);
            FileStream fs = new FileStream(writeSizeFilePath, FileMode.Create);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
        }
        catch (Exception e)
        {
            lock (httpRequestState)
            {
                httpRequestState = LKHttpRequest.HTTPEQUESTSTATE.HTTPEQUESTERROR;
            }
            Debug.Log(e.Message);
            errorMsg = e.Message;
            File.Delete(writeSizeFilePath);
        }

    }
}

