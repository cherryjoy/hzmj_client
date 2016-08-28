using UnityEngine;
using System.Collections;

public abstract class ModelBase
{
	public int id_;
	public const int INVALID_ID = -1;
	public abstract bool LoadConfig(int id);
}
