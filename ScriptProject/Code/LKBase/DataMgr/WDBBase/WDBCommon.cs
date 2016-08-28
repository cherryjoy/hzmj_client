//======================================================
//  WDBCommon
//  2011.1.12 created by Wangnannan
//======================================================
using UnityEngine;
public enum EWDB_FIELD_TYPE
{
	WFT_NULL = 0,
	WFT_INT = 'i',
	WFT_FLOAT = 'f',
	WFT_STRING = 's',
	WFT_INDEX = 'n',
	WFT_V_INDEX = 'v',
	WFT_STRINGTABLE = 't',
    WFT_LONG = 'l',
}

public class WDBConst
{
	public static readonly int INVAILD_STRINGID = -1;
	public static readonly int INVAILD_VINDEX_ID = -1;
	public static readonly uint HeadMark = 0x42444B57;
}

