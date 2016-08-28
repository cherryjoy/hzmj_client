using UnityEngine;
using System.Collections;

public class ModelItem : ModelBase
{
    public string name_;
    public string desc_;
    public string body_;
    public string body_drop_;
    public int drop_effect_;
    public int quality_;
    public int bag_;
    public int use_;
    public int autu_gein_;
    public int stack_sum_;
    public int is_resolve_;
    public int is_sold_;
    public int price_;
    public int is_affirm_sold_;
    public string icon_;
    public int icon_type_;
    public int function_;
    public int buff_id_;
    public int drop_;
    public int trun_around_;
    public int gain_effect_;

    public override bool LoadConfig(int id)
    {
        WDBSheetLine line = CDataMgr.Item.GetData(id);
        if (line == null)
        {
            LKDebug.LogError("LoadConfig error: can't find " + id + " in Item table!");
            return false;
        }

        id_ = line.GetData<int>(WDB_Item.Id);
        name_ = line.GetData<string>(WDB_Item.Name);
        desc_ = line.GetData<string>(WDB_Item.Desc);
        body_ = line.GetData<string>(WDB_Item.Body);
        body_drop_ = line.GetData<string>(WDB_Item.Body_Drop);
        drop_effect_ = line.GetData<int>(WDB_Item.Drop_Effect);
        quality_ = line.GetData<int>(WDB_Item.Quality);
        bag_ = line.GetData<int>(WDB_Item.Bag);
        use_ = line.GetData<int>(WDB_Item.Use);
        autu_gein_ = line.GetData<int>(WDB_Item.AutuGein);
        stack_sum_ = line.GetData<int>(WDB_Item.StackSum);
        is_resolve_ = line.GetData<int>(WDB_Item.IsResolve);
        is_sold_ = line.GetData<int>(WDB_Item.IsSold);
        price_ = line.GetData<int>(WDB_Item.Price);
        is_affirm_sold_ = line.GetData<int>(WDB_Item.IsAffirmSold);
        icon_ = line.GetData<string>(WDB_Item.Icon);
        icon_type_ = line.GetData<int>(WDB_Item.IconType);
        function_ = line.GetData<int>(WDB_Item.Function);
        buff_id_ = line.GetData<int>(WDB_Item.BuffId);
        drop_ = line.GetData<int>(WDB_Item.Drop);
        trun_around_ = line.GetData<int>(WDB_Item.TrunAround);
        gain_effect_ = line.GetData<int>(WDB_Item.GainEffect);

        return true;
    }
}
