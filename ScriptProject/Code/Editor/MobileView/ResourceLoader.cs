using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;

public class ResourceLoader {
    public static ResourceLoader Instance()
    {
        if (null == instance)
        {
            instance = new ResourceLoader();
        }
        return instance;
    }
  
    public static string ftpServerIP = "10.10.41.211";
    public static string SceneName = "scene001";
    public static string filePath  = "/mnt/sdcard/lk/DB";

    public static string filename = "data";
    public static string downloadError = "";


    public static string ftpUserID = "kingnet";
    public static string ftpPassword = "king123";

    public static int downLoadState = 0; //0---未下载   1---下载中    2---下载完
    private static ResourceLoader instance = null;
    private ResourceLoader()
    {

    }
    /// <summary>
    /// ftp方式上传 
    /// </summary>
    public static int UploadFtp()
    {
        FileInfo fileInf = new FileInfo(filePath + "\\" + filename);
        string uri = "ftp://" + ftpServerIP + "/" + fileInf.Name;
        FtpWebRequest reqFTP;
        // Create FtpWebRequest object from the Uri provided 
        reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + fileInf.Name));
        try
        {
            // Provide the WebPermission Credintials 
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);

            // By default KeepAlive is true, where the control connection is not closed 
            // after a command is executed. 
            reqFTP.KeepAlive = false;

            // Specify the command to be executed. 
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

            // Specify the data transfer type. 
            reqFTP.UseBinary = true;

            // Notify the server about the size of the uploaded file 
            reqFTP.ContentLength = fileInf.Length;

            // The buffer size is set to 2kb 
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;

            // Opens a file stream (System.IO.FileStream) to read the file to be uploaded 
            //FileStream fs = fileInf.OpenRead(); 
            FileStream fs = fileInf.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // Stream to which the file to be upload is written 
            Stream strm = reqFTP.GetRequestStream();

            // Read from the file stream 2kb at a time 
            contentLen = fs.Read(buff, 0, buffLength);

            // Till Stream content ends 
            while (contentLen != 0)
            {
                // Write Content from the file stream to the FTP Upload Stream 
                strm.Write(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);
            }

            // Close the file stream and the Request Stream 
            strm.Close();
            fs.Close();
            return 0;
        }
        catch (Exception ex)
        {
            reqFTP.Abort();
            //  Logging.WriteError(ex.Message + ex.StackTrace);
            return -2;
        }
    }
    public static void DownLoadData()
    {
        //test
        // filename = "download.cs";
        //  DownloadFtp();
        //test
        downLoadState = 1;
        downloadError = "";
        Debug.Log("yyss:beginDownLoad");
        filename = "fragment";
        DownloadFtp();
        if (downloadError != "")
        {
            downLoadState = 2;
            return;
        }
        filename = "index";
        DownloadFtp();
        if (downloadError != "")
        {
            downLoadState = 2;
            return;
        }
        filename = "data";
        DownloadFtp();
        Debug.Log("yyss:endDownLoad");
        downLoadState = 2;

    }
    /// <summary>
    /// ftp方式下载 
    /// </summary>
    public static void DownloadFtp()
    {
        FtpWebRequest reqFTP;
        Stream ftpStream = null;
        FileStream outputStream = null;
        FtpWebResponse response = null;
        try
        {
            //filePath = < <The full path where the file is to be created.>>, 
            //filename = < <Name of the file to be created(Need not be the name of the file on FTP server).>> 
            if (!Directory.Exists(filePath))
            {
                Debug.Log("yyss:create file path:" + filePath);
                Directory.CreateDirectory(filePath);
                Debug.Log("yyss:pass file path:" + filePath);
            }
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + filename));
            reqFTP.Timeout = 5000;
            Debug.Log("yyss:ftpCreate" + ftpServerIP + "/" + filename + "/" + ftpUserID + "/" + ftpPassword +"#");
            reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
            reqFTP.UseBinary = true;
            reqFTP.KeepAlive = false;
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            response = reqFTP.GetResponse() as FtpWebResponse;
            ftpStream = response.GetResponseStream();
            Debug.Log("yyss:ftpResponse");
            long cl = response.ContentLength;
            int bufferSize = 2048;
            int readCount;
            byte[] buffer = new byte[bufferSize];


            FileInfo fileInfo = new FileInfo(filePath + "/" + filename);
            Debug.Log("yyss: fileinfo:"+ filePath + "/" + filename);
            outputStream = fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

            readCount = ftpStream.Read(buffer, 0, bufferSize);
            while (readCount > 0)
            {
                outputStream.Write(buffer, 0, readCount);
                readCount = ftpStream.Read(buffer, 0, bufferSize);
            }

            ftpStream.Close();
            outputStream.Close();
            response.Close();
        }
        catch (Exception ex)
        {
            if (null != ftpStream)
            {
                ftpStream.Close();
            }
            if (null != outputStream)
            {
                outputStream.Close();
            }
            if (null != response)
            {
                response.Close();
            }

            downloadError = ex.Message;
            Debug.Log("yyss:" + ex.Message);
            // Logging.WriteError(ex.Message + ex.StackTrace);
            // System.Windows.Forms.MessageBox.Show(ex.Message);
        }
    }
}
