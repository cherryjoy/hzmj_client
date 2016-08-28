using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Text;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WDBHeader
{
	public uint nId;
	public int nRecords;
	public int nFields;
	public int nRecordSize;
	public int nTextSize;
	public uint uVersion;
	public int nNameBufLen;
}

public class WDBSheetLine
{
	public object[] m_Line;
	public object GetData (int field)
	{
		if (field >= 0 && field < m_Line.Length) {
			return m_Line[field];
		}
		return null;
	}
	
	public T GetData<T> (int field)
	{
		if (field >= 0 && field < m_Line.Length) {
			return (T)m_Line[field];
		}
		Debug.LogWarning("lineData.GetData:" + field + " error!");
		return WDBData.createInstance<T>();
	}
}


unsafe public class WDBData
{
	protected byte[] mData;
	protected WDBHeader mHeader;
	public Dictionary<string, int> mFieldName;
	public int[] mFieldType;
	protected Dictionary<int, int> mIndex;
	protected int mDataOffset;
	protected int mStringOffset;

	WDBData mTBl;

	// path name with out extension
	/*
	public void LoadNDB(string path)
	{
		mTBl = new WDBData();
		mTBl.OpenData(path + ".tbl");
		OpenData(path + ".ndb");
	}
	*/

	public void Init()
	{
	}

	public byte[] GetData()
	{
		return mData;
	}

	public void LoadNDB(byte[] ndb, byte[] tbl,string ndbName)
	{
        OpenData(ndb, ndbName);
		if (tbl != null)
		{
			mTBl = new WDBData();
            mTBl.OpenData(tbl, ndbName);
		}
	}
    private static bool GetNDBStrLen(byte[] buff, int index, ref int nextindex, ref int count)
	{
		if (index >= 0 && index < buff.Length && buff.Length >= sizeof(short)) 
		{
			count = 0;
            /*
			while (BitConverter.ToInt16 (buff, index) != 0) 
			{
				index += sizeof(short);
				count += sizeof(short);
			}
             */
            while (true)
            {
                fixed (byte* p = &buff[index])
                {
                    short* pv = (short*)p;
                    if (*pv == 0)
                        break;
                }
                index += sizeof(short);
                count += sizeof(short);
            }
            nextindex = index + sizeof(short);
            return true;
			
		}
		return false;
	}

	public int GetFieldType (int field)
	{
		return mFieldType[field];
	}

    public static int GetOffset(byte[] data)
    {
        WDBHeader header = (WDBHeader)GemType.RawDeserialize(data, typeof(WDBHeader));
        if (header.nId != WDBConst.HeadMark)
            return 0;
        int DataOffset = Marshal.SizeOf(typeof(WDBHeader));

        // read filed name
        int bufLen = header.nNameBufLen * sizeof(short);
        if (bufLen > 0)
        {
            int index = DataOffset;
            for (int i = 0; i < header.nFields; i++)
            {
                int nextindex = 0, count = 0;
                if (GetNDBStrLen(data, index, ref nextindex, ref count))
                {
                    string fieldName = Encoding.Unicode.GetString(data, index, count);
                    index = nextindex;
                }
                else
                    return 0;
            }
            DataOffset = index;
        }
        else
            return 0;

        return DataOffset;
    }

