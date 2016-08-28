using System;
using System.Collections.Generic;
using UnityEngine;
using UniLua;
using System.IO;
class LuaUtil
{
    static LuaState lua_;
    public static void RegisterToLua(LuaState lua, Type type)
    {
        lua_ = lua;
        string[] funcList = new string[]
        {
            "HashEncrypt",
            "DeleteFile",
            "DeleteAllGameObject",
			"HideAllCam",
            "NotificationMessage",
            "CleanNotification",
            "NotificationMessageAddSeconds"
        };

        LuaAPI.lua_CFunction[] funcDeList = new LuaAPI.lua_CFunction[]
        {
            HashEncrypt,
            DeleteFile,
            DeleteAllGameObject,
			HideAllCam,
            NotificationMessage,
            CleanNotification,
            NotificationMessageAddSeconds,
        };
        LuaWrapper.RegisterToLua(lua, type, funcList, funcDeList);
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int HashEncrypt(IntPtr l)
    {
        string intoToEncrypt = lua_.ToString(-1);

        string passwordEncryption = HashSecurity.Encrypt(intoToEncrypt);
        lua_.PushString(passwordEncryption);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int SetActive(IntPtr l)
    {
        GameObject obj = lua_.ToUserDataObject(-1) as GameObject;

        if (!obj.Equals(null))
        {
            bool isActive = lua_.ToBoolean(-2);
            obj.SetActive(isActive);
        }

        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int DeleteFile(IntPtr l)
    {
        string path = lua_.ToString(-1);
        if (!File.Exists(path))
        {
            lua_.PushBoolean(false);
        }
        else
        {
            File.Delete(path);
            lua_.PushBoolean(true);
        }
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int DeleteAllGameObject(IntPtr l)
    {
        GameObject[] gos = GameObject.FindObjectsOfType<GameObject>();
        for (int i = 0; i < gos.Length; i++)
        {
            GameObject.DestroyImmediate(gos[i]);
        }
        Debug.Log("destroy all go " +  gos.Length +"   "+ Time.realtimeSinceStartup);
        return 0;
       
    }
    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int HideAllCam(IntPtr l)
    {
        bool isBlack = lua_.ToBoolean(-1);
        Camera[] cams = Camera.allCameras;
        foreach (var cam in cams)
        {
            if (isBlack)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.nearClipPlane = 1000;
            }
            else
            {
                cam.clearFlags = CameraClearFlags.Nothing;
                cam.enabled = false;
            }
        }

        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int NotificationMessage(IntPtr l)
    {
        int hour = lua_.ToInteger(1);
        int minute = lua_.ToInteger(2);
        string message = lua_.ToString(3);
        Debug.Log("hour  " + hour + "     " + minute + " mess " + message);
        DateTime time = new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Utc);
        time = time.ToLocalTime();
        DateTime notificationTime = new DateTime(time.Year, time.Month, time.Day, hour, minute, 0);
        UnityEngine.iOS.LocalNotification localNotification = new UnityEngine.iOS.LocalNotification();
        localNotification.fireDate = notificationTime;
        localNotification.alertBody = message;
        localNotification.applicationIconBadgeNumber = 1;
        localNotification.hasAction = true;
        localNotification.alertAction = "";
        localNotification.repeatCalendar = UnityEngine.iOS.CalendarIdentifier.ChineseCalendar;
        localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;
        localNotification.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
        UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(localNotification);
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int CleanNotification(IntPtr l)
    {

        UnityEngine.iOS.LocalNotification localNotification = new UnityEngine.iOS.LocalNotification();
        localNotification.applicationIconBadgeNumber = -1;
        UnityEngine.iOS.NotificationServices.PresentLocalNotificationNow(localNotification);
        UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
        UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int NotificationMessageAddSeconds(IntPtr l)
    {
        int seconds = lua_.ToInteger(1);
        string message = lua_.ToString(2);
        int minHour = lua_.ToInteger(3);
        int maxHour = lua_.ToInteger(4);
        DateTime time = new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Utc);
        time = time.ToLocalTime();
        time = time.AddSeconds(seconds);
        Debug.Log(time.ToString());
        if (minHour < time.Hour && maxHour > time.Hour)
        {
            UnityEngine.iOS.LocalNotification localNotification = new UnityEngine.iOS.LocalNotification();
            localNotification.fireDate = time;
            localNotification.alertBody = message;
            localNotification.applicationIconBadgeNumber = 1;
            localNotification.hasAction = true;
            localNotification.alertAction = "";
            localNotification.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
            UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(localNotification);
        }
        return 0;
    }

}
