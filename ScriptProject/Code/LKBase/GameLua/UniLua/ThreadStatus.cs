namespace UniLua
{
    using System;

    public enum ThreadStatus
    {
        LUA_ERRERR = 6,
        LUA_ERRFILE = 7,
        LUA_ERRGCMM = 5,
        LUA_ERRMEM = 4,
        LUA_ERRRUN = 2,
        LUA_ERRSYNTAX = 3,
        LUA_OK = 0,
        LUA_RESUME_ERROR = -1,
        LUA_YIELD = 1
    }
}

