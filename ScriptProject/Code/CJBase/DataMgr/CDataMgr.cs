//======================================================
//  Client Data Mgr
//  2011.1.12 created by Wangnannan
//======================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public class CDataMgr : Singleton<CDataMgr>
{
	private const string data_base_path_ = "Data";
    public static Dictionary<string, WDBData> dataDic = new Dictionary<string,WDBData>();

	public WDBData InitDBData(string name)
	{
        byte[] asset = ResLoader.LoadRaw("Data/"+ name + "_ndb");
		if (asset != null)
		{
            byte[] tbl_asset = ResLoader.LoadRaw("Data/" + name + "_tbl");
			WDBData data = new WDBData();
			if (tbl_asset != null)
			{
                data.LoadNDB(asset, tbl_asset, name);
			}
			else
			{
                data.LoadNDB(asset, null, name);
			}
			return data;
		}
        Debug.Log(name + "NULLLLL");
		return null;
	}

	public WDBData GetOrCreateDB(string dbName)
	{
        WDBData data;
        dataDic.TryGetValue(dbName, out data);
        if (data != null)
        {
            return data;
        }
        else {
            WDBData newData = InitDBData(dbName);
            dataDic.Add(dbName, newData);
            return newData;
        }
	}

    public void Clear()
    {
        dataDic.Clear();
    }

	public static WDBData Coordinates
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Coordinates");
		}
	}

	public static WDBData Bullets
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Bullets");
		}
	}

	public static WDBData EffectBullets
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("EffectBullets");
		}
	}

	public static WDBData BulletCreate
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("BulletCreate");
		}
	}

	public static WDBData BulletAreas
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("BulletAreas");
		}
	}

	public static WDBData Skills
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Skills");
		}
	}

	public static WDBData Monsters
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Monsters");
		}
	}
	public static WDBData SkillInitiatives
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("SkillInitiatives");
		}
	}

	public static WDBData Effects
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Effects");
		}
	}

	public static WDBData MonsterValue
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("monster_value");
		}
	}

	public static WDBData Buffers
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Buffers");
		}
	}

	public static WDBData BufferValues
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("BufferValues");
		}
	}

	public static WDBData Actor
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Actor");
		}
	}

	public static WDBData Players
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Players");
		}
	}

	public static WDBData Emitters
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Emitters");
		}
	}

	public static WDBData BulletCombat
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("BulletCombat");
		}
	}

	public static WDBData BulletDamages
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("BulletDamages");
		}
	}

	public static WDBData PlayerSkillEquips
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("PlayerSkillEquips");
		}
	}

	public static WDBData MonsterSkillEquips
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("MonsterSkillEquips");
		}
	}

	public static WDBData ComboConfigs
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("ComboConfigs");
		}
	}

	public static WDBData SkillMoves
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("SkillMoves");
		}
	}

	public static WDBData GameConfig
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("GameConfig");
		}
	}
    public static WDBData MonsterGroup
    {
        get
        {
             return CDataMgr.Instance.GetOrCreateDB("MonsterGroup");
        }
    }

	public static WDBData AIs
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("AIs");
		}
	}

	public static WDBData Events
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Events");
		}
	}

	public static WDBData Actions
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Actions");
		}
	}

    public static WDBData Obstacles
    {
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Obstacles");
		}
    }

	public static WDBData PlayerProperties
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("PlayerProperties");
		}
	}

	public static WDBData MonsterProperties
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("MonsterProperties");
		}
	}

	public static WDBData FightConfig
	{
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("FightConfig");
		}
	}

    public static WDBData Storys
    {
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Storys");
		}
    }

    public static WDBData Equip_abi
    {
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Equip_abi");
		}
    }

    public static WDBData Equip_Part
    {
		get
		{
			 return CDataMgr.Instance.GetOrCreateDB("Equip_Part");
		}
    }

	public static WDBData BufferParams
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("BufferParams");
		}
	}

	public static WDBData GameAbi
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("GameAbi");
		}
	}

    public static WDBData Item
    {
        get
        {
             return CDataMgr.instance.GetOrCreateDB("Item");
        }
    }

    public static WDBData CityPlayers
	{
		get
		{
             return CDataMgr.instance.GetOrCreateDB("CityPlayers");
		}
	}

	public static WDBData GameScene
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("GameScene");
		}
	}

    public static WDBData Material
    {
        get
        {
             return CDataMgr.instance.GetOrCreateDB("Material");
        }
    }
	public static WDBData FairyFollow
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("Fairy_Follow");
		}
	}

	public static WDBData NPC
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("NPC");
		}
	}

	public static WDBData ButtonOpens
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("ButtonOpens");
		}
	}

	public static WDBData SceneStars
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("SceneStars");
		}
	}

	public static WDBData WeaponAbi
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("Weapon_abi");
		}
	}

	public static WDBData Dialog
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("Dialog");
		}
	}

	public static WDBData MonstersTalk
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("MonstersTalk");
		}
	}

	public static WDBData GameSceneProcess
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("GameSceneProcess");
		}
	}

	public static WDBData FashionAbi
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("Fashion_Abi");
		}
	}

	public static WDBData CameraAnimation
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("CameraAnimation");
		}
	}

	public static WDBData SkillUpgradeDamage
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("SkillUpgradeDamage");
		}
	}

	public static WDBData LoginPlayers
	{
		get
		{
			 return CDataMgr.instance.GetOrCreateDB("LoginPlayers");
		}
	}

	public static WDBData PlayerSounds
	{
		get
		{
            return CDataMgr.instance.GetOrCreateDB("PlayerSounds");
		}
	}

	public static WDBData SkillEffectShow
	{
		get
		{
			return CDataMgr.instance.GetOrCreateDB("SkillEffectShow");
		}
	}

	public static WDBData GameSceneShow
	{
		get
		{
			return CDataMgr.instance.GetOrCreateDB("GameScene_Show");
		}
	}
    public static WDBData City
    {
        get
        {
            return CDataMgr.instance.GetOrCreateDB("City");
        }
    }

}
