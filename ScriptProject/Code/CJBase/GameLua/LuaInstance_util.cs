#if UNITY_IPHONE
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniLua;
using System.Runtime.InteropServices;
using System.Reflection;
using System;
using System.Text;
using SimpleProtobuf;
using System.Linq.Expressions;
using System.Linq;

public class LuaClass
{

    Type register_type_ = null;

    int register_funtion_ = 0;
    int classIndex = 0;
    LuaState lua_ = null;
    public List<MethodInfo> methods_ = new List<MethodInfo>();
    public List<ParameterInfo[]> paramters_ = new List<ParameterInfo[]>();
    public int type_ref_ = 0;

    public bool is_register
    {
        get
        {
            return type_ref_ != 0;
        }
    }

    public LuaClass(LuaState lua, Type type, int index)
    {
        classIndex = index;
        lua_ = lua;
        BeginRegisterClass(type);
    }

    void BeginRegisterClass(Type type)
    {
        if (register_type_ != null)
            return;
        lua_.NewTable();
        lua_.PushValue(-1);
        lua_.SetField(1, "__index");

        register_type_ = type;
    }

    public void RegisterFunction(FuncInfo funcInfos)
    {
        MethodInfo m = null;
        if (funcInfos.funcParamTypes == null)
        {
            m = register_type_.GetMethod(funcInfos.funcName);
        }
        else
        {
            m = register_type_.GetMethod(funcInfos.funcName, funcInfos.funcParamTypes);
        }

        RegisterFuncByMethodInfo(m, new LuaAPI.lua_CFunction(LuaInstance.Callback));
    }

    public void RegisterStaticFunction(FuncInfo funcInfos)
    {
        MethodInfo m = null;
        if (funcInfos.funcParamTypes == null)
        {
            m = register_type_.GetMethod(funcInfos.funcName);
        }
        else
        {
            m = register_type_.GetMethod(funcInfos.funcName, funcInfos.funcParamTypes);
        }

        RegisterFuncByMethodInfo(m, new LuaAPI.lua_CFunction(LuaInstance.CallbackStatic));
    }

    public void RegisterFuncByMethodInfo(MethodInfo m, LuaAPI.lua_CFunction cfunc)
    {
        if (m == null)
            return; // FIX add warnning
        else
        {
            methods_.Add(m);
            paramters_.Add(m.GetParameters());

            int a = LuaInstance.MergeInt(register_funtion_, classIndex);
            lua_.PushInteger(a);
            LuaAPI.lua_pushcclosure(lua_.GetLuaPtr(), cfunc, 1);
            lua_.SetField(1, m.Name);

            register_funtion_++;
        }

    }

    public void RegisterProperty(string name)
    {

    }

    public void EndRegisterClass()
    {
        if (type_ref_ == 0)
        {
            lua_.PushValue(-1);
            type_ref_ = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
            lua_.SetGlobal(register_type_.Name);
        }
    }
    public GCHandle RegisterObject(object o, string name)
    {
        GCHandle handle = lua_.NewUserData(o);
        //lua_.GetGlobal(register_type_.Name); // FIX add warnning
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, type_ref_);
        lua_.SetMetaTable(1);
        lua_.SetGlobal(name);

        return handle;
    }

    public GCHandle RegisterField(object o, string name)
    {
        // must push a table first
        GCHandle handle = lua_.NewUserData(o);
        //lua_.GetGlobal(register_type_.Name); // FIX add warnning
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, type_ref_);
        lua_.SetMetaTable(-2);
        lua_.SetField(-2, name);

        return handle;
    }
}

public delegate int LuaCallCSharpDelegate(IntPtr l);

public class LuaInstance
{
    private static LuaInstance instance_;
    LuaState lua_;

    public static object[][] params_ = new object[10][];

    // CSharp class
    Dictionary<Type, LuaClass> classesDic_ = new Dictionary<Type, LuaClass>();
    public List<LuaClass> classesList = new List<LuaClass>();
    public int regiserClass = 0;

    //lua files
    HashSet<string> code_ = new HashSet<string>();

    public static LuaInstance instance
    {
        get
        {
            if (instance_ == null)
            {
                instance_ = new LuaInstance();
            }
            return instance_;
        }
    }

