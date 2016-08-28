using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class LKJsonObj
{
    object mData;
    public LKJsonObj(object data)
    {
        mData = data;
    }
    public object data
    {
        get
        {
            return mData;
        }
    }

    public LKJsonObj this[string key]
    {
        get 
        {
            if (mData == null)
                return null;
            if (mData.GetType() == typeof(Dictionary<string, object>))
            {
                Dictionary<string, object> data = mData as Dictionary<string, object>;
                if (data.ContainsKey(key))
                    return data[key] as LKJsonObj;
                else
                    return null;
            }
            else
                return null;
        }
        set 
        {
            if (mData == null)
                return ;
            if (mData.GetType() == typeof(Dictionary<string, object>))
            {
                Dictionary<string, object> data = mData as Dictionary<string, object>;
                if (data.ContainsKey(key))
                {
                    data[key] = value;
                }
                else
                {
                    data.Add(key, value);
                }
            }
        }
    }

    public LKJsonObj this[int key]
    {
        get
        {
            if (mData == null)
                return null;
            if (mData.GetType() == typeof(List<object>))
            {
                List<object> data = mData as List<object>;
                return data[key] as LKJsonObj;
            }
            else
                return null;
        }
        set
        {
            if (mData == null)
                return ;
            if (mData.GetType() == typeof(List<object>))
            {
                List<object> data = mData as List<object>;
                data.Add(value);
            }
        }
    }

    public int Length
    {
        get
        {
            if (mData == null)
                return 0;
            if (mData.GetType() == typeof(List<object>))
                return (mData as List<object>).Count;
            else if (mData.GetType() == typeof(Dictionary<string, object>))
                return (mData as Dictionary<string, object>).Count;
            else
                return 0;
        }
    }

	public bool ContainsKey(string keyVal)
	{
		if ((mData != null) && (mData.GetType() == typeof(Dictionary<string, object>)))
			return (mData as Dictionary<string, object>).ContainsKey(keyVal);

		return false;
	}

	public Dictionary<string, object> GetDictionaryData()
	{
		if ((mData != null) && (mData.GetType() == typeof(Dictionary<string, object>)))
			return mData as Dictionary<string, object>;
		return null;
	}
    public List<object> GetListObject()
    {
        if (mData != null && mData.GetType() == typeof(List<object>) )
            return mData as List<object>;
        return null;
    }
    public Type GetDataType()
    {
        return mData.GetType();
    }
	public static implicit operator string(LKJsonObj obj)
	{
		if (obj.mData == null)
			return string.Empty;
		if (obj.mData.GetType() == typeof(string))
			return (string)obj.mData;
		else
			return string.Empty;
	}

	public override string ToString()
	{
		return (string)this;
	}
}

public class LKJson
{
    enum EToken
    {
        left_brace,     /// {
        right_brace,    /// }
        left_bracket,   /// [
        right_bracket,  /// ]
        quotation,      /// "
        colon,          /// :
        comma,          /// , 
        unknown,        ///
        end             /// end of input
    }

    public LKJson()
    {

    }

    int mReadIndex = 0;

    void BeginParse()
    {
        mReadIndex = 0;
    }

    bool GetCharacter(string data, out char outc)
    {
        if (mReadIndex < data.Length)
        {
            outc = data[mReadIndex++];
            return true;
        }
        else
        {
            outc = (char)0;
            return false;
        }
    }

    void GiveBack()
    {
        mReadIndex--;
    }

    bool JumpOverEmptyChar(string data)
    {
        while (mReadIndex < data.Length)
        {
            char c = data[mReadIndex];
            if (IsEmptyChar(c) == true)
            {
                mReadIndex++;
            }
            else
                return true;
        }
        return false;
    }

