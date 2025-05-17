using UnityEngine;
using WebSocketSharp;
using XLua;

[LuaCallCSharp]
public class WebSocketClient : MonoBehaviour
{
    private static WebSocket ws;
    public static int blockNumber = -1;
    public static int eBlockNumber = -1;
    public static int eRowNumber = -1;

    public static void Connect()
    {
        // Connect to the WebSocket server
        ws = new WebSocket("ws://localhost:8080/ws");

        ws.OnMessage += (sender, e) =>
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                switch (e.Data)
                {
                    case string message when message.StartsWith("teris:"):
                        blockNumber = int.Parse(message.Substring(6));
                        GameManager.GenerateNewBlock();
                        break;
                    case "gameOver":
                        GameManager.GameOver()
                        break;
                    case string message when message.StartsWith("e "):
                        reactTo(message.Substring(2));
                        break;
                    default:
                        Debug.Log("Message from server: " + e.Data);
                        break;
                }

                void reactTo(string message)
                {
                    switch (message)
                    {
                        case "gameOver":
                            GameManager.EGameOver();
                            break;
                        case "moveLeft":
                            GameManager.EMoveLeft();
                            break;
                        case "moveRight":
                            GameManager.EMoveRight();
                            break;
                        case "drop":
                            GameManager.EDrop();
                            break;
                        case "rotate":
                            GameManager.ERotate();
                            break;
                        case "onLand":
                            GameManager.EOnLand();
                            break;    
                        case string msg1 when msg1.StartsWith("clearRow:"):
                            eRowNumber = int.Parse(msg1.Substring(9));
                            GameManager.EClearRow();
                            break;
                        case string msg2 when msg2.StartsWith("teris:"):
                            eBlockNumber = int.Parse(msg2.Substring(6));
                            GameManager.EGenerateNewBlock();
                            break;
                        default:
                            Debug.Log("Unknown message: " + message);
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
        // Close the WebSocket connection
        if (ws != null)
        {
            ws.Close();
        }
    }
}