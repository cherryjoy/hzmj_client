using System;
using System.IO;
using System.Runtime.InteropServices;


public class UsStructTo
{
	//注：Marshal相关接口在IOS上不支持泛型

    public static byte[] StructToBytes(object structObj)
    {
        int size = Marshal.SizeOf(structObj);
        //创建byte数组
        byte[] bytes = new byte[size];
        //分配结构体大小的内存空间
        IntPtr structPtr = Marshal.AllocHGlobal(size);
        //将结构体拷到分配好的内存空间
        Marshal.StructureToPtr(structObj, structPtr, false);
        //从内存空间拷到byte数组
        Marshal.Copy(structPtr, bytes, 0, size);
        //释放内存空间
        Marshal.FreeHGlobal(structPtr);
        //返回byte数组
        return bytes;
    }

    public static object BytesToStuct(byte[] bytes,Type type)
    {
        //得到结构体的大小
        int size = Marshal.SizeOf(type);
        //byte数组长度小于结构体的大小
        if (size > bytes.Length)
        {
            //返回空
            return null;
         }
        //分配结构体大小的内存空间
         IntPtr structPtr = Marshal.AllocHGlobal(size);
        //将byte数组拷到分配好的内存空间
         Marshal.Copy(bytes,0,structPtr,size);
        //将内存空间转换为目标结构体
        object obj = Marshal.PtrToStructure(structPtr, type);
        //释放内存空间
        Marshal.FreeHGlobal(structPtr);
        //返回结构体
        return obj;
    }
}