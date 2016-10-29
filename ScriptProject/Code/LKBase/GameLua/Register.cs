using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using UniLua;
using UnityEditor;

class Register
{
    public static void RegisterClass(string className)
    {
		Debug.Log("className: " + className);
        Type type = GetGameClassType(className);

        LuaInstance.instance.Get().Pop(1);
        AddFuncInfo(type, className);        
        LuaClass lua_class = LuaInstance.instance.RegisterLuaClass(type);
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

    public static Type GetGameClassType(string className) {
        Type type = Types.GetType(className, "UnityEngine");
        if (type == null)
        {
            type = Types.GetType(className, "Main");
        }
        if (type == null)
        {
            type = Types.GetType(className, "LKBase");
        }
        if (type == null)
        {
            type = Type.GetType(className);
        }

        return type;
    }

    public static void RegisterWrapper(bool isUpdate)
    {
		Debug.Log("RegisterWrapper begin");
        LuaState lua_ = LuaInstance.instance.Get();

        if (isUpdate)
        {
            LuaNTools.RegisterToLua(lua_, typeof(LuaNTools));
        }
        else
        {
            LuaVector3.RegisterToLua(lua_, typeof(LuaVector3));
            LuaQuaternion.RegisterToLua(lua_, typeof(LuaQuaternion));
            LuaColor.RegisterToLua(lua_, typeof(LuaColor));
            LuaNTools.RegisterToLua(lua_, typeof(LuaNTools));
            LuaNetwork.RegisterToLua(lua_, typeof(LuaNetwork));
            LuaLong.RegisterToLua(lua_, typeof(LuaLong));
            LuaDateTime.RegisterToLua(lua_, typeof(LuaDateTime));
            LuaUtil.RegisterToLua(lua_, typeof(LuaUtil));
            LuaClassFactory.RegisterToLua(lua_, typeof(LuaClassFactory));
        }
     	
		Debug.Log("RegisterWrapper end");
    }

    public static void AddFuncInfo(Type type,string name = " kong")
    {
        if (type == null)
        {
            Debug.Log(name + " is null");
        }
        if (LuaClassList.classes.ContainsKey(type))
        {
            return;
        }

        MethodInfo[] methods_ = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static
    | BindingFlags.FlattenHierarchy);
        List<FuncInfo> funcInfos_ = new List<FuncInfo>();
        for (int i = 0; i < methods_.Length; i++)
        {
            if (methods_[i].IsGenericMethod)
                continue;
            
            ParameterInfo[] parameterInfos_ = methods_[i].GetParameters();
            Type[] types_ = new Type[parameterInfos_.Length];
            for (int j = 0; j < parameterInfos_.Length; j++)
            {
                types_[j] = parameterInfos_[j].ParameterType;
            }

            funcInfos_.Add(new FuncInfo("", methods_[i].Name, types_, methods_[i].IsStatic));
        }

        FuncInfo[] rightFuncInfos = funcInfos_.ToArray();

        RenameOverLoadFunc(rightFuncInfos);

        LuaClassList.classes.Add(type, rightFuncInfos); 
        
    }

    private static void RenameOverLoadFunc(FuncInfo[] funcInfos_)
    {
        var dic = new Dictionary<string, int>();
        for (var i = 0; i < funcInfos_.Length; i++)
        {
            if (!dic.ContainsKey(funcInfos_[i].orginalName))
            {
                dic.Add(funcInfos_[i].orginalName, 1);
                funcInfos_[i].funcName = funcInfos_[i].orginalName;
            }
            else
            {
                var num = dic[funcInfos_[i].orginalName]++;
                funcInfos_[i].funcName = funcInfos_[i].orginalName + num;
            }
        }
    }

    public static void PrintLuaClassList()
    {
        FileStream funcInfoFile = new FileStream(Application.dataPath+"\\FuncInfoFile.txt",
            FileMode.OpenOrCreate,FileAccess.ReadWrite);
        StreamWriter sw = new StreamWriter(funcInfoFile, Encoding.UTF8);
        StringBuilder sb = new StringBuilder();
        Type[] types = new Type[LuaClassList.classes.Keys.Count];
        int i = 0;
        foreach(var key in LuaClassList.classes.Keys)
        {
            types[i++] = key;
        }
        for (var j = 0; j < types.Length; j++)
        {
            sb.AppendLine("================================");
            sb.AppendLine(types[j].ToString());
            sb.AppendLine();
            FuncInfo[] funcInfos;
            LuaClassList.classes.TryGetValue(types[j], out funcInfos);
            
            foreach(var funcInfo in funcInfos)
            {
                sb.AppendLine("Class:   " + types[j]);   
                if(funcInfo.IsStatic)
                {
                    sb.AppendLine("FuncName:  Static  " + funcInfo.funcName);
                }
                else
                {
                    sb.AppendLine("FuncName:   " + funcInfo.funcName);
                }
                string paramTypes = "";
                foreach(var paramType in funcInfo.funcParamTypes)
                {
                    paramTypes = paramTypes + "   " + paramType.ToString();
                }
                if (paramTypes == "")
                {
                    paramTypes = "NULL";
                }
                sb.AppendLine("ParamType:  " + paramTypes);
                sb.AppendLine();
                sb.AppendLine();
            }
        }
        sw.Write(sb);
        sw.Flush();
        sw.Close();
        funcInfoFile.Close();
    }
}

