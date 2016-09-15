using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.IO;
using UnityEditor;

public class DBScriptGenratorEditor : EditorWindow
{
	private string src_db_file;
	private string wdb_out_path = "ScriptProject/Code/Scripts/DataBase/WDB/";
    private bool wdbSelect = true;
	private string model_out_path = "ScriptProject/Code/Scripts/DataBase/Model/";
    private bool modelSelect = true;
    private bool coverModelFile = false;

    private string lua_wdb_out_path = "Assets/StreamingAssets/Script/WDB/";
    private string[] options = { "Lua", "C#" };
    private int index = 0;

	[MenuItem("CJ-TOOL/db script auto genrator")]
	public static void Init()
	{
		DBScriptGenratorEditor window = EditorWindow.GetWindow<DBScriptGenratorEditor>();
		window.position = new Rect(100, 100, 500, 400);
		window.Show();
	}

	void OnGUI()
	{
        index = EditorGUI.Popup(new Rect(3, 25, position.width - 6, 20), "TYPE", index, options);
        if (index == 0)
        {
            src_db_file = EditorGUI.TextField(new Rect(3, 50, position.width - 6, 20), "策划表名：", src_db_file);
            lua_wdb_out_path = EditorGUI.TextField(new Rect(3, 75, position.width - 6, 20), "wdb输出路径：", lua_wdb_out_path);

            if (GUILayout.Button("Genrate"))
            {
                WDBData data = CDataMgr.Instance.GetOrCreateDB(src_db_file);
                if (data == null)
                {
                    Debug.LogError("Can't find " + src_db_file + "!!");
                    return;
                }

                LuaDBScriptGenrator.GenrateWDBScript(data, src_db_file, lua_wdb_out_path);
                LuaDBScriptGenrator.RegisterDBinCDataMgr(src_db_file);
                //LuaDBScriptGenrator.RegisterWDBDataInCSharp(src_db_file);
            }
        }
        else 
        {
            src_db_file = EditorGUI.TextField(new Rect(3, 50, position.width - 6, 20), "策划表名：", src_db_file);
            wdb_out_path = EditorGUI.TextField(new Rect(3, 75, position.width - 6, 20), "wdb输出路径：", wdb_out_path);
            wdbSelect = EditorGUI.Toggle(new Rect(3, 100, position.width - 6, 20), "输出wdb ", wdbSelect);
            model_out_path = EditorGUI.TextField(new Rect(3, 125, position.width - 6, 20), "model输出路径：", model_out_path);
            modelSelect = EditorGUI.Toggle(new Rect(3, 150, position.width - 6, 20), "输出model ", modelSelect);

            coverModelFile = EditorGUI.Toggle(new Rect(3, 200, position.width - 6, 20), "覆盖掉之前的Model文件 ", coverModelFile);

            if (GUILayout.Button("Genrate"))
            {
                WDBData data = CDataMgr.Instance.GetOrCreateDB(src_db_file);
                if (data == null)
                {
                    Debug.LogError("Can't find " + src_db_file + "!!");
                    return;
                }

                if (wdbSelect)
                {
                    Debug.Log("输出 wdb");
                    DBScriptGenrator.GenrateWDBScript(data, src_db_file, wdb_out_path);
                    DBScriptGenrator.RegisterWDBDataInCSharp(src_db_file);
                }
                if (modelSelect)
                {
                    Debug.Log("输出 model");
                    DBScriptGenrator.GenrateModelScript(data, src_db_file, model_out_path, coverModelFile);
                    DBScriptGenrator.RegisterWDBDataInCSharp(src_db_file);
                }
            }
        }
	}
}

public class DBScriptGenrator
{
	public static void GenrateWDBScript(WDBData data, string script_name, string out_path)
	{
		StreamWriter writer = new StreamWriter(out_path + "WDB_" + script_name + ".cs");
		writer.WriteLine("using UnityEngine;");
		writer.WriteLine("using System.Collections;");
		writer.WriteLine("");
		writer.WriteLine("public class WDB_" + script_name);
		writer.WriteLine("{");
		foreach (KeyValuePair<string, int> kv in data.mFieldName)
		{
			writer.WriteLine("\tpublic static readonly int " + kv.Key + ";");
		}
		writer.WriteLine("");
		writer.WriteLine("\tstatic WDB_" + script_name + "()");
		writer.WriteLine("\t{");
		writer.WriteLine("\t\tWDBData db = CDataMgr." + script_name + ";");
		writer.WriteLine("\t\tif(db != null)");
		writer.WriteLine("\t\t{");
		foreach (KeyValuePair<string, int> kv in data.mFieldName)
		{
			writer.WriteLine("\t\t\tdb.GetFieldByName(\"" + kv.Key + "\", out " + kv.Key + ");");
		}
		writer.WriteLine("\t\t}");
		writer.WriteLine("\t}");
		writer.WriteLine("}");
		writer.Flush();
		writer.Close();
	}