	public bool OpenData(byte[] data,string ndbName)
	{
		/*
		FileStream fs = new FileStream(path, FileMode.Open);
		if (fs.CanRead == false)
			return false;
		mData = new byte[fs.Length];
		fs.Read(mData, 0, (int)fs.Length);
		fs.Close();
		*/
		mData = data;
		// read header
		mHeader = (WDBHeader)GemType.RawDeserialize(mData, typeof(WDBHeader));
		if (mHeader.nId != WDBConst.HeadMark)
			return false;
		mDataOffset = Marshal.SizeOf(typeof(WDBHeader));

		// read filed name
		int bufLen = mHeader.nNameBufLen * sizeof(short);
		if (bufLen > 0) 
		{
			int index = mDataOffset;
			mFieldName = new Dictionary<string, int> ();
			for (int i = 0; i < mHeader.nFields; i++) 
			{
				int nextindex = 0, count = 0;
				if (GetNDBStrLen (mData, index, ref nextindex, ref count)) 
				{
					string fieldName = Encoding.Unicode.GetString (mData, index, count);
					index = nextindex;
					if (mFieldName.ContainsKey(fieldName))
					{
                        LKDebug.LogError(fieldName + " Have Exist.In table:" + ndbName);
						continue;
					}
					
					mFieldName.Add (fieldName, i);
				} 
				else
					return false;
			}
			mDataOffset = index;
		}
		else
			return true;

        if (mDataOffset % 4 != 0)
            mDataOffset += 2;
		if (mDataOffset % 4 !=0)
		{
			mDataOffset += 2;
		}
		
		// read the first data as field type
		mFieldType = new int[mHeader.nFields];
		for (int j = 0; j < mHeader.nFields; j++) 
		{
			mFieldType[j] = BitConverter.ToInt32 (mData, mDataOffset + j * sizeof(int));
		}

        for (int j = 0; j < mHeader.nFields; j++)
        {
            mDataOffset += sizeof(int);
        }

		// string offset
		mStringOffset = mDataOffset + mHeader.nRecords * mHeader.nRecordSize;

		// create index
		int indexfield = -1;
		for (int i = 0; i < GetFieldCount (); i++) 
		{
			int type = GetFieldType(i);
			if (type == (int)EWDB_FIELD_TYPE.WFT_V_INDEX) 
			{
				indexfield = i;
			}
		}
		if (indexfield >= 0) 
		{
			mIndex = new Dictionary<int, int> ();
			//MaybeEmpty
			for (int i = 0; i < GetRecordCount (); i++) 
			{
				int activeIndex = (int)GetDataByNumber (i, indexfield);
				if (mIndex.ContainsKey(activeIndex))
				{
                    LKDebug.LogError(activeIndex + " Have Exist.In table:" + ndbName);
					continue;
				}
				mIndex.Add (activeIndex, i);
			}
		}

		return true;
	}

	public object GetDataByNumber(int row, int field)
	{
		object obj = null;
		if (row >= 0 && row < GetRecordCount () && 
		    field >= 0 && field < GetFieldCount()) 
		{
			int data_offset = mDataOffset + row * mHeader.nRecordSize;
            int n = 0;
            if (GetFieldType(field) == (int)EWDB_FIELD_TYPE.WFT_FLOAT)
            {
                //obj = BitConverter.ToSingle(mData, data_offset + field * sizeof(int));



                fixed (byte* p = &mData[data_offset + GetOffsetInRecord(field)])
                {
                    obj = *((float*)p);
                }
                return obj;
            }
            else if (GetFieldType(field) == (int)EWDB_FIELD_TYPE.WFT_LONG)
            {
                long l = 0;
                fixed (byte* p = &mData[data_offset + GetOffsetInRecord(field)])
                {
                    l = *((long*)p);
                }

                return l;
            }
            else
            {
                //obj = BitConverter.ToInt32(mData, data_offset + field * sizeof(int));

                fixed (byte* p = &mData[data_offset + GetOffsetInRecord(field)])
                {
                    n = *((int*)p);
                }



            }

			if (GetFieldType(field) == (int)EWDB_FIELD_TYPE.WFT_STRING)
			{
				int nextstrindex = 0, count = 0;
				int strindex = n + mStringOffset;

				if (GetNDBStrLen (mData, strindex, ref nextstrindex, ref count)) 
				{
					obj = Encoding.Unicode.GetString (mData, strindex, count);
				}
			}
			else if (GetFieldType(field) == (int)EWDB_FIELD_TYPE.WFT_STRINGTABLE)
			{
				int strindex = n;
				obj = mTBl.GetData (strindex, 0);
			}
            else
                return n;
		}
		return obj;
	}