    LuaInstance()
    {
        lua_ = LuaAPI.NewState(IntPtr.Zero);
        lua_.L_OpenLibs();
        LuaClassList.Init();

        for (int i = 0; i < 10; i++)
            params_[i] = new object[i];

        //Register Debug.Log
        Type debugClassType = typeof(Debug);
        LuaClass lua_class = RegisterLuaClass(debugClassType); 
        FuncInfo[] funcInfos;
        if (LuaClassList.classes.TryGetValue(debugClassType, out funcInfos))
        {
            for (int j = 0; j < funcInfos.Length; j++)
                lua_class.RegisterStaticFunction(funcInfos[j]);
        }
        lua_class.EndRegisterClass();
    }

    public LuaState Get()
    {
        return lua_;
    }

    public void SetMetaTable()
    {
        lua_.SetMetaTable(1);
    }
    public LuaClass RegisterLuaClass(Type type)
    {
        LuaClass lua_class = null;
        if (classesDic_.TryGetValue(type, out lua_class) == true)
        {
            return lua_class;
        }

        lua_class = new LuaClass(lua_, type, regiserClass);
        regiserClass++;

        classesDic_.Add(type, lua_class);
        classesList.Add(lua_class);
        return lua_class;
    }

    public static string ConstructString(LuaState lua)
    {
        int top = lua.GetTop();
        if (top == 1)
        {
            return lua.ToString(1);
        }
        StringBuilder builder = new StringBuilder();
        for (int i = 1; i <= top; i++)
        {
            switch (lua.Type(i))
            {
                case LuaType.LUA_TNIL:
                    builder.Append("nil");
                    break;

                case LuaType.LUA_TBOOLEAN:
                    builder.Append(lua.ToBoolean(i));
                    break;

                case LuaType.LUA_TNUMBER:
                    builder.Append(lua.ToString(i));
                    break;

                case LuaType.LUA_TSTRING:
                    builder.Append(lua.ToString(i));
                    break;

                case LuaType.LUA_TTABLE:
                    builder.Append("table");
                    break;

                case LuaType.LUA_TFUNCTION:
                    builder.Append(string.Format("function", new object[0]));
                    break;

                default:
                    builder.Append("unknow");
                    break;
            }
            if ((i + 1) <= top)
            {
                builder.Append(",");
            }
        }
        return builder.ToString();
    }

    public void ReadFile(string filename, string code)
    {
        code_.Add(filename);
        lua_.L_DoString(code);
    }

    public bool HasFile(string filename)
    {
        return code_.Contains(filename);
    }
   
    //CallBacks for LuaClass
    static public int Callback(IntPtr l)
    {
        LuaState lua = LuaInstance.instance.lua_;
        int id = lua.ToInteger(lua.UpvalueIndex(1));
        int classIndex = 0;
        int methodIndex = 0;
        SpritInt(id, ref methodIndex, ref classIndex);
       
        LuaClass luaClass = LuaInstance.instance.classesList[classIndex];

        int len = lua.GetTop();
        object[] p = LuaInstance.params_[len - 1];
        ParameterInfo[] ps = luaClass.paramters_[methodIndex];

        SetCallBackParameter(lua, p, ps,2);

        object obj = lua.ToUserDataObject(1);
        object value = luaClass.methods_[methodIndex].Invoke(obj, p);
        SetReturnParameter(lua, value, luaClass.methods_[methodIndex].ReturnParameter);

        return 1;
    }

    //static method have diff way to get their info
    static public int CallbackStatic(IntPtr l)
    {
        LuaState lua = LuaInstance.instance.lua_;
        int id = lua.ToInteger(lua.UpvalueIndex(1));
        int classIndex = 0;
        int methodIndex = 0;
        SpritInt(id, ref methodIndex, ref classIndex);
        LuaClass luaClass = LuaInstance.instance.classesList[classIndex];

        int len = lua.GetTop();
        object[] p = LuaInstance.params_[len];
        ParameterInfo[] ps = luaClass.paramters_[methodIndex];
        SetCallBackParameter(lua, p, ps,1);

        object value = luaClass.methods_[methodIndex].Invoke(null, p);
        SetReturnParameter(lua, value, luaClass.methods_[methodIndex].ReturnParameter);

        return 1;
    }

