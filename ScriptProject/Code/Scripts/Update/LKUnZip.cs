using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using System.Threading;
using System.IO.Compression;
class LKUnZip
{
    private float progress;
    public float Progress
    {
        get {return progress;}
    }
    private bool unZipSuccess=false;
    public bool UnZipSuccess
    {
        get { return unZipSuccess; }
    }
    private bool unZipIng=false;
    public bool UnZipIng
    {
        get { return unZipIng; }
    }
    private bool isError =false;
    public bool IsError
    {
        get { return isError; }
    }
    private bool existsUpdateLua=false;
    public bool ExistsUpdateLua
    {
        get { return existsUpdateLua; }
    }

    public void ExcuteUnzip(string zipPath, string destPath)
    {
        int UpdateRegisterClassStr = AutherFile.GetStringHash("UpdateUI/UpdateRegisterClass.txt");
        int UpdateConfigStr = AutherFile.GetStringHash("Utils/UpdateConfig.txt");
        int UpdateControllerStr = AutherFile.GetStringHash("Utils/UpdateController.txt");
        Thread thread = new Thread(delegate()
        {
            try
            {
                unZipIng = true;
                ZipFile z = new ZipFile(zipPath);
                long unZipCount = z.Count;
                z.Close();
                ZipInputStream zip = new ZipInputStream(File.OpenRead(zipPath));
                ZipEntry theEntry;
                int curUnzipCount = 0;
                while ((theEntry = zip.GetNextEntry()) != null)
                {
                    int theEntryName = int.Parse( theEntry.Name);
                    if (theEntry.IsFile)
                    {
                        byte[] bytesBuffer = new byte[theEntry.Size];
                        int i = zip.Read(bytesBuffer, 0, bytesBuffer.Length);
                        AutherFile.PutData(theEntryName, bytesBuffer);
                        if (theEntryName.Equals(UpdateRegisterClassStr) || theEntryName.Equals(UpdateConfigStr) ||
                            theEntryName.Equals(UpdateControllerStr))
                        {
                            existsUpdateLua = true;
                        }
                        Debug.Log("PutData " + theEntryName);
                    }
                    curUnzipCount++;
                    progress = (float)curUnzipCount / unZipCount;
                }
                zip.Close();
                zip.Dispose();
                unZipSuccess = true;
                unZipIng = false;
            }
            catch (Exception e)
            {
                isError = true;
                Debug.Log(e.Message);
            }
        });
        thread.Start();
    }
    public static void SynchroUnzip(string zipPath, string destPath)
    {
            ZipInputStream zip = new ZipInputStream(File.OpenRead(zipPath));
            ZipEntry theEntry;
            int curUnzipCount = 0;
            while ((theEntry = zip.GetNextEntry()) != null)
            {
                curUnzipCount++;
                string theEntryName = theEntry.Name.Replace("\\", "/");
                if (theEntry.IsFile)
                {
                    byte[] bytesBuffer = new byte[theEntry.Size];
                    int i = zip.Read(bytesBuffer, 0, bytesBuffer.Length);
                    string entryPath = Path.Combine(destPath, theEntryName);
                    GetFilePath(entryPath);
                    FileStream fs = File.Create(entryPath);
                    fs.Write(bytesBuffer, 0, i);
                    fs.Close();
                    fs.Dispose();
                }
                else if (theEntry.IsDirectory)
                {

                    string dirPath = Path.Combine(destPath, theEntryName);
                    GetFilePath(dirPath);
                }
            }
            zip.Close();
            zip.Dispose();
    
    }
    static void GetFilePath(string path)
    {
        int indexs = path.LastIndexOf("/");
        path = path.Substring(0, indexs);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);

        }
    }
}


