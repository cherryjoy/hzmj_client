using UnityEngine;
using System.Collections;

public class WDB_Item
{
	public static readonly int Id;
	public static readonly int Name;
	public static readonly int Desc;
	public static readonly int Body;
	public static readonly int Body_Drop;
	public static readonly int Drop_Effect;
	public static readonly int Quality;
	public static readonly int Bag;
	public static readonly int Use;
	public static readonly int AutuGein;
	public static readonly int StackSum;
	public static readonly int IsResolve;
	public static readonly int IsSold;
	public static readonly int Price;
	public static readonly int IsAffirmSold;
	public static readonly int Icon;
	public static readonly int IconType;
	public static readonly int Function;
	public static readonly int BuffId;
	public static readonly int Drop;
	public static readonly int TrunAround;
	public static readonly int GainEffect;

	static WDB_Item()
	{
		WDBData db = CDataMgr.Item;
		if(db != null)
		{
			db.GetFieldByName("Id", out Id);
			db.GetFieldByName("Name", out Name);
			db.GetFieldByName("Desc", out Desc);
			db.GetFieldByName("Body", out Body);
			db.GetFieldByName("Body_Drop", out Body_Drop);
			db.GetFieldByName("Drop_Effect", out Drop_Effect);
			db.GetFieldByName("Quality", out Quality);
			db.GetFieldByName("Bag", out Bag);
			db.GetFieldByName("Use", out Use);
			db.GetFieldByName("AutuGein", out AutuGein);
			db.GetFieldByName("StackSum", out StackSum);
			db.GetFieldByName("IsResolve", out IsResolve);
			db.GetFieldByName("IsSold", out IsSold);
			db.GetFieldByName("Price", out Price);
			db.GetFieldByName("IsAffirmSold", out IsAffirmSold);
			db.GetFieldByName("Icon", out Icon);
			db.GetFieldByName("IconType", out IconType);
			db.GetFieldByName("Function", out Function);
			db.GetFieldByName("BuffId", out BuffId);
			db.GetFieldByName("Drop", out Drop);
			db.GetFieldByName("TrunAround", out TrunAround);
			db.GetFieldByName("GainEffect", out GainEffect);
		}
	}
}