    static void SetCallBackParameter(LuaState lua,object[] p, ParameterInfo[] ps,int indexInStack)
    {
        LuaType paraType = LuaType.LUA_TNONE;
        for (int i = 0; i < ps.Length; i++)
        {
            int index = indexInStack;
            paraType = lua.Type(index);

            if (ps[i].ParameterType == typeof(object))
            {
                if (paraType == LuaType.LUA_TNUMBER)
                {
                    p[i] = (float)lua.ToNumber(index);
                }
                else if (paraType == LuaType.LUA_TSTRING)
                {
                    p[i] = lua.ToString(index);
                }
                else if (paraType == LuaType.LUA_TBOOLEAN)
                {
                    p[i] = lua.ToBoolean(index);
                }
                else if (paraType == LuaType.LUA_TUSERDATA)
                {
                    p[i] = lua.ToUserDataObject(index);
                }
            }
            else
            {
                if (paraType == LuaType.LUA_TNUMBER)
                {
                    if (ps[i].ParameterType == typeof(int))
                    {
                        p[i] = lua.ToInteger(index);
                    }
                    else if (ps[i].ParameterType == typeof(float))
                    {
                        p[i] = (float)lua.ToNumber(index);
                    }
                }
                else 
                {
                    if (paraType == LuaType.LUA_TSTRING)
                    {
                        p[i] = lua.ToString(index);
                    }
                    else if (paraType == LuaType.LUA_TBOOLEAN)
                    {
                        p[i] = lua.ToBoolean(index);
                    }
                    else if (paraType == LuaType.LUA_TUSERDATA)
                    {
                        p[i] = lua.ToUserDataObject(index);
                    }
                }
            }
        }
    }

    static void SetReturnParameter(LuaState lua, object p, ParameterInfo ps)
    {
        if (p == null)
            return;

        if (ps.ParameterType == typeof(int))
        {
            lua.PushInteger((int)p);
        }
        else if (ps.ParameterType == typeof(long))
        {
            lua.PushLongInterger((long)p);
        }
        else if (ps.ParameterType == typeof(float))
        {
            lua.PushNumber((float)p);
        }
        else if (ps.ParameterType == typeof(bool))
        {
            lua.PushBoolean((bool)p);
        }
        else if (ps.ParameterType == typeof(string))
        {
            lua.PushString((string)p);
        }
        else
        {
            LuaClass lua_class = null;
            if (LuaInstance.instance.classesDic_.TryGetValue(ps.ParameterType, out lua_class) == true)
            {
                lua.NewUserData(p);
                lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, lua_class.type_ref_);
                lua.SetMetaTable(-2);
            }
            else
            {
                Debug.Log("-------No Class Found In Lua classesDic--------");
            }
        }

    }

    public static int MergeInt(int first, int second)
    {
        return ((first << 16) + second);
    }

    public static void SpritInt(int num,ref int reslut1,ref int reslut2)
    {
        reslut1 = num >> 16;
        reslut2 = num & 0xffff;
    }

}

class RegisterFactory
{
    public static Delegate MagicMethod<T>(MethodInfo method)
    {
        var parameter = method.GetParameters().Single();
        var instance = Expression.Parameter(typeof(T),"a");
        var methodCall = Expression.Call(
            instance,
            method
            );
        return Expression.Lambda<Func<T, object>>(
            Expression.Convert(methodCall, typeof(object)),
            instance
            ).Compile();
    }
    public static Delegate MagicMethod1<T>(MethodInfo method)
    {
        var parameter = method.GetParameters().Single();
        var instance = Expression.Parameter(typeof(T),"a1");
        var argument = Expression.Parameter(typeof(object),"a2");
        var methodCall = Expression.Call(
            instance,
            method,
            Expression.Convert(argument, parameter.ParameterType)
            );
        return Expression.Lambda<Func<T, object, object>>(
            Expression.Convert(methodCall, typeof(object)),
            instance, argument
            ).Compile();
    }

}
#endif