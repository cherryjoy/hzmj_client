using System.Collections;
using System.Collections.Generic;
using System.Text;
namespace UniLua
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    // For iOS AOT
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute(Type t) { }
    }

    public class LuaState
    {
        private IntPtr m_lua = IntPtr.Zero;
        private LuaAPI.lua_CFunction m_pfnDispatch;
        private LuaAPI.lua_CFunction m_pfnPanic;
		private static LuaState m_GlobalState;

        public LuaState(IntPtr lua)
        {
            this.m_lua = lua;
			m_GlobalState = this;
            this.m_pfnPanic = LuaState.Lua_Panic;
            this.m_pfnDispatch = LuaState.Lua_DispatchFun;
            LuaAPI.lua_atpanic(this.m_lua, this.m_pfnPanic);
        }

        public void Close()
        {
           LuaAPI.luaclose_RLdb(this.m_lua);
        }

        private void CallPrint(string info, params object[] par)
        {
            string s = string.Format(info, par);
            this.GetGlobal("warn");
            if (this.Type(-1) != LuaType.LUA_TFUNCTION)
            {
                this.Pop(1);
            }
            else
            {
                this.PushString(s);
                if (this.PCall(1, 0, 0) != ThreadStatus.LUA_OK)
                {
                    this.Pop(1);
                }
            }
        }

        public bool CheckStack(int size)
        {
            return (LuaAPI.lua_checkstack(this.m_lua, size) != 0);
        }

        public void Concat(int n)
        {
            LuaAPI.lua_concat(this.m_lua, n);
        }


        public static IntPtr Create()
        {
            return LuaAPI.luaL_newstate();
        }

        public void CreateTable(int narray, int nrec)
        {
            LuaAPI.lua_createtable(this.m_lua, narray, nrec);
        }


        public int Error()
        {
            return LuaAPI.lua_error(this.m_lua);
        }


        public void GetField(int index, string key)
        {
            LuaAPI.lua_getfield(this.m_lua, index, key);
        }

        public void GetGlobal(string name)
        {
            LuaAPI.lua_getfield(this.m_lua, LuaAPI.LUA_GLOBALSINDEX, name);
        }

        public IntPtr GetLuaPtr()
        {
            return this.m_lua;
        }

        public bool GetMetaTable(int index)
        {
            return (LuaAPI.lua_getmetatable(this.m_lua, index) != 0);
        }

        public void GetTable(int index)
        {
            LuaAPI.lua_gettable(this.m_lua, index);
        }

        public int GetTop()
        {
            return LuaAPI.lua_gettop(this.m_lua);
        }



        public void Insert(int index)
        {
            LuaAPI.lua_insert(this.m_lua, index);
        }

        public bool IsFunction(int index)
        {
            return (LuaAPI.lua_type(this.m_lua, index) == 6);
        }

        public bool IsNil(int index)
        {
            return (LuaAPI.lua_type(this.m_lua, index) == 0);
        }

        public bool IsNone(int index)
        {
            return (LuaAPI.lua_type(this.m_lua, index) == -1);
        }

        public bool IsNoneOrNil(int index)
        {
            return (LuaAPI.lua_type(this.m_lua, index) <= 0);
        }

        public bool IsString(int index)
        {
            return (LuaAPI.lua_type(this.m_lua, index) == 4);
        }

        public bool IsTable(int index)
        {
            return (LuaAPI.lua_type(this.m_lua, index) == 5);
        }


        public int L_CheckInteger(int narg)
        {
            return (int)LuaAPI.luaL_checkinteger(this.m_lua, narg);
        }

        public double L_CheckNumber(int narg)
        {
            return LuaAPI.luaL_checknumber(this.m_lua, narg);
        }



        public string L_CheckString(int narg)
        {
            int l = 0;
            IntPtr ptr = LuaAPI.luaL_checklstring(this.m_lua, narg, ref l);
            if (ptr == IntPtr.Zero)
            {
                return string.Empty;
            }
            return LuaAPI.StringFromNativeUtf8(ptr, l);
        }

        public void L_CheckType(int index, LuaType t)
        {
            LuaAPI.luaL_checktype(this.m_lua, index, (int)t);
        }


        public ThreadStatus L_DoFile(string filename)
        {
#if ASSETBUNDLE
            if ((LuaAPI.luaL_loadpak(this.m_lua, filename) == 0) && (LuaAPI.lua_pcall(this.m_lua, 0, -1, 0) == 0))
            {
                return ThreadStatus.LUA_OK;
            }
#else
            if ((LuaAPI.luaL_loadfile(this.m_lua, filename) == 0) && (LuaAPI.lua_pcall(this.m_lua, 0, -1, 0) == 0))
            {
                return ThreadStatus.LUA_OK;
            }
#endif
            //string errorInfo = "luaError: " + LuaInstance.instance.Get().ErrorInfo(1) + Environment.NewLine;
            UnityEngine.Debug.LogError("DoFile Error: " + filename + "\nlua stack: " + LuaInstance.ConstructString(LuaInstance.instance.Get()));
            return ThreadStatus.LUA_ERRRUN;
        }

        public ThreadStatus L_DoString(string s)
        {
            ThreadStatus status = (ThreadStatus)LuaAPI.luaL_loadstring(this.m_lua, s);
            if (status != ThreadStatus.LUA_OK)
            {
                Debug.LogError(ToString(-1));
                return status;
            }
            status = (ThreadStatus)LuaAPI.lua_pcall(this.m_lua, 0, -1, 0);
            if (status != ThreadStatus.LUA_OK)
            {
                Debug.LogError(ToString(-1));
            }
            return status;
        }

        public void L_Require(string fileName) {
            LuaState lua = LuaInstance.instance.Get();
            lua.PushString(fileName);
            LuaAPI.ll_require(lua.GetLuaPtr());
        }


        public void L_OpenLibs()
        {
            LuaAPI.luaL_openlibs(this.m_lua);
        }



        public int L_Ref(int t)
        {
            return LuaAPI.luaL_ref(this.m_lua, t);
        }



        public void L_Unref(int t, ref int reference)
        {
            if (reference != LuaAPI.LUA_REFNIL)
            {
                LuaAPI.luaL_unref(this.m_lua, t, reference);
                reference = LuaAPI.LUA_REFNIL;
            }
        }



		[MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
        private static int Lua_DispatchFun(IntPtr L)
        {
            CSharpFunctionDelegate target = (CSharpFunctionDelegate)GCHandle.FromIntPtr(LuaAPI.lua_touserdata(L, -1001001)).Target;
			return target(LuaState.m_GlobalState);
        }

		[MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
		private static int Lua_Panic(IntPtr lua)
        {
            int len = 0;
            IntPtr ptr = LuaAPI.luaL_tolstring32(lua, -1, ref len);
            if (ptr != IntPtr.Zero)
            {
                Debug.LogError(string.Format("Lua Panic {0}", LuaAPI.StringFromNativeUtf8(ptr, len)));
            }
            return 0;
        }

        public void NewTable()
        {
            LuaAPI.lua_createtable(this.m_lua, 0, 0);
        }

        public void NewTable(int narr, int nrec = 0)
        {
            LuaAPI.lua_createtable(this.m_lua, narr, nrec);
        }

        public bool Next(int index)
        {
            return (LuaAPI.lua_next(this.m_lua, index) != 0);
        }

        public ThreadStatus PCall(int numArgs, int numResults, int errFunc)
        {
            LuaState lua = LuaInstance.instance.Get();
            int errorHandlerIndex = 0;
#if UNITY_EDITOR||UNITY_STANDALONE
            lua.GetGlobal("lua_get_debug_info");
            lua.PushValue(-2 - numArgs);
            for (int i = 0; i < numArgs; i++)
            {
                lua.PushValue(-numArgs - 2);
            }
            errorHandlerIndex = -2 - numArgs;
#endif
            if (LuaAPI.lua_pcall(this.m_lua, numArgs, numResults, errorHandlerIndex) != 0)
            {
                Debug.LogError("lua Error stack: " + LuaInstance.ConstructString(lua));
#if UNITY_EDITOR||UNITY_STANDALONE
                lua.Pop(numArgs + 3);
#endif

                return ThreadStatus.LUA_ERRRUN;
            }

#if UNITY_EDITOR||UNITY_STANDALONE
            int removeNum = numArgs + 2;
            while (removeNum > 0)
            {
                lua.Remove(-numResults - 1);
                removeNum--;
            }
#endif
           
            return ThreadStatus.LUA_OK;
        }

        public void SimpleLuaFuncCall(string scriptName,string funcName) {
            GetGlobal(scriptName);
            GetField(-1, funcName);
            PCall(0, 0, 0);
            Pop(1);
        }

        public void LuaFuncCall(int luaObj_ref, string funcName)
        {
            if (LuaGetFunc(luaObj_ref, funcName))
            {
                RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaObj_ref);
                PCall(1, 0, 0);

                Pop(1);
            }
        }


        public void LuaFuncCall(int luaObj_ref, string funcName, object obj)
        {
            if (LuaGetFunc(luaObj_ref, funcName))
            {
                RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaObj_ref);
                PushObj(obj);
                PCall(2, 0, 0);

                Pop(1);
            }
        }

        public void LuaFuncCall(int luaObj_ref, string funcName, object obj1, object obj2)
        {
            if (LuaGetFunc(luaObj_ref, funcName))
            {
                RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaObj_ref);
                PushObj(obj1);
                PushObj(obj2);
                PCall(3, 0, 0);

                Pop(1);
            }
        }

        public void LuaStaticFuncCall(int luaclass_ref, string funcName, object obj)
        {
            if (LuaGetFunc(luaclass_ref, funcName))
            {
                PushObj(obj);
                PCall(1, 0, 0);

                Pop(1);
            }
        }

        public void LuaStaticFuncCall(int luaclass_ref, string funcName, object obj1,object obj2)
        {
            if (LuaGetFunc(luaclass_ref, funcName))
            {
                PushObj(obj1);
                PushObj(obj2);
                PCall(2, 0, 0);

                Pop(1);
            }
        }

        void PushObj(object p) {
            //is object
            if (!LuaInstance.PushBaseTypeObj(this, p.GetType(), p))
            {
                //not base type or base type in object
                NewClassUserData(p);
            }
        }

        bool LuaGetFunc(int luaclass_ref, string funcName)
        {
            if (luaclass_ref == LuaAPI.LUA_REFNIL)
                return false;
            RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaclass_ref);
            GetField(-1, funcName);

            if (IsNil(-1))
            {
#if UNITY_EDITOR
                Debug.LogWarning(funcName + " dont exist.");
#endif
                Pop(2);
                return false;
            }

            return true;
        }


        
        public void Pop(int n)
        {
            LuaAPI.lua_settop(this.m_lua, -n - 1);
        }

        public void PushBoolean(bool b)
        {
            LuaAPI.lua_pushboolean(this.m_lua, !b ? 0 : 1);
        }

        public GCHandle PushCSharpClosure(CSharpFunctionDelegate f, int n)
        {
            GCHandle handle = GCHandle.Alloc(f);
            IntPtr p = GCHandle.ToIntPtr(handle);
            LuaAPI.lua_pushlightuserdata(this.m_lua, p);
            LuaAPI.lua_insert(this.m_lua, -(n + 1));
            LuaAPI.lua_pushcclosure(this.m_lua, this.m_pfnDispatch, n + 1);
            return handle;
        }

        public GCHandle PushCSharpFunction(CSharpFunctionDelegate f)
        {
            GCHandle handle = GCHandle.Alloc(f);
            IntPtr p = GCHandle.ToIntPtr(handle);
            LuaAPI.lua_pushlightuserdata(this.m_lua, p);
            LuaAPI.lua_pushcclosure(this.m_lua, this.m_pfnDispatch, 1);
            return handle;
        }

        public void PushLuaClosure(LuaAPI.lua_CFunction func, int n) {
            LuaAPI.lua_pushcclosure(this.m_lua, func, n);
        
        }


        public void PushInteger(int n)
        {
//#if UNITY_ANDROID 
//             LuaAPI.lua_pushinteger(this.m_lua, n);
//#elif UNITY_IPHONE || UNITY_STANDALONE_WIN
//            long longN = n;
//            LuaAPI.lua_pushinteger(this.m_lua, longN);
//#endif
			LuaAPI.luaL_pushinteger32(this.m_lua, n);
        }

        public GCHandle PushLightUserData(object o)
        {
            GCHandle handle = GCHandle.Alloc(o);
            LuaAPI.lua_pushlightuserdata(this.m_lua, GCHandle.ToIntPtr(handle));
            return handle;
        }

        public void PushNil()
        {
            LuaAPI.lua_pushnil(this.m_lua);
        }

        public void PushNumber(double n)
        {
            LuaAPI.lua_pushnumber(this.m_lua, n);
        }

        public void PushString(string s)
        {
           // int len = 0;
          //  IntPtr ptr = LuaAPI.NativeUtf8FromString(s, ref len);
            LuaAPI.lua_pushstring(this.m_lua, s);
        }



        public void PushValue(int index)
        {
            LuaAPI.lua_pushvalue(this.m_lua, index);
        }

        public void PushIntList(List<int> list)
        {
            if (list != null)
            {
                NewTable(list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    PushInteger(list[i]);
                    RawSetI(-2, i + 1);
                }
            }
            else
            {
                NewTable();
                Debug.Log("List is null!");
            }
        }

        public void PushFloatList(List<float> list)
        {
            if (list != null)
            {
                NewTable(list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    PushNumber(list[i]);
                    RawSetI(-2, i + 1);
                }
            }
            else
            {
                NewTable();
                Debug.Log("List is null!");
            }
        }

        public void PushBoolList(List<bool> list)
        {
            if (list != null)
            {
                NewTable(list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    PushBoolean(list[i]);
                    RawSetI(-2, i + 1);
                }
            }
            else
            {
                NewTable();
                Debug.LogError("List is null!");
            }
        }

        public void PushStringList(List<String> list)
        {
            if (list != null)
            {
                NewTable(list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    PushString(list[i]);
                    RawSetI(-2, i + 1);
                }
            }
            else
            {
                NewTable();
                Debug.LogError("List is null!");
            }
        }

        public bool RawEqual(int index1, int index2)
        {
            return (LuaAPI.lua_rawequal(this.m_lua, index1, index2) != 0);
        }

        public void RawGet(int index)
        {
            LuaAPI.lua_rawget(this.m_lua, index);
        }

        public void RawGetI(int index, int n)
        {
            LuaAPI.lua_rawgeti(this.m_lua, index, n);
        }

        public int RawLen(int index)
        {
            return LuaAPI.lua_objlen(this.m_lua, index);
        }

        public void RawSet(int index)
        {
            LuaAPI.lua_rawset(this.m_lua, index);
        }

        public void RawSetI(int index, int n)
        {
            LuaAPI.lua_rawseti(this.m_lua, index, n);
        }

        public void Remove(int index)
        {
            LuaAPI.lua_remove(this.m_lua, index);
        }

        public void Replace(int index)
        {
            LuaAPI.lua_replace(this.m_lua, index);
        }

        public void SetField(int index, string key)
        {
            LuaAPI.lua_setfield(this.m_lua, index, key);
        }

        public void SetGlobal(string name)
        {
            LuaAPI.lua_setfield(this.m_lua, LuaAPI.LUA_GLOBALSINDEX, name);
        }

        public bool SetMetaTable(int index)
        {
            return (LuaAPI.lua_setmetatable(this.m_lua, index) != 0);
        }
        public int NewMetaTable(string name)
        {
            return LuaAPI.luaL_newmetatable(this.m_lua, name);
        }

        public void SetTable(int index)
        {
            LuaAPI.lua_settable(this.m_lua, index);
        }

        public void SetTop(int top)
        {
            LuaAPI.lua_settop(this.m_lua, top);
        }


        public bool ToBoolean(int index)
        {
            return (LuaAPI.lua_toboolean(this.m_lua, index) != 0);
        }

        public int ToInteger(int index)
        {
            return (int)LuaAPI.lua_tonumber(this.m_lua, index);
        }

        public int ToIntegerX(int index, out bool isnum)
        {
            int num = 0;
            long num2 = LuaAPI.lua_tointeger(this.m_lua, index);
            isnum = num != 0;
            return (int)num2;
        }

        public double ToNumber(int index)
        {
            return LuaAPI.lua_tonumber(this.m_lua, index);
        }

        public object ToObject(int index)
        {
            IntPtr ptr = LuaAPI.lua_topointer(this.m_lua, index);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            GCHandle handle = (GCHandle)ptr;
            object target = handle.Target;
            handle.Free(); // FIX, dont free this
            return target;
        }

        public object ToUserDataObject(int index)
        {
            IntPtr ptr = LuaAPI.lua_topointer(this.m_lua, index);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            IntPtr data_ptr = Marshal.ReadIntPtr(ptr);
            if (data_ptr == IntPtr.Zero)
            {
                return null;
            }
            GCHandle handle = GCHandle.FromIntPtr(data_ptr);
            return handle.Target;
        }

        public byte[] CopyBytesFromUnManaged(int index,int length)
        {
            IntPtr ptr = LuaAPI.lua_topointer(this.m_lua, index);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            byte[] data = new byte[length];
            Marshal.Copy(ptr, data, 0, length);
            return data;
        }

        public void PushLongInterger(long n)
        {
            LuaAPI.lua_pushlong(this.m_lua, n);
        }

        public long ReadLongFromUnManaged(int index)
        {
            IntPtr ptr = LuaAPI.lua_topointer(this.m_lua, index);
            if (ptr == IntPtr.Zero)
            {
                Debug.LogWarning("read long: ptr is null");
                return -1;
            }
            
            return Marshal.ReadInt64(ptr,0);
        }

        public long ReadLongId(int index) {
            long longValue = LuaAPI.lua_toID(this.m_lua, index);
            return longValue;
        }

        public void PushLongId(long v) {
            LuaAPI.lua_pushID(this.m_lua, v);
        }

        public GCHandle ToUserDataHandler(int index)
        {
            IntPtr ptr = LuaAPI.lua_topointer(this.m_lua, index);
            if (ptr == IntPtr.Zero)
            {
                return new GCHandle();
            }
            IntPtr data_ptr = Marshal.ReadIntPtr(ptr);
            if (data_ptr == IntPtr.Zero)
            {
                return new GCHandle();
            }
            GCHandle handle = GCHandle.FromIntPtr(data_ptr);
            return handle;
        }

        public string ToString(int index)
        {
            int len = 0;
            IntPtr ptr = LuaAPI.luaL_tolstring32(this.m_lua, index, ref len);
            if (ptr == IntPtr.Zero)
            {
                return string.Empty;
            }
            return Marshal.PtrToStringAnsi(ptr);
        }


        public int[] ToIntArray(int index)
        {
            int length = LuaAPI.lua_objlen(this.m_lua, index);
            int[] array = new int[length];

            for (int i = 0; i < length; i++)
            {
                RawGetI(index, i + 1);
                array[i] = (int)ToInteger(-1);
                Pop(1);
            }

            return array;
        }

        public float[] ToFloatArray(int index)
        {
            int length = LuaAPI.lua_objlen(this.m_lua, index);
            float[] array = new float[length];

            for (int i = 0; i < length; i++)
            {
                RawGetI(index, i + 1);
                array[i] = (float)ToNumber(-1);
                Pop(1);
            }

            return array;
        }

        public bool[] ToBoolArray(int index)
        {
            int length = LuaAPI.lua_objlen(this.m_lua, index);
            bool[] array = new bool[length];

            for (int i = 0; i < length; i++)
            {
                RawGetI(index, i + 1);
                array[i] = (bool)ToBoolean(-1);
                Pop(1);
            }

            return array;
        }

        public string[] ToStringArray(int index)
        {
            int length = LuaAPI.lua_objlen(this.m_lua, index);
            string[] array = new string[length];

            for (int i = 0; i < length; i++)
            {
                RawGetI(index, i + 1);
                array[i] = (string)ToString(-1);
                Pop(1);
            }

            return array;
        }

        public GCHandle ToUserData(int index)
        {
            return GCHandle.FromIntPtr(LuaAPI.lua_topointer(this.m_lua, index));
        }

        private GCHandle NewUserData(object o)
        {
            GCHandle handle = GCHandle.Alloc(o);
            IntPtr obj_ptr = GCHandle.ToIntPtr(handle);
           
            IntPtr ptr = LuaAPI.lua_newuserdata(this.m_lua, IntPtr.Size);

            Marshal.WriteIntPtr(ptr, obj_ptr);
      
            return handle;
        }

        public bool NewClassUserData(object obj)
        {
            if (obj == null)
            {
                PushNil();
                return true;
            }
            LuaClass lua_class = null;
            if (LuaInstance.instance.classesDic_.TryGetValue(obj.GetType(), out lua_class) == true)
            {
                NewUserData(obj);
                RawGetI(LuaAPI.LUA_REGISTRYINDEX, lua_class.type_ref_);
                PushLuaClosure(LuaState.GC, 0);
                SetField(-2, "__gc");
                SetMetaTable(-2);

                return true;
            }
            else
            {
                Debug.LogError("-------No Class Found In Lua classesDic--------" + "Type   " + obj.GetType());
                return false;
            }
        }

        public bool NewTypeClassUserData(object obj,Type type)
        {
            LuaClass lua_class = null;
            if (LuaInstance.instance.classesDic_.TryGetValue(type, out lua_class) == true)
            {
                NewUserData(obj);
                RawGetI(LuaAPI.LUA_REGISTRYINDEX, lua_class.type_ref_);
                PushLuaClosure(LuaState.GC, 0);
                SetField(-2, "__gc");
                SetMetaTable(-2);

                return true;
            }
            else
            {
                Debug.LogError("-------No Class Found In Lua classesDic--------" + "Type   " + obj.GetType());
                return false;
            }
        }

        //use for struct to lua
        public void NewUserDataWithGC(object o)
        {
            GCHandle handle = GCHandle.Alloc(o);
            IntPtr obj_ptr = GCHandle.ToIntPtr(handle);
            IntPtr ptr = LuaAPI.lua_newuserdata(this.m_lua, IntPtr.Size);
            Marshal.WriteIntPtr(ptr, obj_ptr);
            SetGCFunc();
        }

        public void SetGCFunc()
        {
            LuaState lua = LuaInstance.instance.Get();
            lua.NewTable();
            lua.PushLuaClosure(GC, 0);
            lua.SetField(-2, "__gc");
            lua.SetMetaTable(-2);
        }

        public GCHandle NewUnManageMem(object o) {
            GCHandle handle = GCHandle.Alloc(o, GCHandleType.Pinned);
            IntPtr obj_ptr = handle.AddrOfPinnedObject();
            LuaInstance.instance.Get().PushNumber((double)obj_ptr.ToInt64());
            return handle;
        }

        [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
        public static int GC(IntPtr l)
        {
            GCHandle h = LuaInstance.instance.Get().ToUserDataHandler(-1);
            h.Free();
            
            return 0;
        }
        public LuaType Type(int index)
        {
            return (LuaType)LuaAPI.lua_type(this.m_lua, index);
        }

        public string TypeName(LuaType t)
        {
            IntPtr ptr = LuaAPI.lua_typename(this.m_lua, (int)t);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            return LuaAPI.StringFromNativeUtf8(ptr, 0);
        }

        public int UpvalueIndex(int i)
        {
            return (LuaAPI.LUA_GLOBALSINDEX - i);
        }

        public string ErrorInfo() { 
            LuaAPI.lua_geterror_info(this.m_lua);
            if (this.IsNil(-1))
            {
                return string.Empty;
            }
            string errorInfo = LuaInstance.ConstructString(LuaInstance.instance.Get());
            return errorInfo;
        
        }

        public object GetObjByType(LuaAPI.VarType type,int index)
        {
            if (type == LuaAPI.VarType.E_Bool)
            {
                return ToBoolean(index);
            }
            else if (type == LuaAPI.VarType.E_Double)
            {
                return ToNumber(index);
            }
            else if (type == LuaAPI.VarType.E_Float)
            {
                return (float)ToNumber(index);
            }
            else if (type == LuaAPI.VarType.E_Int)
            {
                return ToInteger(index);
            }
            else if (type == LuaAPI.VarType.E_Long)
            {
                return ReadLongId(index);
            }
            else if (type == LuaAPI.VarType.E_String)
            {
                return ToString(index);
            }
            else if (type == LuaAPI.VarType.E_Object)
            {
                return ToUserDataObject(index);
            }

            return null;
        }
    }
}

