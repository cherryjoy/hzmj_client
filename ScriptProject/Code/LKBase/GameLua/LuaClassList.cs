/*
 * Game class and Unity class will be register here
 * 
 * 
 * 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LuaClassList
{
    public static Dictionary<Type, FuncInfo[]> classes = new Dictionary<Type, FuncInfo[]>();
	public static void Init(){
        classes.Add(typeof(Register), new FuncInfo[] {
            new FuncInfo("RegisterClass","RegisterClass",
                new Type[]{typeof(String)},true
                ),
            new FuncInfo("RegisterWrapper","RegisterWrapper",
                new Type[]{typeof(bool)},true
                ),
            });

    }
}
