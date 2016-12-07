namespace UniLua
{
    using System;
    using System.IO;

    public static class LuaConf
    {
        public const string LUA_SIGNATURE = "\x001bLua";
        public const int LUAI_BITSINT = 0x20;
        public const int LUAI_FIRSTPSEUDOIDX = -1001000;
        public const int LUAI_MAXSTACK = 0xf4240;

        public static string LUA_DIRSEP
        {
            get
            {
                return Path.DirectorySeparatorChar.ToString();
            }
        }
    }
}

