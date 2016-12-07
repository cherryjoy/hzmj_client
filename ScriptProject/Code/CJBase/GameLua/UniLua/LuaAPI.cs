namespace UniLua
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class LuaAPI
    {
        public const int LUA_GCCOLLECT = 2;
        public const int LUA_GCCOUNT = 3;
        public const int LUA_GCCOUNTB = 4;
        public const int LUA_GCGEN = 10;
        public const int LUA_GCINC = 11;
        public const int LUA_GCISRUNNING = 9;
        public const int LUA_GCRESTART = 1;
        public const int LUA_GCSETMAJORINC = 8;
        public const int LUA_GCSETPAUSE = 6;
        public const int LUA_GCSETSTEPMUL = 7;
        public const int LUA_GCSTEP = 5;
        public const int LUA_GCSTOP = 0;
        public const int LUA_NOREF = -2;
        public const int LUA_OPADD = 0;
        public const int LUA_OPDIV = 3;
        private const int LUA_OPEQ = 0;
        private const int LUA_OPLE = 2;
        private const int LUA_OPLT = 1;
        public const int LUA_OPMOD = 4;
        public const int LUA_OPMUL = 2;
        public const int LUA_OPPOW = 5;
        public const int LUA_OPSUB = 1;
        public const int LUA_OPUNM = 6;
        public const int LUA_REFNIL = -1;
        public const int LUA_IDSIZE = 60;
        //public const int LUA_REGISTRYINDEX = -1001000;
        public const int LUA_REGISTRYINDEX = -10000;
        public const int LUA_GLOBALSINDEX = -10002;
        public const int LUA_ENVIRONINDEX = -10001;

        public const int LUA_HOOKCALL = 0;
        public const int LUA_HOOKRET = 1;
        public const int LUA_HOOKLINE = 2;
        public const int LUA_HOOKCOUNT = 3;
        public const int LUA_HOOKTAILCALL = 4;

        public const int LUA_MASKCALL = (1 << LUA_HOOKCALL);
        public const int LUA_MASKRET = (1 << LUA_HOOKRET);
        public const int LUA_MASKLINE = (1 << LUA_HOOKLINE);
        public const int LUA_MASKCOUNT = (1 << LUA_HOOKCOUNT);

        public static byte[] ManagedBuffer;
        public static IntPtr NativeBuffer = IntPtr.Zero;
        public const int NativeBufferSize = 0x100000;

        public enum VarType
        {
            E_Int,
            E_Float,
            E_Double,
            E_Long,
            E_Bool,
            E_String,
            E_Object,
        }

        
        

#if UNITY_IPHONE
		private const String DllName = "__Internal";
#else
        private const String DllName = "libCore";
#endif


        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_atpanic(IntPtr L, lua_CFunction panicf);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_checkstack(IntPtr L, int sz);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_close(IntPtr L);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_concat(IntPtr L, int n);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_createtable(IntPtr L, int narr, int nrec);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_dump(IntPtr L, lua_Writer writer, IntPtr data);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_error(IntPtr L);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_gc(IntPtr L, int what, int data);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern lua_Alloc lua_getallocf(IntPtr L, IntPtr ud);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_getfield(IntPtr L, int idx, string k);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_getmetatable(IntPtr L, int objindex);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_gettable(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_gettop(IntPtr L);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_insert(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_iscfunction(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_isnumber(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_isstring(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_isuserdata(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_load(IntPtr L, lua_Reader reader, IntPtr dt, IntPtr chunkname, IntPtr mode);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr lua_newstate(lua_Alloc f, IntPtr ud);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr lua_newthread(IntPtr L);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr lua_newuserdata(IntPtr L, int sz);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_next(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_pcall(IntPtr L, int nargs, int nresults, int errfunc);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_pushboolean(IntPtr L, int b);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushID(IntPtr L, long b);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushcclosure(IntPtr L, lua_CFunction fn, int n);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_sethook(IntPtr L, lua_Hook fn, int mask, int count);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_RMDB(IntPtr L,int is_block);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_IdleDB(IntPtr L);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaclose_RLdb(IntPtr L);

#if UNITY_IPHONE
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
		public static extern void luaL_pushinteger32(IntPtr L, int n);
#else 
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushinteger(IntPtr L, int n);
#endif
        public static void luaL_pushinteger(IntPtr L, int n)
        {        
#if UNITY_IPHONE
            luaL_pushinteger32(L, n);
#else
            lua_pushinteger(L, n);
#endif
        }
       
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_pushlightuserdata(IntPtr L, IntPtr p);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
#if UNITY_IPHONE
        public static extern void luaL_pushlstring32(IntPtr L, IntPtr s, int l);
#else
		public static extern void lua_pushlstring(IntPtr L, IntPtr s, int l);
#endif
        public static void luaL_pushlstring(IntPtr L, IntPtr s, int l)
        {
#if UNITY_IPHONE
            luaL_pushlstring32(L, s, l);
#else
            lua_pushlstring(L, s, l);
#endif
        }

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_pushnil(IntPtr L);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_pushnumber(IntPtr L, double n);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_pushstring(IntPtr L, string s);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_pushthread(IntPtr L);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_pushvalue(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushlong(IntPtr L, long longValue);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_rawequal(IntPtr L, int idx1, int idx2);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_rawget(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_rawgeti(IntPtr L, int idx, int n);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_objlen(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_rawset(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_rawseti(IntPtr L, int idx, int n);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_remove(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_replace(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_resume(IntPtr L, IntPtr from, int narg);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_setallocf(IntPtr L, lua_Alloc f, IntPtr ud);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_setfield(IntPtr L, int idx, string k);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_setmetatable(IntPtr L, int objindex);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_settable(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_settop(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_status(IntPtr L);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_toboolean(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern lua_CFunction lua_tocfunction(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern long lua_tointeger(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
#if UNITY_IPHONE
        public static extern IntPtr luaL_tolstring32(IntPtr L, int idx, ref int len);
#else
        public static extern IntPtr lua_tolstring(IntPtr L, int idx, ref int len);	
#endif
        public static IntPtr luaL_tolstring(IntPtr L, int idx, ref int len)
        {
#if UNITY_IPHONE
            return luaL_tolstring32(L, idx, ref len);
#else
            return lua_tolstring(L, idx, ref len);       
#endif
        }
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern long lua_toID(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern double lua_tonumber(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr lua_topointer(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr lua_tothread(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr lua_touserdata(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_type(IntPtr L, int idx);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr lua_typename(IntPtr L, int tp);
        public static int lua_upvalueindex(int i)
        {
            return (-1001000 - i);
        }

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void lua_xmove(IntPtr from, IntPtr to, int n);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int luaL_argerror(IntPtr L, int numarg, IntPtr extramsg);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int luaL_callmeta(IntPtr L, int obj, IntPtr e);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void luaL_checkany(IntPtr L, int narg);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern long luaL_checkinteger(IntPtr L, int numArg);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr luaL_checklstring(IntPtr L, int numArg, ref int l);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern double luaL_checknumber(IntPtr L, int numArg);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void luaL_checkstack(IntPtr L, int sz, IntPtr msg);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void luaL_checktype(IntPtr L, int narg, int t);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr luaL_checkudata(IntPtr L, int ud, IntPtr tname);
 

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int luaL_getmetafield(IntPtr L, int obj, IntPtr e);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr luaL_gsub(IntPtr L, IntPtr s, IntPtr p, IntPtr r);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int luaL_loadfile(IntPtr L, string filename);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_loadpak(IntPtr L, string filename);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int luaL_loadstring(IntPtr L, string s);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ll_require(IntPtr L);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int luaL_newmetatable(IntPtr L, string tname);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr luaL_newstate();
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void luaL_openlibs(IntPtr L);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern long luaL_optinteger(IntPtr L, int nArg, long def);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern IntPtr luaL_optlstring(IntPtr L, int numArg, IntPtr def, ref int l);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern double luaL_optnumber(IntPtr L, int nArg, double def);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int luaL_ref(IntPtr L, int t);


        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void luaL_unref(IntPtr L, int t, int refIndex);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern void luaL_where(IntPtr L, int lvl);
        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lua_geterror_info(IntPtr L);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int push_sharp_userdata(IntPtr L,int index);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void push_sharp_new_userdata(IntPtr L,int index);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int to_sharp_userdata(IntPtr L,int stack);


#if !UNITY_IPHONE
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_Hook(IntPtr L, IntPtr debug);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr lua_Alloc(IntPtr ud, IntPtr ptr, int osize, int nsize);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_CFunction(IntPtr L);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate sbyte[] lua_Reader(IntPtr L, IntPtr ud, ref int sz);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_Writer(IntPtr L, sbyte[] p, int sz, IntPtr ud);
#else
        public delegate int lua_Hook(IntPtr L, IntPtr debug);
		public delegate IntPtr lua_Alloc(IntPtr ud, IntPtr ptr, int osize, int nsize);
		public delegate int lua_CFunction(IntPtr L);
		public delegate sbyte[] lua_Reader(IntPtr L, IntPtr ud, ref int sz);
		public delegate int lua_Writer(IntPtr L, sbyte[] p, int sz, IntPtr ud);
#endif

        public static LuaState NewState(IntPtr pWarpLua)
        {
            if (NativeBuffer == IntPtr.Zero)
            {
                NativeBuffer = Marshal.AllocHGlobal(0x100000);
                ManagedBuffer = new byte[0x100000];
            }
            if (pWarpLua == IntPtr.Zero)
            {
                return new LuaState(LuaState.Create());
            }
            return new LuaState(pWarpLua);
        }

        public static string StringFromNativeUtf8(IntPtr nativeUtf8, int len)
        {
            if (nativeUtf8 == IntPtr.Zero)
            {
                return string.Empty;
            }
            if (len <= 0)
            {
                len = 0;
                while (Marshal.ReadByte(nativeUtf8, len) != 0)
                {
                    len++;
                }
            }
            Marshal.Copy(nativeUtf8, ManagedBuffer, 0, len);
            return Encoding.UTF8.GetString(ManagedBuffer, 0, len);
        }
    }

}

