using UnityEngine;
using UnityEngine.Networking;

public class NetworkManagerExtended : MonoBehaviour
{
    public bool isAtStartup = true;
    public int numPlayers = 0;
    NetworkClient myClient;

    void Update()
    {
        if (isAtStartup)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                SetupClient();
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                SetupServer();
                SetupLocalClient();
            }
        }
    }

    void OnGUI()
    {
        if (isAtStartup)
        {
            GUI.Label(new Rect(2, 30, 150, 100), "Press B for host");
            GUI.Label(new Rect(2, 50, 150, 100), "Press C for join");
        }
    }

    // Create a server and listen on a port
    public void SetupServer()
    {
        Debug.Log("Setup server");
        NetworkServer.Listen(4444);
        NetworkServer.RegisterHandler(MsgType.Connect, OnServerConnect);
        isAtStartup = false;
    }

    // Create a client and connect to the server port
    public void SetupClient()
    {
        Debug.Log("Setup client");
        myClient = new NetworkClient();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        myClient.Connect("127.0.0.1", 4444);
        isAtStartup = false;
    }

    // Create a local client and connect to the local server
    public void SetupLocalClient()
    {
        Debug.Log("Host connected as client");
        myClient = ClientScene.ConnectLocalServer();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        isAtStartup = false;
    }

    public void OnServerConnect(NetworkMessage networkMessage)
    {
        Debug.Log("Client connected, connections=" + NetworkServer.connections.Count);

        // Todo give player id to connected...
    }

    // Client function
    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Connected to server");
    }
}