    // read a token string
    EToken ReadToken(string data, ref string outs)
    {
        bool res = JumpOverEmptyChar(data);
        if (res == false)
        {
            outs = string.Empty;
            return EToken.end;
        } 
        StringBuilder sb = new StringBuilder();

        while (true)
        {
            char c;
            res = GetCharacter(data, out c);
            if (res == true)
            {
                EToken token = GetToken(c);
                if (token == EToken.unknown)
                {
                    if (IsEmptyChar(c) == false)
                        sb.Append(c);
                    else
                    {
                        outs = sb.ToString();
                        return EToken.unknown;
                    }
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        outs = sb.ToString();
                        mReadIndex--; // give back
                        return EToken.unknown;
                    }
                    else
                    {
                        outs = string.Empty;
                        return token;
                    }
                }
            }
            else
            {
                outs = sb.ToString();
                return EToken.end;
            }   
        }
    }

    string ReadString(string data)
    {
        StringBuilder builder = new StringBuilder();
        while (mReadIndex < data.Length)
        {
            char c = data[mReadIndex];
            if (c == '"')
            {
                mReadIndex++;
                break;
            }
            else if (c == '\\')
            {
                mReadIndex++;
                if (mReadIndex == data.Length)
                    return builder.ToString();
                else
                {
                    c = data[mReadIndex++];
                    switch (c)
                    {
                        case 'n':
                            builder.Append('\n'); break;
                        case '\\':
                            builder.Append('\\'); break;
                        case 't':
                            builder.Append('\t'); break;
                        case 'r':
                            builder.Append('\r'); break;
                        case '\"':
                            builder.Append('\"'); break;
                        default:
                            builder.Append(c); break;
                    }
                }
            }
            else
            {
                mReadIndex++;
                builder.Append(c);
            }
        }
        return builder.ToString();
    }

    LKJsonObj ReadObject(string data)
    {
        Dictionary<string, object> value_map = new Dictionary<string, object>();

        do
        {
            // find "
            string str_token = string.Empty;
            EToken token = ReadToken(data, ref str_token);
            if (token == EToken.quotation)
            {
                string key = ReadString(data);
                EToken colon = ReadToken(data, ref str_token);
                if (colon == EToken.colon)
                {
                    object val = ReadJsObject(data);

                    value_map.Add(key, val);
                }
                else
                    throw new Exception("need : between key and value");

                token = ReadToken(data, ref str_token);
                if (token == EToken.right_brace)
                    return new LKJsonObj(value_map);
                else if (token == EToken.comma)
                {
                    continue;
                }
                else
                    throw new Exception("need } at the end");
            }
            else if (token == EToken.right_brace)
            {
                return new LKJsonObj(value_map);
            }
            else
                throw new Exception("invalid token");
            
        } while (true);
    }

    public LKJsonObj DeSerializer(string data)
    {
        BeginParse();

        return ReadJsObject(data);
    }

    LKJsonObj ReadArray(string data)
    {
        List<object> array = new List<object>();
        do
        {
            object obj = ReadJsObject(data);
            if (obj != null)
                array.Add(obj);
            string str_token = string.Empty;
            EToken token = ReadToken(data, ref str_token);
            if (token == EToken.right_bracket)
                return new LKJsonObj(array);
            else if (token == EToken.comma)
            {
                continue;
            }
            else
                throw new Exception("invalid token");
        } while (true);
    }

    public LKJsonObj ReadJsObject(string data)
    {
        string str_token = string.Empty;
        EToken token = ReadToken(data, ref str_token);
        if (token == EToken.left_brace)
        {
            return ReadObject(data);
        }
        else if (token == EToken.left_bracket)
        {
            return ReadArray(data);
        }
        else if (token == EToken.quotation)
        {
            return new LKJsonObj(ReadString(data));
        }
        else if (token == EToken.unknown)
        {
            /*
            int n = 0;
            float f = 0;
            if (int.TryParse(str_token, out n) == true)
                return new LKJsonObj(n);
            else if (float.TryParse(str_token, out f) == true)
                return new LKJsonObj(f);

            if (str_token == "true")
                return new LKJsonObj(true);
            else if (str_token == "false")
                return new LKJsonObj(false);
            else if (str_token == "null")
                return null;
            */
            return new LKJsonObj(str_token);
        }
        else if (token == EToken.right_bracket)
        {
            // give back
            GiveBack();
            return null;
        }
        else
            throw new Exception("invalid token");
    }

    EToken GetToken(char character)
    {
        switch (character)
        {
            case '[': return EToken.left_bracket;
            case ']': return EToken.right_bracket;
            case '{': return EToken.left_brace;
            case '}': return EToken.right_brace;
            case '\"': return EToken.quotation;
            case ':': return EToken.colon;
            case ',': return EToken.comma;
            default: return EToken.unknown;
        }
    }

    bool IsEmptyChar(char character)
    {
        if (character == ' ' || character == '\t' || character == '\r' || character == '\n')
            return true;
        else
            return false;
    }
    public static string SerializeJson(LKJsonObj jsonObject)
    {
        TabLevel = 0;
        isFromArray = false;
        return GetString(jsonObject);
    }
    private static int TabLevel = 0;
    private static string GetString(LKJsonObj jsonObject)
    {
        TabLevel++;
        string tabString = string.Empty;
        for (int i = 0; i < TabLevel; i++)
        {
            tabString += "\t";
        }
        StringBuilder sb = new StringBuilder();
        if (isFromArray)
        {
            sb.Append("\n");
        }
        sb.Append(tabString.Substring(0, tabString.LastIndexOf('\t')));
        sb.Append("{\n");
        Dictionary<string, object> jsonDic = jsonObject.GetDictionaryData();
        if (jsonDic != null)
        {
            IEnumerator enumer = jsonDic.GetEnumerator();
		    while (enumer.MoveNext())
		    {
                KeyValuePair<string, object> kvp = (KeyValuePair<string, object>)enumer.Current;
                LKJsonObj dirObj =  kvp.Value as LKJsonObj;
                if (dirObj == null)
                {
                    sb.Append(string.Format("{0}\"{1}\":\"{2}\",\n", tabString, kvp.Key, DisPlayContent(kvp.Value.ToString())));
                }
                else if (dirObj.GetDataType() == typeof(Dictionary<string, object>)) 
                {
                    sb.Append(string.Format("{0}\"{1}\":{2},\n", kvp.Key, GetString(dirObj)) );
                }
                else if (dirObj.GetDataType() == typeof(List<object>))
                {
                    sb.Append(string.Format("{0}\"{1}\":{2},\n", tabString, kvp.Key, GetArrayString(dirObj.GetListObject())));
                }
                else if (dirObj.GetDataType() == typeof(string))
                {
                    sb.Append(string.Format("{0}\"{1}\":\"{2}\",\n", tabString, kvp.Key, DisPlayContent(kvp.Value.ToString())));
                }
                else
                {
                    sb.Append(string.Format("{0}\"{1}\":\"{2}\",\n", tabString, kvp.Key, DisPlayContent(kvp.Value.ToString())));
                }
            }
        }
        if (sb.Length > 2)
            sb.Remove(sb.Length - 2, 2);
        sb.Append("\n");
        sb.Append(tabString.Substring(0, tabString.LastIndexOf('\t')));
        sb.Append("}");
        TabLevel--;
        return sb.ToString();
    }
    static string DisPlayContent(string str)
    {
        string endStr = str.Replace("\\\\", "\\");
        endStr = endStr.Replace("\\", "\\\\");
        return endStr;
    }
    private static bool isFromArray = false;
    private static string GetArrayString(List<object> jsonArray)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        for (int i = 0; i < jsonArray.Count; i++)
        {
            LKJsonObj dirObj = jsonArray[i] as LKJsonObj;
            if (dirObj == null)
            {
                sb.Append(string.Format("\"{0}\",", DisPlayContent(jsonArray[i].ToString())));
            }
            else if (dirObj.GetDataType() == typeof(Dictionary<string, object>))
            {
                isFromArray = true;
                sb.Append(string.Format("{0},", GetString(dirObj) ));
                isFromArray = false;
            }
            else if (dirObj.GetDataType() == typeof(List<object>))
            {
                sb.Append(string.Format("{0},", GetArrayString(dirObj.GetListObject())));
            }
            else if (dirObj.GetDataType() == typeof(string))
            {
                sb.Append(string.Format("\"{0}\",", DisPlayContent(dirObj.ToString())));
            }
            else
            {
                sb.Append(string.Format("\"{0}\",", DisPlayContent(jsonArray[i].ToString())));
            }

        }
        if (sb.Length > 1)
            sb.Remove(sb.Length - 1, 1);
        sb.Append("]");
        return sb.ToString();
    }
}
