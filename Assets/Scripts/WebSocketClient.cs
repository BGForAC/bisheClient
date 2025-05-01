using UnityEngine;
using WebSocketSharp;
using XLua;

[LuaCallCSharp]
public class WebSocketClient : MonoBehaviour
{
    private static WebSocket ws;

    void Start()
    {
        // Connect to the WebSocket server
        ws = new WebSocket("ws://localhost:8080/ws");

        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Message from server: " + e.Data);
        };

        ws.Connect();
        ws.Send("Hello from Unity!");
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