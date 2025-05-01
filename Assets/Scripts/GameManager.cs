using System.Collections;
using System.Collections.Generic;
using XLua;
using UnityEngine;
using System;

[CSharpCallLua]
public class GameManager : MonoBehaviour
{
    internal static LuaEnv luaEnv = new LuaEnv();
    public TextAsset luaGlobalScript;
    public TextAsset luaTerisScript;
    public TextAsset luaGameManagerScript;

    private LuaTable scriptScopeTable;

    private Action luaMoveLeft;
    private Action luaMoveRight;
    private Action luaDrop;
    private Action luaCancelDrop;
    private Action luaRotate;
    private Action luaUpdate;
    private Action luaInit;

    void OnEnable()
    {
        scriptScopeTable = luaEnv.NewTable();

        using (LuaTable meta = luaEnv.NewTable())
        {
            meta.Set("__index", luaEnv.Global);
            scriptScopeTable.SetMetaTable(meta);
        }

        PlayerInput.onMoveLeft += MoveLeft;
        PlayerInput.onMoveRight += MoveRight;
        PlayerInput.onDrop += Drop;
        PlayerInput.onCancelDrop += CancelDrop;
        PlayerInput.onRotate += Rotate;    

        luaEnv.DoString(luaGlobalScript.text, luaGlobalScript.name, scriptScopeTable);
        luaEnv.DoString(luaTerisScript.text, luaTerisScript.name, scriptScopeTable);
        luaEnv.DoString(luaGameManagerScript.text, luaGameManagerScript.name, scriptScopeTable);

        scriptScopeTable.Get("moveLeft", out luaMoveLeft);
        scriptScopeTable.Get("moveRight", out luaMoveRight);
        scriptScopeTable.Get("drop", out luaDrop);
        scriptScopeTable.Get("cancelDrop", out luaCancelDrop);
        scriptScopeTable.Get("rotate", out luaRotate);
        scriptScopeTable.Get("update", out luaUpdate);

        scriptScopeTable.Get("init", out luaInit);
        luaInit?.Invoke();
    }

    void MoveLeft()
    {
        luaMoveLeft?.Invoke();
    }

    void MoveRight()
    {
        luaMoveRight?.Invoke();
    }

    void Drop()
    {
        luaDrop?.Invoke();
    }

    void CancelDrop()
    {
        luaCancelDrop?.Invoke();
    }

    void Rotate()
    {
        luaRotate?.Invoke();
    }

    void Update()
    {
        luaUpdate?.Invoke(); 
    }
}
