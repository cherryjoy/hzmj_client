using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


public class AutherFile
{
#if UNITY_IPHONE
	private const String DllName = "__Internal";
#else
    private const String DllName = "libCore";
#endif

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool LKFile_Init(string path);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void LKFile_DeleteFile(string path);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool LKFile_PutData(int name, byte[] data, int len);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr LKFile_GetData(string name, ref int len);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void LKFile_FreeData(IntPtr data);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int LKFile_GetDataOffset(string name);
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetStringHash(string key);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int LKFile_InitOnlyRead(string name);
    public static byte[] GetData(string name)
    {
        int len = 0;
        IntPtr p = LKFile_GetData(name, ref len);
        if (IntPtr.Zero != p)
        {
            byte[] data = new byte[len];
            Marshal.Copy(p, data, 0, len);
            LKFile_FreeData(p);
            
            return data;
        }
        else
            return null;
    }

    public static bool PutData(int name, byte[] data)
    {
        return LKFile_PutData(name, data, (int)data.Length);
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool LKFile_UnInit();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr LKFile_GetFileList(ref int size);

    public static string[] GetFileList()
    {
        int size = 0;
        IntPtr pfiles = LKFile_GetFileList(ref size);
        string[] files = new string[size];

        if (size > 0)
        {
            IntPtr[] pV = new IntPtr[size];
            Marshal.Copy(pfiles, pV, 0, size);
            for (int i = 0; i < size; i++)
            {
                string s = Marshal.PtrToStringAnsi(pV[i]);
                LKFile_FreeData(pV[i]);
                files[i] = s;
            }
        }

        return files;
    }
}

