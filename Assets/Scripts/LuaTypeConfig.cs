using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;

public static class LuaTypeConfig
{
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>()
    {
        typeof(Action),
        typeof(Action<string>),
        typeof(Action<string, int>),
        typeof(Action<string, string[]>),
        typeof(UnityEngine.Events.UnityAction)
    };
}
