using UnityEngine;
using System.Collections;

// Note: Need explicit manually initialize!
public struct SafeInteger
{
	int mValue;
	const int Mask = 0x0d0d0d0d;

	public static implicit operator int(SafeInteger si)
	{
		int v = (int)((uint)si.mValue << 16 | (uint)si.mValue >> 16);
		return v ^ Mask;
	}
	
	public static implicit operator SafeInteger(int n)
	{
		SafeInteger si;
		n = (int)((uint)n << 16 | (uint)n >> 16);
		si.mValue = n ^ Mask;
		
		return si;
	}

	public static SafeInteger operator ++(SafeInteger n)
	{
		return n += 1;
	}
	
	public override string ToString()
	{	
		int v = (int)this;
		return v.ToString();
	}
}
