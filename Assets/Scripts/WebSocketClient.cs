using UnityEngine;
using WebSocketSharp;
using XLua;

[LuaCallCSharp]
public class WebSocketClient : MonoBehaviour
{
    private static WebSocket ws;

    public static void Connect()
    {
        ws = new WebSocket("ws://localhost:8080/ws");

        ws.OnMessage += (sender, e) =>
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                switch (e.Data)
                {
                    case "login success":
                        GameManager.LoginSuccess();
                        break;
                    case "login failed":
                        GameManager.LoginFailed();
                        break;
                    case "game over":
                        GameManager.GameOver(GameManager.account);
                        break;
                    case string message when message.StartsWith("match success "):
                        string[] parts = message.Split(' ');
                        string[] opponentIds = parts[2].Split(',');
                        GameManager.MatchSuccess(opponentIds);
                        break; 
                    case string message when message.StartsWith("teris:"):
                        int blockNumber = int.Parse(message.Substring(6));
                        GameManager.GenerateNewBlock(GameManager.account, blockNumber);
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
                            GameManager.GameOver(playerId);
                            break;
                        case "moveLeft":
                            GameManager.MoveLeft(playerId);
                            break;
                        case "moveRight":
                            GameManager.MoveRight(playerId);
                            break;
                        case "drop":
                            GameManager.Drop(playerId);
                            break;
                        case "rotate":
                            GameManager.Rotate(playerId);
                            break;
                        case "onLand":
                            GameManager.OnLand(playerId);
                            break;
                        case string msg1 when msg1.StartsWith("clearRow:"):
                            int rowNumber = int.Parse(msg1.Substring(9));
                            GameManager.ClearRow(playerId, rowNumber);
                            break;
                        case string msg2 when msg2.StartsWith("teris:"):
                            int blockNumber = int.Parse(msg2.Substring(6));
                            GameManager.GenerateNewBlock(playerId, blockNumber);
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