	private static string GetFiledName(string old_name)
	{
		string new_name = string.Empty;
		for (int i = 0; i < old_name.Length; i++ )
		{
			if (old_name[i] >= 'A' && old_name[i] <= 'Z')
			{
				if (i > 0 && old_name[i-1] != '_')
					new_name += '_';
				new_name += (char)(old_name[i] + 32);
			}
			else
				new_name += old_name[i];
		}
		new_name += '_';
		return new_name;
	}

	private static string GetFiledType(int filed_int_type)
	{
		string filed_name = string.Empty;
		switch ((EWDB_FIELD_TYPE)filed_int_type)
		{
			case EWDB_FIELD_TYPE.WFT_INDEX:
			case EWDB_FIELD_TYPE.WFT_INT:
			case EWDB_FIELD_TYPE.WFT_V_INDEX:
				filed_name = "int";
				break;
			case EWDB_FIELD_TYPE.WFT_FLOAT:
				filed_name = "float";
				break;
			case EWDB_FIELD_TYPE.WFT_STRING:
			case EWDB_FIELD_TYPE.WFT_STRINGTABLE:
				filed_name = "string";
				break;
			default:
				filed_name = "UNKONW-TYPE";
				break;
		}
		return filed_name;
	}

	public static void GenrateModelScript(WDBData data, string script_name, string out_path,bool coverFile)
	{
        StreamWriter writer;

        if (coverFile)
            writer = new StreamWriter(out_path + "Model" + script_name + ".cs");
        else
            writer = new StreamWriter(out_path + "Model" + script_name + ".cs.auto");
		
		writer.WriteLine("using UnityEngine;");
		writer.WriteLine("using System.Collections;");

		writer.WriteLine("");
		writer.WriteLine("public class Model" + script_name + " : ModelBase");
		writer.WriteLine("{");
		foreach (KeyValuePair<string, int> kv in data.mFieldName)
		{
			string filed_type = GetFiledType(data.mFieldType[kv.Value]);
			if (kv.Key == "Id")
				continue;
			writer.WriteLine("\tpublic " + filed_type + " " + GetFiledName(kv.Key) + ";");
		}


		writer.WriteLine("");
		writer.WriteLine("\tpublic override bool LoadConfig(int id)");
		writer.WriteLine("\t{");
		writer.WriteLine("\t\tWDBSheetLine line = CDataMgr." + script_name + ".GetData(id);");
		writer.WriteLine("\t\tif(line == null)");
		writer.WriteLine("\t\t{");
		writer.WriteLine("\t\t\tLKDebug.LogError(\"LoadConfig error: can't find \" + id + \" in " + script_name + " table!\");");
		writer.WriteLine("\t\t\treturn false;");
		writer.WriteLine("\t\t}");

		writer.WriteLine("");
		foreach (KeyValuePair<string, int> kv in data.mFieldName)
		{
			string filed_type = GetFiledType(data.mFieldType[kv.Value]);
			writer.WriteLine("\t\t" + GetFiledName(kv.Key) + " = line.GetData<" + filed_type + ">(WDB_" + script_name + "." + kv.Key + ");");
		}

		writer.WriteLine("");
		writer.WriteLine("\t\treturn true;");
		writer.WriteLine("\t}");
		writer.WriteLine("}");
		writer.Flush();
		writer.Close();
	}

    public static void RegisterWDBDataInCSharp(string script_name)
    {
        StreamReader reader = new StreamReader("ScriptProject/Code/LKBase/DataMgr/CDataMgr.cs");
        //小写变量名
        string[] lowNameArray = script_name.Split('_');
        string lowName = "";
        for (int i = 0; i < lowNameArray.Length; i++)
        {
            lowName += (lowNameArray[i].Substring(0, 1).ToLower() + lowNameArray[i].Substring(1) + "_");
        }
        //大写变量名
        string[] upNameArray = script_name.Split('_');
        string upName = "";
        for (int i = 0; i < upNameArray.Length; i++)
        {
            upName += (upNameArray[i].Substring(0, 1).ToUpper() + upNameArray[i].Substring(1));
        }

        string oldText = upName;
        string strLine = reader.ReadLine();
        List<string> wholeText = new List<string>();

        int count = 0;
        while (strLine != null)
        {
            Debug.Log("count: "+ count++);
            wholeText.Add(strLine);
            Debug.Log("oldText: " + oldText);
            Debug.Log("strLine: " + strLine);
            Debug.Log("strLine.IndexOf(oldText): " + strLine.IndexOf(oldText));
            if (strLine.IndexOf(oldText) >= 0)
            {
                Debug.Log("Csharp File Has Already Been Registered!!!");
                return;
            }
            strLine = reader.ReadLine();
        }

        reader.Close();
        reader.Dispose();

        StreamWriter writer = new StreamWriter("ScriptProject/Code/LKBase/DataMgr/CDataMgr.cs");
        for (int i = 0; i < wholeText.Count - 2; i++)
        {
            writer.WriteLine(wholeText[i]);
        }

        writer.WriteLine("\tpublic static WDBData " + upName);
        writer.WriteLine("\t{");
        writer.WriteLine("\t\tget");
        writer.WriteLine("\t\t{");
        writer.WriteLine("\t\t\treturn CDataMgr.instance.GetOrCreateDB(\"" + script_name + "\");");
        writer.WriteLine("\t\t}");
        writer.WriteLine("\t}");
        writer.WriteLine("");
        writer.WriteLine("");
        writer.WriteLine("}");

        writer.Flush();
        writer.Close();
        Debug.Log("RegisterDB in CSharp finished!");
    }
}

