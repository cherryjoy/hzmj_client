using System;
using System.Collections.Generic;

public static class PanelVertexBuffer
{
	private static List<int[]> bufferMaps = new List<int[]>();

	static PanelVertexBuffer()
	{
		for (int i = 60; i <= 2400 ; i += 60)
		{
			bufferMaps.Add(new int[i]);
		}
	}

	public static int[] GetBuffer(int sizeNeed)
	{
		for (int i = 0; i < bufferMaps.Count; i++ )
		{
			if (bufferMaps[i].Length >= sizeNeed)
			{
				return bufferMaps[i];
			}
		}
		if (sizeNeed >  30000)
		{
			UnityEngine.LKDebug.LogWarning(" Size Too Larger : " + sizeNeed);
		}
		return new int[sizeNeed];
	}
}