    int GetOffsetInRecord(int field)
    {
        int offset = 0;
        for (int i = 0; i < field; i++)
        {
            if (GetFieldType(i) == (int)EWDB_FIELD_TYPE.WFT_LONG)
            {
                offset += sizeof(long);
            }
            else
            {
                offset += sizeof(int);
            }

        }

        return offset;

    }

	public WDBSheetLine GetData (int index)
	{
		if (mIndex != null) {
			int row = 0;
			if (mIndex.TryGetValue (index, out row))
				return GetDataByNumber (row);
		} else {
			return GetDataByNumber (index);
		}
		return null;
	}
	public object GetData (int index, int field)
	{
		/*
		WDBSheetLine line = GetData (index);
		if (line != null) {
			return line.GetData (field);
		}
		return null;
		*/
		if (mIndex != null) {
			int row = 0;
			if (mIndex.TryGetValue (index, out row))
				return GetDataByNumber (row, field);
		} else {
			return GetDataByNumber (index, field);
		}
		return null;
	}

    public int GetRowNumByIndex(int index) {
        if (mIndex != null)
        {
            int row = 0;
            if (mIndex.TryGetValue(index, out row))
                return row;
        }
        return -1;
    }
	public T GetData<T>(int index, int field)
	{
		object o = GetData(index, field);
		if (o != null)
			return (T)o;
		
		Debug.LogWarning("index:" + index + "field:" + field + " is null");
		return createInstance<T>();
	}
	public T GetData<T> (int index, string fieldName)
	{
		int filed = 0;
		if (GetFieldByName (fieldName, out filed)) {
			return GetData<T> (index, filed);
		}
		Debug.LogWarning("index:" + index + "fieldName:" + fieldName + " is null");
        return createInstance<T>();
	}
	public T GetDataByNumber<T> (int row, int field)
	{
		/*
		WDBSheetLine line = GetDataByNumber (row);
		if (line != null) {
			return line.GetData<T>(field);
		}
		return createInstance<T>();
		*/
		object obj = GetDataByNumber(row, field);
		if (obj != null)
			return (T)obj;
		else
			return createInstance<T>();
	}
	public string GetFieldName(int field)
	{
		foreach (KeyValuePair<string, int> pair in mFieldName)
		{
			if( pair.Value == field)
			{
				return pair.Key;
			}
		}
		return null;
	}
	public static T createInstance<T>()
	{
		if(typeof(T) == typeof(string))
		{
			return (T)(object)string.Empty;
		}
		else
		{
			return System.Activator.CreateInstance<T>();
		}
	}

	public int GetFieldCount ()
	{
		return mFieldName.Count;
	}

	public int GetRecordCount ()
	{
		// field type store in data
		return mHeader.nRecords;
	}

	public WDBSheetLine GetDataByNumber (int row)
	{
		if (row >= 0 && row < GetRecordCount ()) 
		{
			WDBSheetLine data = new WDBSheetLine();
			data.m_Line = new object[mHeader.nFields];

			for (int i = 0; i < mHeader.nFields; i++)
			{
				data.m_Line[i] = GetDataByNumber(row, i);
			}
			
			return data;
		}
		return null;
	}
	public bool GetFieldByName (string fieldName, out int field)
	{
		return mFieldName.TryGetValue (fieldName, out field);
	}
    public int GetFieldByName(string fieldName)
    {
        int field;
        mFieldName.TryGetValue(fieldName, out field);
        return field;
    }
	public string GetFiledName(int field)
	{
		foreach (KeyValuePair<string, int> pair in mFieldName){
			if( pair.Value == field){
				return pair.Key;
			}
		}
		return null;
	}
	public object GetData (int index, string fieldName)
	{
		int filed = 0;
		if (GetFieldByName (fieldName, out filed)) {
			return GetData (index, filed);
		}
		return null;
	}
	public bool HasIndex(int index)
	{
		return mIndex.ContainsKey(index);
	}
}
