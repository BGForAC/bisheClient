using System.Collections;
using System.Collections.Generic;
using XLua;
using UnityEngine;
using System;
using UnityEngine.UIElements;

[CSharpCallLua]
public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    internal LuaEnv luaEnv = new LuaEnv();
    public TextAsset luaGlobalScript;
    public TextAsset luaTerisScript;
    public TextAsset luaTerisGameManagerScript;
    public TextAsset luaGameManagerScript;

    private LuaTable scriptScopeTable;

    private Action<string> luaMoveLeft;
    private Action<string> luaMoveRight;
    private Action luaAcc;
    private Action luaCancelAcc;
    private Action<string> luaDrop;
    private Action<string> luaRotate;
    private Action luaUpdate;
    public Action<string, int> luaGenrateNewBlock;
    private Action<string, string[]> luaInit;
    private Action<string> luaOnLand;
    private Action<string, int> luaClearRow;
    private Action<string> luaGameOver;

    public string account;

    private GameObject loginPanel;
    private GameObject accountInputField;
    private GameObject loginButton;
    private GameObject waitPanel;
    private GameObject waitName;
    private GameObject waitButton;
    private GameObject waitButtonTxt;
    private GameObject connectStateTxt;

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

    void BtnStartMatch()
    {
        WebSocketClient.Send("start match");
        waitButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(BtnStartMatch);
        waitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(BtnCancelMatch);
        waitButtonTxt.GetComponent<UnityEngine.UI.Text>().text = "取消匹配";
    }

    void BtnCancelMatch()
    {
        WebSocketClient.Send("cancel match");
        waitButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(BtnCancelMatch);
        waitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(BtnStartMatch);
        waitButtonTxt.GetComponent<UnityEngine.UI.Text>().text = "开始匹配";
    }

    public void LoginSuccess()
    {
        loginPanel.SetActive(false);
        waitPanel.SetActive(true);
        waitName.GetComponent<UnityEngine.UI.Text>().text = "玩家：" + account;
        waitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(BtnStartMatch);
        print("玩家：" + account + " 登录成功");
    }

    public void LoginFailed()
    {
    }

    public void MatchSuccess(string[] opponentIds)
    {
        waitPanel.SetActive(false);
        print("匹配成功，对手ID：" + string.Join(", ", opponentIds));
        luaInit?.Invoke(account, opponentIds);

        PlayerInput.onMoveLeft += CMoveLeft;
        PlayerInput.onMoveRight += CMoveRight;
        PlayerInput.onAcc += CAcc;
        PlayerInput.onCancelAcc += CCancelAcc;
        PlayerInput.onRotate += CRotate;

        waitButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(BtnCancelMatch);
        waitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(BtnStartMatch);
        waitButtonTxt.GetComponent<UnityEngine.UI.Text>().text = "开始匹配";
    }

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        Screen.SetResolution(640, 360, false);
        loginPanel = GameObject.Find("UI").transform.Find("LoginPanel").gameObject;
        accountInputField = loginPanel.transform.Find("trsContent").Find("inputName").gameObject;
        loginButton = loginPanel.transform.Find("trsContent").Find("btnLogin").gameObject;
        connectStateTxt = loginPanel.transform.Find("trsContent").Find("txtConnectState").gameObject;
        waitPanel = GameObject.Find("UI").transform.Find("WaitPanel").gameObject;
        waitName = waitPanel.transform.Find("trsContent").Find("txtName").gameObject;
        waitButton = waitPanel.transform.Find("trsContent").Find("btnMatch").gameObject;
        waitButtonTxt = waitButton.transform.Find("txtMatch").gameObject;

        Application.runInBackground = true;
        loginButton.GetComponent<UnityEngine.UI.Button>().interactable = false;

        WebSocketClient.Connect();
    }

    public void Init()
    {
        // 为按钮注册点击事件
        loginButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        loginButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(BtnLogin);
        connectStateTxt.GetComponent<UnityEngine.UI.Text>().text = "连接状态：已连接";

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

    private void CMoveLeft()
    {
        MoveLeft(account);
    }
    private void CMoveRight()
    {
        MoveRight(account);
    }
    private void CAcc()
    {
        luaAcc?.Invoke();
    }

    private void CCancelAcc()
    {
        luaCancelAcc?.Invoke();
    }

    private void CRotate()
    {
        luaRotate?.Invoke(account);
    }

    public void GameOver(string playerId)
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

    public void MoveLeft(string playerId)
    {
        luaMoveLeft?.Invoke(playerId);
    }

    public void MoveRight(string playerId)
    {
        luaMoveRight?.Invoke(playerId);
    }

    public void Drop(string playerId)
    {
        luaDrop?.Invoke(playerId);
    }

    public void Rotate(string playerId)
    {
        luaRotate?.Invoke(playerId);
    }

    public void OnLand(string playerId)
    {
        luaOnLand?.Invoke(playerId);
    }

    public void ClearRow(string playerId, int rowNumber)
    {
        luaClearRow?.Invoke(playerId, rowNumber);
    }

    void Update()
    {
        MainThreadDispatcher.Update();
        luaUpdate?.Invoke();
    }

    public void GenerateNewBlock(string playerId, int number)
    {
        luaGenrateNewBlock?.Invoke(playerId, number);
    }
}
