//======================================================
//  Core Code---Singleton
//  2011.12.14 created by Wangnannan
//======================================================
using System.Diagnostics;
public class Singleton<T> where T: new()
{  
    protected Singleton(){Debug.Assert(null==instance);}
    protected static T instance = new T();
    public static T Instance
    {
        get{return instance;}
    }
}