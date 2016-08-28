using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniLua;
using System.Runtime.InteropServices;
using System.Reflection;
using System;
using System.IO;
using System.Text;
using System.Linq.Expressions;
using System.Linq;

public struct FuncInfo
{
    public string funcName;
    public string orginalName;
    public Type[] funcParamTypes;
    public bool IsStatic;

    public FuncInfo(string name, string orgiName, Type[] types, bool isStatic)
    {
        funcName = name;
        orginalName = orgiName;
        funcParamTypes = types;
        IsStatic = isStatic;
    }
};

public class LuaInstance
{
    private static LuaInstance instance_;
    LuaState lua_;

    public static object[][] params_ = new object[10][];

    public int text_table_ref = LuaAPI.LUA_REFNIL;
    public int table_table_ref = LuaAPI.LUA_REFNIL;
    public int client_table_ref = LuaAPI.LUA_REFNIL;
    // CSharp class
    public Dictionary<Type, LuaClass> classesDic_ = new Dictionary<Type, LuaClass>();
    public List<LuaClass> classesList = new List<LuaClass>();
    public int regiserClass = 0;

    //lua files
    HashSet<string> code_ = new HashSet<string>();

    private string luaRootPath;
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
        for (int i = 0; i < 10; i++)
            params_[i] = new object[i];
        LuaClassList.Init();
        PrepareLuaState();
    }

    public void PrepareLuaState()
    {
        lua_ = LuaAPI.NewState(IntPtr.Zero);
        lua_.L_OpenLibs();
#if UNITY_EDITOR|| LuaDebugger
        LuaAPI.luaopen_RMDB(lua_.GetLuaPtr(), 0);
#endif
        //Register Global Class
        LuaDebug.RegisterToLua(lua_, typeof(LuaDebug));
        RegisterType(typeof(Register));
        LuaDBTable.RegisterToLua(lua_, typeof(LuaDBTable));
    }

    public void Reset()
    {
        classesDic_ = new Dictionary<Type, LuaClass>();
        classesList = new List<LuaClass>();
        regiserClass = 0;
        code_ = new HashSet<string>();
        PrepareLuaState();
        Init();
        TimerManager.Instance.Init();
    }

    public void Init(bool isUpdate=false)
    {
#if UNITY_ANDROID || UNITY_IPHONE
        instance_.luaRootPath = PluginTool.SharedInstance().PersisitentDataPath + "Script/?.txt";
#elif UNITY_EDITOR||UNITY_STANDALONE 
        instance_.luaRootPath = Application.streamingAssetsPath + "/Script/?.txt";
#endif

        LuaState lua = instance_.Get();
        lua.GetGlobal("package");
        lua.PushString(instance_.luaRootPath);
        lua.SetField(-2, "path");
        lua.Pop(1);

#if UNITY_ANDROID || UNITY_IPHONE
        lua.PushBoolean(false);
        lua.SetGlobal("IS_PLATFORM_DEBUG");
#elif UNITY_EDITOR||UNITY_STANDALONE
        lua.PushBoolean(true);
        lua.SetGlobal("IS_PLATFORM_DEBUG");
#endif

        if (isUpdate)
        {
            instance_.DoFile("UpdateUI/UpdateRegisterClass.txt");
        }
        else
        {
            instance_.DoFile("Utils/RegisterClass.txt");
        }
#if UNITY_EDITOR||UNITY_STANDALONE
        Register.PrintLuaClassList();
#endif
        //cache most use table ref
        lua.GetGlobal("Text");
        text_table_ref = lua.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
        lua.GetGlobal("table");
        table_table_ref = lua.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
        lua.GetGlobal("Client");
        client_table_ref = lua.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
    }

    public void RegisterType(Type type)
    {
        LuaClass lua_class = RegisterLuaClass(type);
        if (lua_class.is_register == false)
        {
            FuncInfo[] funcInfos;
            if (LuaClassList.classes.TryGetValue(type, out funcInfos))
            {
                for (int j = 0; j < funcInfos.Length; j++)
                {
                    if (funcInfos[j].IsStatic)
                    {
                        lua_class.RegisterStaticFunction(funcInfos[j]);
                    }
                    else
                    {
                        lua_class.RegisterFunction(funcInfos[j]);
                    }
                }
            }
            lua_class.EndRegisterClass();
        }
    }

    public void RegisterFunc(string funcName, LuaAPI.lua_CFunction func, int paraNum)
    {
        lua_.PushLuaClosure(func, paraNum);
        lua_.SetGlobal(funcName);
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
        //         if (top == 1)
        //         {
        //             return lua.ToString(1);
        //         }
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
                    double n = lua.ToNumber(i);
                    builder.Append(n.ToString());
                    break;

                case LuaType.LUA_TSTRING:
                    builder.Append("\n"+lua.ToString(i));
                    break;

                case LuaType.LUA_TTABLE:
                    builder.Append("table");
                    break;

                case LuaType.LUA_TFUNCTION:
                    builder.Append(string.Format("function", new object[0]));
                    break;
                case LuaType.LUA_TUSERDATA:
                    builder.Append("userdata");
                    break;
                case LuaType.LUA_TID:
                    builder.Append("TID:"+lua.ToString(i));
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


    public void DoFile(string filename,bool checkContain = true)
    {
        if (checkContain)
        {
            if (code_.Contains(filename))
            {
                return;
            }
            code_.Add(filename);
        }

#if UNITY_EDITOR || UNITY_STANDALONE
#if ASSETBUNDLE || LKFILE
        string path = filename.Replace('\\', '/');
#else
        string path = Application.streamingAssetsPath + "/Script/" + filename;
        filename = path.Replace('/', '\\');
#endif
#elif UNITY_ANDROID || UNITY_IPHONE
#if ASSETBUNDLE || LKFILE
        string path = filename.Replace('\\', '/');
#else
        string path = PluginTool.SharedInstance().PersisitentDataPath +"Script/" +filename;
        path = path.Replace('\\', '/');
#endif
#endif
        lua_.L_DoFile(path);
    }

    //CallBacks for LuaClass
    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    static public int SetField(IntPtr l)
    {
        LuaState lua = LuaInstance.instance.lua_;
        int id = lua.ToInteger(lua.UpvalueIndex(1));
        int classIndex = 0;
        int fieldIndex = 0;
        SpritInt(id, ref fieldIndex, ref classIndex);

        LuaClass luaClass = LuaInstance.instance.classesList[classIndex];

        object p = null;
        p = GetFiledValue(lua, luaClass, fieldIndex, p);

        if (p != null)
        {
            try
            {
                if (luaClass.fields_[fieldIndex].IsStatic)
                {
                    luaClass.fields_[fieldIndex].SetValue(null, p);
                }
                else
                {
                    object obj = lua.ToUserDataObject(1);
                    luaClass.fields_[fieldIndex].SetValue(obj, p);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(LogCallBackError(ex));
            }
            return 0;
        }
        else
        {
            PushFieldValueToStack(lua, luaClass, fieldIndex);
            return 1;
        }
    }

    private static object GetFiledValue(LuaState lua, LuaClass luaClass, int fieldIndex, object p)
    {
        int idx = luaClass.fields_[fieldIndex].IsStatic ? 1 : 2;

        LuaType paraType = lua.Type(idx);

        p = GetRightTypeObj(lua, luaClass.fields_[fieldIndex].FieldType, paraType, idx);

        return p;
    }

    private static void PushFieldValueToStack(LuaState lua, LuaClass luaClass, int fieldIndex)
    {
        object returnObj = null;

        object obj = null;
        if (!luaClass.fields_[fieldIndex].IsStatic)
        {
            obj = lua.ToUserDataObject(1);
        }

        try
        {
            returnObj = luaClass.fields_[fieldIndex].GetValue(obj);
        }
        catch (Exception ex)
        {
            Debug.LogError(LogCallBackError(ex));
        }

        if (returnObj == null || returnObj.Equals(null))
        {
            lua.PushNil();
        }
        else if (!PushBaseTypeObj(lua, luaClass.fields_[fieldIndex].FieldType, returnObj))
        {
            //Debug.Log(returnObj + returnObj.GetType().Name);
            lua.NewClassUserData(returnObj);
        }

    }

    //CallBacks for LuaClass
    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    static public int Callback(IntPtr l)
    {
        LuaState lua = LuaInstance.instance.lua_;
        object obj = lua.ToUserDataObject(1);
        int returnParNum = DoCallBack(lua, obj, 2);

        return returnParNum;
    }

    //static method have diff way to get their info
    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    static public int CallbackStatic(IntPtr l)
    {
        LuaState lua = LuaInstance.instance.lua_;
        object obj = null;
        int returnParNum = DoCallBack(lua, obj, 1);

        return returnParNum;
    }

    static int DoCallBack(LuaState lua, object obj, int indexInStack)
    {
        int id = lua.ToInteger(lua.UpvalueIndex(1));
        int classIndex = 0;
        int methodIndex = 0;
        SpritInt(id, ref methodIndex, ref classIndex);

        LuaClass luaClass = LuaInstance.instance.classesList[classIndex];

        ParameterInfo[] ps = luaClass.paramters_[methodIndex];
        object[] p = LuaInstance.params_[luaClass.paramters_[methodIndex].Length];
        if (ps.Length > 0)
        {
            SetCallBackParameter(lua, p, ps, indexInStack);
        }

        object value = null;
     
        try
        {
           value = luaClass.methods_[methodIndex].Invoke(obj, p);
           //Debug.Log(LuaInstance.ConstructString(LuaInstance.instance.Get()));
        }
        catch (Exception ex)
        {
            //Debug.Log("FuncName   " + luaClass.methods_[methodIndex].Name);
            //Debug.Log("This   " + obj);
            Debug.LogError(LogCallBackError(ex));
        }

        if (value == null)
        {
            //LuaInstance.ConstructString(lua);
            return 0;
        }
        SetReturnParameter(lua, value, luaClass.methods_[methodIndex].ReturnParameter);
        return 1;
    }



    static void SetCallBackParameter(LuaState lua, object[] p, ParameterInfo[] ps, int indexInStack)
    {
        LuaType paraType = LuaType.LUA_TNONE;
        for (int i = 0; i < ps.Length; i++)
        {
            int index = indexInStack + i;
            paraType = lua.Type(index);
            p[i] = GetRightTypeObj(lua, ps[i].ParameterType, paraType, index);
        }
    }

    public static void SetReturnParameter(LuaState lua, object p, ParameterInfo ps)
    {
        if (p == null || p.Equals(null))
        {
            lua.PushNil();
            return;
        }
        //is base object
        if (!PushBaseTypeObj(lua, p.GetType(), p))
        {
            if (p.GetType() == typeof(LuaObject))
            {
                LuaObject luaObj = (LuaObject)p;
                lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaObj.Index);
            }
            else {   //not base type or base type in object
                lua.NewClassUserData(p);
            }
        }
    }

    //push base type
    public static bool PushBaseTypeObj(LuaState lua, Type type, object p)
    {
        bool pushSuccess = true;
        if (type == typeof(object))
        {
            type = p.GetType();
        }
        if (type == typeof(int) || type.IsEnum)
        {
            lua.PushInteger((int)p);
        }
        else if (type == typeof(long))
        {
            lua.PushLongId((long)p);
        }
        else if (type == typeof(float))
        {
            lua.PushNumber((float)p);
        }
        else if (type == typeof(double))
        {
            lua.PushNumber((double)p);
        }
        else if (type == typeof(bool))
        {
            lua.PushBoolean((bool)p);
        }
        else if (type == typeof(string))
        {
            lua.PushString((string)p);
        }
        else
        {
            return !pushSuccess;
        }

        return pushSuccess;

    }

    static object GetRightTypeObj(LuaState lua, Type funcParaType, LuaType luaParaType, int index)
    {
        object rightObj = null;
        if (funcParaType == typeof(object))
        {
            if (luaParaType == LuaType.LUA_TNUMBER)
            {
                rightObj = lua.ToNumber(index);
            }
            else if (luaParaType == LuaType.LUA_TSTRING)
            {
                rightObj = lua.ToString(index);
            }
            else if (luaParaType == LuaType.LUA_TBOOLEAN)
            {
                rightObj = lua.ToBoolean(index);
            }
            else if (luaParaType == LuaType.LUA_TUSERDATA)
            {
                rightObj = lua.ToUserDataObject(index);
            }
        }
        else if (funcParaType == typeof(long))
        {
            if (luaParaType == LuaType.LUA_TUSERDATA)
            {
                rightObj = lua.ReadLongFromUnManaged(index);
            }
            else if (luaParaType == LuaType.LUA_TID)
            {
                rightObj = lua.ReadLongId(index);
            }
        }
        else
        {
            if (luaParaType == LuaType.LUA_TNUMBER)
            {
                if (funcParaType == typeof(int) || funcParaType.IsEnum)
                {
                    rightObj = lua.ToInteger(index);
                }
                else if (funcParaType == typeof(float))
                {
                    rightObj = (float)lua.ToNumber(index);
                }
                else if (funcParaType == typeof(double))
                {
                    rightObj = lua.ToNumber(index);
                }
            }
            else
            {
                if (luaParaType == LuaType.LUA_TSTRING)
                {
                    rightObj = lua.ToString(index);
                }
                else if (luaParaType == LuaType.LUA_TBOOLEAN)
                {
                    rightObj = lua.ToBoolean(index);
                }
                else if (luaParaType == LuaType.LUA_TUSERDATA)
                {
                    rightObj = lua.ToUserDataObject(index);
                }
            }
        }

        return rightObj;
    }

    public static int MergeInt(int first, int second)
    {
        return ((first << 16) + second);
    }

    public static void SpritInt(int num, ref int reslut1, ref int reslut2)
    {
        reslut1 = num >> 16;
        reslut2 = num & 0xffff;
    }

    public static string LogCallBackError(Exception ex)
    {
        string luaError = "";

        luaError = LuaInstance.instance.Get().ErrorInfo();

        string errorString = "luaError: " + luaError + Environment.NewLine; 
        if (ex.InnerException != null && ex.InnerException.Message != "")
        {
            errorString += ex.InnerException.Message + Environment.NewLine;
            errorString += ex.InnerException.StackTrace + Environment.NewLine;
        }
        else
        {
            errorString += " " + ex.Message + Environment.NewLine;
        }

        return errorString;
    }

    public void RegisterGameObject(object o, string name)
    {
        lua_.NewClassUserData(o);
        lua_.SetField(-2, name);

        lua_.Pop(1);
    }

    public void RegisterGameObject(object o, int Object_ref)
    {
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, LuaInstance.instance.table_table_ref);
        lua_.GetField(-1, "insert");
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Object_ref);
        lua_.NewClassUserData(o);
        lua_.PCall(2, 0, 0);
       
    }
}
