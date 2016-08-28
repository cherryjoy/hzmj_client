using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
namespace UnityEngine
{
	public class LKDebug
	{
		
#if UNITY_IPHONE
	[DllImport ("__Internal")]
		static extern void Log(string msg);
#endif

		
		public static void Log(object message)
		{
#if UNITY_IPHONE
			Log("Normal Log: " + string.Format("{0}", message));
#else
			Debug.Log(message);
#endif
		}

		public static void LogError(object message)
		{
#if UNITY_IPHONE
			Log("Error Log: " + string.Format("{0}", message));
#else
			Debug.LogError(message);
			
#endif
		}

		public static void LogWarning(object message)
		{
#if UNITY_IPHONE
			Log("Warning Log: " + string.Format("{0}", message));
#else
			Debug.LogWarning(message);
#endif
		}
	}
}
