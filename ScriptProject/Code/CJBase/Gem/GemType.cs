//======================================================
//  GemType
//  2011.1.11 created by Wangnannan
//======================================================
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class GemType
{
	/// <summary>
	/// Using RawData to init struct.
	/// </summary>
	/// <param name="rawdatas">
	/// A <see cref="System.Byte[]"/>
	/// </param>
	/// <param name="anytype">
	/// A <see cref="Type"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.Object"/>
	/// </returns>
	public static object RawDeserialize (byte[] rawdatas, Type anytype)
	{
		int rawsize = Marshal.SizeOf (anytype);
		if (rawsize > rawdatas.Length)
			return null;
		IntPtr buffer = Marshal.AllocHGlobal (rawsize);
		Marshal.Copy (rawdatas, 0, buffer, rawsize);
		object retobj = Marshal.PtrToStructure (buffer, anytype);
		Marshal.FreeHGlobal (buffer);
		return retobj;
	}
}
