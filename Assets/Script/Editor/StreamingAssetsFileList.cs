using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text;

public class StreamingAssetsFileList : EditorWindow 
{
    [MenuItem("CJ-TOOL/GenStreamingAssetsFileList")]

    static void GenStreamingAssetsFileList()
    {
        ArrayList al = new ArrayList();
        GetAllDirList(al, Application.streamingAssetsPath);
        Debug.Log(Application.streamingAssetsPath.Replace("/", "\\"));
        string filePath = Application.streamingAssetsPath + "\\StreamingFile.txt";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        FileStream funcInfoFile = new FileStream(filePath, FileMode.CreateNew,FileAccess.ReadWrite);
        StreamWriter sw = new StreamWriter(funcInfoFile, Encoding.UTF8);
        StringBuilder sb = new StringBuilder();
        int num = 0;
        for (int i = 0; i < al.Count; ++i)
        {
            DirectoryInfo di = new DirectoryInfo(al[i].ToString());
            FileInfo[] fiA = di.GetFiles();
            for (int j = 0; j < fiA.Length; ++j)
            {
                string path = al[i].ToString();
                path = path.Replace(Application.streamingAssetsPath.Replace("/", "\\") + "\\", "");
                path += "\\" + fiA[j].Name;
                path = path.Replace("\\", "/");
                //string miss = path.Substring(path.Length - 5, 5);
                if (fiA[j].Extension != ".meta")
                {
                    sb.AppendLine(path);
                    ++num;
                }
            }
        }

        Debug.Log("file count: " + num);
        sw.Write(sb);
        sw.Flush();
        sw.Close();
        funcInfoFile.Close();
        
        funcInfoFile = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        StreamReader sr = new StreamReader(funcInfoFile, Encoding.UTF8);
        string line = string.Empty;
        while ((line = sr.ReadLine()) != null)
        {
            Debug.Log("file: " + line);
        }

        sr.Close();
        funcInfoFile.Close();
        
    }

    private static void GetAllDirList(ArrayList al, string strBaseDir)
    {
        DirectoryInfo di = new DirectoryInfo(strBaseDir);
        DirectoryInfo[] diA = di.GetDirectories();
        for (int i = 0; i < diA.Length; ++i)
        {
            al.Add(diA[i].FullName);
            GetAllDirList(al, diA[i].FullName);
        }
    }
}