class LKThreadCopyFile
{
    private string copyProgressCallback="";
    private string copyEndCallback="";
    private string copyErrorCallback="";
    float progress = -1;
    public string CopyProgressCallback
    {
        get {return copyProgressCallback;}
        set { copyProgressCallback = value; }
    }
    public string CopyEndCallback
    {
        get { return copyEndCallback; }
        set { copyEndCallback = value; }
    }
    public string CopyErrorCallback
    {
        get { return copyErrorCallback; }
        set { copyErrorCallback = value; }
    }
    public float Progress
    {
        get { return progress; }
        set { progress = value; }
    }
    public void ThreadCopyFile(string file, string destPath, string copyProgress, string copyEnd, string copyError)
    {
        if (!File.Exists(file))
        {
            copyErrorCallback = copyError;
        }
        else
        {
            copyProgressCallback = copyProgress;           
            FileInfo fi = new FileInfo(file);
            FileStream fs = fi.OpenRead();
            long lenght = fs.Length;
            int writeSize = 0;
            FileStream writeStream = new FileStream(Path.Combine(destPath, fi.Name), FileMode.Create);
            byte[] byteBuffer = new byte[4096 * 1024];
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    int curReadSize = fs.Read(byteBuffer, 0, byteBuffer.Length);
                    if (curReadSize > 0)
                    {
                        writeStream.Write(byteBuffer, 0, curReadSize);
                        writeSize += curReadSize;
                        progress = (float)writeSize / lenght;
                    }
                    else
                    {
                        break;
                    }
                }
                writeStream.Close();
                progress = -1;
                copyEndCallback = copyEnd;
            });
            thread.Start();
        }
    }

}
/*
    public  void ExcuteUnzip(string zipPath, string destPath)
    {
        ThreadPool.SetMinThreads(10, 20);
        ThreadPool.SetMaxThreads(30, 100);
        
        Thread thread = new Thread(delegate()
        {
            ManualResetEvent manualEvent = new ManualResetEvent(false);
            unZipIng = true;
            ZipFile z = new ZipFile(zipPath);
            long unZipCount = z.Count;
            z.Close();
            ZipInputStream zip = new ZipInputStream(File.OpenRead(zipPath));
            ZipEntry theEntry;
            int curUnzipCount = 0;
            while ((theEntry = zip.GetNextEntry()) != null)
            {
                
                string theEntryName = theEntry.Name.Replace("\\", "/");
                if (theEntry.IsFile)
                {
                    byte[] bytesBuffer = new byte[theEntry.Size];
                    try
                    {
                        int i = zip.Read(bytesBuffer, 0, bytesBuffer.Length);
                        if (theEntry.Name.StartsWith("AssetBundles"))
                        {
                            string entryPath = Path.Combine(destPath, theEntryName);
                            GetFilePath(entryPath);
                            ThreadPool.QueueUserWorkItem((x) =>
                                {
                                    FileStream fs = File.Create(entryPath);
                                    fs.Write(bytesBuffer, 0, i);
                                    fs.Close();
                                    curUnzipCount++;
                                    if (curUnzipCount == unZipCount)
                                    {
                                        manualEvent.Set();
                                    }
                                });
                        }
                        else
                        {
                            AutherFile.PutData(theEntryName, bytesBuffer);
                            Debug.Log("PutData " + theEntryName);
                            curUnzipCount++;
                            if (curUnzipCount == unZipCount)
                            {
                                manualEvent.Set();
                            }
                        }
                       

                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                        isError = true;
                    }
                }
                else if (theEntry.IsDirectory && theEntry.Name.StartsWith("AssetBundles"))
                {
                    curUnzipCount++;
                    string dirPath = Path.Combine(destPath, theEntryName);
                    GetFilePath(dirPath);
                }
                else
                {
                    curUnzipCount++;
                }

                progress = (float)curUnzipCount / unZipCount;
            }
            zip.Close();
            zip.Dispose();
            manualEvent.WaitOne(5000, true) ;
            unZipSuccess = true;
            unZipIng = false;
        });
        thread.Start();
    }*/