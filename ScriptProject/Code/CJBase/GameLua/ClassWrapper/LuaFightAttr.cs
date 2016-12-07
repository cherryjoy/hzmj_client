using System;
using System.Collections.Generic;
using UnityEngine;
using UniLua;

class LuaFightAttr
{
	static LuaState lua_;
	public static void RegisterToLua(LuaState lua, Type type)
	{
		lua_ = LuaInstance.instance.Get();
		string[] funcList = new string[]
        {
             "logtest",
        };

		LuaAPI.lua_CFunction[] funcDeList = new LuaAPI.lua_CFunction[]
        {
             logtest,
        };
		LuaWrapper.RegisterToLua(lua, type, funcList, funcDeList);
	}

	[MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
	public static int logtest(/*参数类型IntPtr?*/IntPtr l)
	{
		int[] aaa = lua_.ToIntArray(-1);

		LuaDataCenter.Instance.PlayerAttr_ = aaa;

		//LuaInstance.instance.Get().RawGetI(-1, 1);
		//array[i] = (int)ToInteger(-1);
		//LuaInstance.instance.Get().Pop(1);


		//LuaInstance.instance.Get().GetField(-1, 1); // get function name obj_function

		//var x = LuaInstance.instance.Get().ToNumber(1);
		//var y = LuaInstance.instance.Get().ToNumber(2);
		//var z = LuaInstance.instance.Get().ToNumber(3);


		//解析表param

		return 1;
	}
}
