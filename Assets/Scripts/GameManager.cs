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
    public TextAsset luaETerisScript;
    public TextAsset luaGameManagerScript;
    public TextAsset luaEGameManagerScript;

    private LuaTable scriptScopeTable;

    private Action luaMoveLeft;
    private Action luaMoveRight;
    private Action luaDrop;
    private Action luaCancelDrop;
    private Action luaRotate;
    private Action luaUpdate;
    private Action luaInit;
    public static Action luaGenrateNewBlock;

    public static Action luaEMoveLeft;
    public static Action luaEMoveRight;
    public static Action luaEDrop;
    public static Action luaERotate;
    public static Action luaEClearRow;
    public static Action luaEGenerateNewBlock;
    public static Action luaEOnLand;
    private Action luaEInit;


    void OnEnable()
    {
        WebSocketClient.Connect();
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
        luaEnv.DoString(luaETerisScript.text, luaETerisScript.name, scriptScopeTable);
        luaEnv.DoString(luaGameManagerScript.text, luaGameManagerScript.name, scriptScopeTable);
        luaEnv.DoString(luaEGameManagerScript.text, luaEGameManagerScript.name, scriptScopeTable);

        scriptScopeTable.Get("moveLeft", out luaMoveLeft);
        scriptScopeTable.Get("moveRight", out luaMoveRight);
        scriptScopeTable.Get("drop", out luaDrop);
        scriptScopeTable.Get("cancelDrop", out luaCancelDrop);
        scriptScopeTable.Get("rotate", out luaRotate);
        scriptScopeTable.Get("update", out luaUpdate);
        scriptScopeTable.Get("generateNewBlock", out luaGenrateNewBlock);

        scriptScopeTable.Get("eMoveLeft", out luaEMoveLeft);
        scriptScopeTable.Get("eMoveRight", out luaEMoveRight);
        scriptScopeTable.Get("eDrop", out luaEDrop);
        scriptScopeTable.Get("eRotate", out luaERotate);
        scriptScopeTable.Get("eClearRow", out luaEClearRow);
        scriptScopeTable.Get("eGenerateNewBlock", out luaEGenerateNewBlock);
        scriptScopeTable.Get("eOnLand", out luaEOnLand);

        scriptScopeTable.Get("init", out luaInit);
        scriptScopeTable.Get("eInit", out luaEInit);

        luaInit?.Invoke();
        luaEInit?.Invoke();
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
        MainThreadDispatcher.Update();
        luaUpdate?.Invoke(); 
    }

    public static void GenerateNewBlock()
    {
        luaGenrateNewBlock?.Invoke();
    }

    internal static void EMoveLeft()
    {
        luaEMoveLeft?.Invoke();
    }

    internal static void EMoveRight()
    {
        luaEMoveRight?.Invoke();
    }

    internal static void EDrop()
    {
        luaEDrop?.Invoke();
    }

    internal static void ERotate()
    {
        luaERotate?.Invoke();
    }

    internal static void EClearRow()
    {
        luaEClearRow?.Invoke();
    }

    internal static void EGenerateNewBlock()
    {
        luaEGenerateNewBlock?.Invoke();
    }

    internal static void EOnLand()
    {
        luaEOnLand?.Invoke();
    }
}
