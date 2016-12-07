using System;
using System.Collections.Generic;
using UniLua;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;

class LuaUserdataGenerator
{
    static public int CreateByteArray(IntPtr l)
    {
        LuaState lua_ = LuaInstance.instance.Get();

        int size = (int)lua_.ToNumber(1);
        byte[] buffer = new byte[size];

        lua_.NewUserData(buffer);

        return 1;
    }
}
