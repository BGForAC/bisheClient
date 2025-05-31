using System;
using System.Threading;
using UnityEngine;
using WebSocketSharp;
using XLua;

[LuaCallCSharp]
public class WebSocketClient : MonoBehaviour
{
    private static WebSocket ws;
    private static Boolean isConnected = false;

    public static void Connect()
    {
        ws = new WebSocket("ws://localhost:8080/ws");

        ws.OnError += (sender, e) =>
        {
            Debug.LogError("WebSocket Error: " + e.Message);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket connection closed: " + e.Reason);
            if (isConnected)
            {
                isConnected = false;
                Debug.Log("WebSocket connection lost. Attempting to reconnect...");
                Debug.Log("Reconnecting...");
                TryReconnect();
            }
            else
            {
                Debug.Log("WebSocket connection failed to establish.");
                Debug.Log("Reconnecting...");
                TryReconnect();
            }
        };

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket connected successfully.");
            isConnected = true;
            MainThreadDispatcher.Enqueue(() =>
            {
                GameManager.Instance.Init();
            });
        };

        ws.OnMessage += (sender, e) =>
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                switch (e.Data)
                {
                    case "login success":
                        GameManager.Instance.LoginSuccess();
                        break;
                    case "login failed":
                        GameManager.Instance.LoginFailed();
                        break;
                    case "game over":
                        GameManager.Instance.GameOver(GameManager.Instance.account);
                        break;
                    case string message when message.StartsWith("match success "):
                        string[] parts = message.Split(' ');
                        string[] opponentIds = parts[2].Split(',');
                        GameManager.Instance.MatchSuccess(opponentIds);
                        break;
                    case string message when message.StartsWith("teris:"):
                        int blockNumber = int.Parse(message.Substring(6));
                        GameManager.Instance.GenerateNewBlock(GameManager.Instance.account, blockNumber);
                        break;
                    case string message when message.StartsWith("* "):
                        reactTo(message.Substring(2));
                        break;
                    default:
                        Debug.Log("Message from server: " + e.Data);
                        break;
                }

                void reactTo(string msg)
                {
                    string playerId = msg.Split(' ')[0];
                    string action = msg.Split(' ')[1];
                    switch (action)
                    {
                        case "gameOver":
                            GameManager.Instance.GameOver(playerId);
                            break;
                        case "moveLeft":
                            GameManager.Instance.MoveLeft(playerId);
                            break;
                        case "moveRight":
                            GameManager.Instance.MoveRight(playerId);
                            break;
                        case "drop":
                            GameManager.Instance.Drop(playerId);
                            break;
                        case "rotate":
                            GameManager.Instance.Rotate(playerId);
                            break;
                        case "onLand":
                            GameManager.Instance.OnLand(playerId);
                            break;
                        case string msg1 when msg1.StartsWith("clearRow:"):
                            int rowNumber = int.Parse(msg1.Substring(9));
                            GameManager.Instance.ClearRow(playerId, rowNumber);
                            break;
                        case string msg2 when msg2.StartsWith("teris:"):
                            int blockNumber = int.Parse(msg2.Substring(6));
                            GameManager.Instance.GenerateNewBlock(playerId, blockNumber);
                            break;
                        default:
                            Debug.Log("Unknown action: " + action);
                            break;
                    }
                }
            });
        };

        ws.Connect();
    }

    private static void TryReconnect()
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            GameManager.Instance.StartCoroutine(ReconnectCoroutine());
        });
    }

    private static System.Collections.IEnumerator ReconnectCoroutine()
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second before trying to reconnect
        Connect();
    }

    public static void Send(string message)
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Send(message);
        }
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
        }
    }
}