public class LuaDBScriptGenrator
{
    public static void GenrateWDBScript(WDBData data, string script_name, string out_path)
    {
        StreamWriter writer = new StreamWriter(out_path + "WDB_" + script_name + ".txt");
        writer.WriteLine("WDB_" + script_name + " = {}");
        writer.WriteLine("--[[");

        foreach (KeyValuePair<string, int> kv in data.mFieldName)
        {
            writer.WriteLine("\tWDB_" + script_name + "." + kv.Key);
        }
        writer.WriteLine("]]--");
        writer.WriteLine("");
        writer.WriteLine("LuaDBTable.SetLuaTableField(\""+script_name+"\")");

        writer.Flush();
        writer.Close();
        Debug.Log("Write WDB file finished!");
    }

    public static void RegisterDBinCDataMgr(string script_name)
    {
        string oldText = "LuaCDataMgr." + script_name+"()";

        try
        {
            using (StreamReader reader = new StreamReader("Assets/StreamingAssets/Script/Utils/LuaCDataMgr.txt")) 
            {
                string strLine;
                while ((strLine = reader.ReadLine()) != null)
                {
                    if (strLine.IndexOf(oldText) >= 0)
                    {
                        Debug.Log("Table Has Already Been Registered!!!");
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        StreamWriter writer = File.AppendText("Assets/StreamingAssets/Script/Utils/LuaCDataMgr.txt");
        writer.WriteLine("function LuaCDataMgr." + script_name + "()");
        writer.WriteLine("\tif LuaCDataMgr.m_" + script_name + " == nil then");
        writer.WriteLine("\t\tLuaCDataMgr." + "m_" + script_name + " = " + "CDataMgr.get_Instance():GetOrCreateDB(\"" + script_name+"\") end");
        writer.WriteLine("\treturn LuaCDataMgr." + "m_"+script_name);
        writer.WriteLine("end");

        writer.Flush();
        writer.Close();
        Debug.Log("RegisterDB in LuaCDataMgr finished!");
    }

    public static void RegisterWDBDataInCSharp(string script_name) 
    {
        StreamReader reader = new StreamReader("ScriptProject/Code/LKBase/DataMgr/CDataMgr.cs");
        //小写变量名
        string[] lowNameArray = script_name.Split('_');
        string lowName = "";
        for (int i = 0; i < lowNameArray.Length; i++)
        {
            lowName += (lowNameArray[i].Substring(0, 1).ToLower() + lowNameArray[i].Substring(1)+"_");
        }
        //大写变量名
        string[] upNameArray = script_name.Split('_');
        string upName = "";
        for (int i = 0; i < upNameArray.Length; i++)
        {
            upName += (upNameArray[i].Substring(0, 1).ToUpper() + upNameArray[i].Substring(1));
        }

        string oldText = upName;
        string strLine = reader.ReadLine();
        List<string> wholeText = new List<string>();

        while (strLine != null)
        {
            wholeText.Add(strLine);
            if (strLine.IndexOf(oldText) >= 0)
            {
                Debug.Log("Csharp File Has Already Been Registered!!!");
                return;
            }
            strLine = reader.ReadLine();
        }

        reader.Close();
        reader.Dispose();

        StreamWriter writer = new StreamWriter("ScriptProject/Code/LKBase/DataMgr/CDataMgr.cs");
        for (int i = 0; i < wholeText.Count - 2;i++)
        {
            writer.WriteLine(wholeText[i]);
        }

        writer.WriteLine("\tprivate static WDBData " + lowName + ";");
        writer.WriteLine("\tpublic static WDBData " + upName);
        writer.WriteLine("\t{");
        writer.WriteLine("\t\tget");
        writer.WriteLine("\t\t{");
        writer.WriteLine("\t\t\tif (" + lowName + " == null)");
        writer.WriteLine("\t\t\t\t" + lowName + " = CDataMgr.instance.GetOrCreateDB(\"" + script_name + "\");");
        writer.WriteLine("\t\t\treturn " + lowName + ";");
        writer.WriteLine("\t\t}");
        writer.WriteLine("\t}");
        writer.WriteLine("");
        writer.WriteLine("");
        writer.WriteLine("}");

        writer.Flush();
        writer.Close();
        Debug.Log("RegisterDB in CSharp finished!");
    }
}
