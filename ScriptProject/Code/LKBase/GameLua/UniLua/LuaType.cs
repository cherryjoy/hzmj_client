namespace UniLua
{
    using System;

    public enum LuaType
    {
        LUA_TNONE = -1,
        LUA_TNIL = 0,
        LUA_TBOOLEAN = 1,
        LUA_TLIGHTUSERDATA = 2,
        LUA_TNUMBER = 3,
        LUA_TID = 4,
        LUA_TSTRING = 5,
        LUA_TTABLE = 6,
        LUA_TFUNCTION = 7,
        LUA_TUSERDATA = 8,
        LUA_TTHREAD = 9,
        LAST_TAG = LUA_TTHREAD,
        LUA_NUMTAGS = LAST_TAG + 1,
        LUA_TPROTO = LAST_TAG + 1,
        LUA_TUPVAL = LAST_TAG + 2,
        LUA_TDEADKEY = LAST_TAG + 3,
        LUA_TUINT64 = LAST_TAG + 4,
    }

}

