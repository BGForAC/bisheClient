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
    public TextAsset luaTerisGameManagerScript;
    public TextAsset luaGameManagerScript;

    private LuaTable scriptScopeTable;

    private static Action<string> luaMoveLeft;
    private static Action<string> luaMoveRight;
    private static Action luaAcc;
    private static Action luaCancelAcc;
    private static Action<string> luaDrop;
    private static Action<string> luaRotate;
    private static Action luaUpdate;
    public static Action<string, int> luaGenrateNewBlock;
    private static Action<string, string[]> luaInit;
    private static Action<string> luaOnLand;
    private static Action<string, int> luaClearRow;
    private static Action<string> luaGameOver;

    public static string account;

    private static GameObject loginPanel;
    private static GameObject accountInputField;
    private static GameObject loginButton;
    private static GameObject waitPanel;
    private static GameObject waitName;
    private static GameObject waitButton;
    private static GameObject waitButtonTxt;

    void BtnLogin()
    {
        account = accountInputField.GetComponent<UnityEngine.UI.InputField>().text;
        if (string.IsNullOrEmpty(account))
        {
            Debug.Log("请输入账号");
            return;
        }
        WebSocketClient.Send("login " + account);
    }

    static void BtnStartMatch()
    {
        WebSocketClient.Send("start match");
        waitButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(BtnStartMatch);
        waitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(BtnCancelMatch);
        waitButtonTxt.GetComponent<UnityEngine.UI.Text>().text = "取消匹配";
    }

    static void BtnCancelMatch()
    {
        WebSocketClient.Send("cancel match");
        waitButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(BtnCancelMatch);
        waitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(BtnStartMatch);
        waitButtonTxt.GetComponent<UnityEngine.UI.Text>().text = "开始匹配";
    }

    public static void LoginSuccess()
    {
        loginPanel.SetActive(false);
        waitPanel.SetActive(true);
        waitName.GetComponent<UnityEngine.UI.Text>().text = "玩家：" + account;
        waitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(BtnStartMatch);
        print("玩家：" + account + " 登录成功");
    }
    
    public static void LoginFailed()
    {
    }

    public static void MatchSuccess(string[] opponentIds)
    {
        waitPanel.SetActive(false);
        print("匹配成功，对手ID：" + string.Join(", ", opponentIds));
        luaInit?.Invoke(account, opponentIds);

        PlayerInput.onMoveLeft += CMoveLeft;
        PlayerInput.onMoveRight += CMoveRight;
        PlayerInput.onAcc += CAcc;
        PlayerInput.onCancelAcc += CCancelAcc;
        PlayerInput.onRotate += CRotate;    
    }

    void OnEnable()
    {
        Application.runInBackground = true;
        WebSocketClient.Connect();
        loginPanel = GameObject.Find("UI").transform.Find("LoginPanel").gameObject;
        accountInputField = loginPanel.transform.Find("trsContent").Find("inputName").gameObject;
        loginButton = loginPanel.transform.Find("trsContent").Find("btnLogin").gameObject;
        waitPanel = GameObject.Find("UI").transform.Find("WaitPanel").gameObject;
        waitName = waitPanel.transform.Find("trsContent").Find("txtName").gameObject;
        waitButton = waitPanel.transform.Find("trsContent").Find("btnMatch").gameObject;
        waitButtonTxt = waitButton.transform.Find("txtMatch").gameObject;
        // 为按钮注册点击事件
        loginButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(BtnLogin);
        scriptScopeTable = luaEnv.NewTable();

        using (LuaTable meta = luaEnv.NewTable())
        {
            meta.Set("__index", luaEnv.Global);
            scriptScopeTable.SetMetaTable(meta);
        }

        luaEnv.DoString(luaGlobalScript.text, luaGlobalScript.name, scriptScopeTable);
        luaEnv.DoString(luaTerisScript.text, luaTerisScript.name, scriptScopeTable);
        luaEnv.DoString(luaTerisGameManagerScript.text, luaTerisGameManagerScript.name, scriptScopeTable);
        luaEnv.DoString(luaGameManagerScript.text, luaGameManagerScript.name, scriptScopeTable);

        scriptScopeTable.Get("moveLeft", out luaMoveLeft);
        scriptScopeTable.Get("moveRight", out luaMoveRight);
        scriptScopeTable.Get("drop", out luaDrop);
        scriptScopeTable.Get("rotate", out luaRotate);
        scriptScopeTable.Get("update", out luaUpdate);
        scriptScopeTable.Get("generateNewBlock", out luaGenrateNewBlock);
        scriptScopeTable.Get("onLand", out luaOnLand);
        scriptScopeTable.Get("clearRow", out luaClearRow);
        scriptScopeTable.Get("acc", out luaAcc);
        scriptScopeTable.Get("cancelAcc", out luaCancelAcc);
        scriptScopeTable.Get("gameOver", out luaGameOver);

        scriptScopeTable.Get("init", out luaInit);
    }

    private static void CMoveLeft()
    {
        MoveLeft(account);
    }
    private static void CMoveRight()
    {
        MoveRight(account);
    }
    private static void CAcc()
    {
        luaAcc?.Invoke();
    }

    private static void CCancelAcc()
    {
        luaCancelAcc?.Invoke();
    }

    private static void CRotate()
    {
        luaRotate?.Invoke(account);
    }

    public static void GameOver(string playerId)
    {
        luaGameOver?.Invoke(playerId);
        if (playerId == account)
        {
            PlayerInput.onMoveLeft -= CMoveLeft;
            PlayerInput.onMoveRight -= CMoveRight;
            PlayerInput.onAcc -= CAcc;
            PlayerInput.onCancelAcc -= CCancelAcc;
            PlayerInput.onRotate -= CRotate;
        }
    }

    public static void MoveLeft(string playerId)
    {
        luaMoveLeft?.Invoke(playerId);
    }

    public static void MoveRight(string playerId)
    {
        luaMoveRight?.Invoke(playerId);
    }

    public static void Drop(string playerId)
    {
        luaDrop?.Invoke(playerId);
    }

    public static void Rotate(string playerId)
    {
        luaRotate?.Invoke(playerId);
    }

    public static void OnLand(string playerId)
    {
        luaOnLand?.Invoke(playerId);
    }

    public static void ClearRow(string playerId, int rowNumber)
    {
        luaClearRow?.Invoke(playerId, rowNumber);
    }

    void Update()
    {
        MainThreadDispatcher.Update();
        luaUpdate?.Invoke();
    }

    public static void GenerateNewBlock(string playerId, int number)
    {
        luaGenrateNewBlock?.Invoke(playerId, number);
    }
}
