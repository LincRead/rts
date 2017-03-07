using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    private bool isAtStartup = true;

    NetworkClient myClient;
    GameController gameController;

    void Start()
    {
        gameController = GetComponent<GameController>();

        if (!gameController.IsMultiplayer())
            isAtStartup = false;
    }

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
        NetworkServer.Listen(4444);
        NetworkServer.RegisterHandler(MsgType.Connect, OnServerConnect);
        NetworkServer.RegisterHandler(MsgTypes.MessagePlayerReady, OnServerReceivePlayerReady);
        NetworkServer.RegisterHandler(MsgTypes.MessageCommand, OnServerReceiveCommand);
        isAtStartup = false;
    }

    // Create a client and connect to the server port
    public void SetupClient()
    {
        myClient = new NetworkClient();
        AddClientHandlers();
        myClient.Connect("127.0.0.1", 4444);
        isAtStartup = false;
    }

    // Create a local client and connect to the local server
    public void SetupLocalClient()
    {
        myClient = ClientScene.ConnectLocalServer();
        AddClientHandlers();
        isAtStartup = false;
    }
    void AddClientHandlers()
    {
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        myClient.RegisterHandler(MsgTypes.MessagePlayerID, OnClientReceivePlayerID);
        myClient.RegisterHandler(MsgTypes.MessagePlayerReady, OnClientReceivePlayerReady);
        myClient.RegisterHandler(MsgTypes.MessageCommand, OnClientReceiveCommand);
    }

    // --- SERVER --- ///

    public void OnServerConnect(NetworkMessage networkMessage)
    {
        MessagePlayerID msg = new MessagePlayerID();
        msg.pid = NetworkServer.connections.Count - 1;
        Debug.Log("Client " + msg.pid + " connected to the server");
        NetworkServer.SendToClient(networkMessage.conn.connectionId, MsgTypes.MessagePlayerID, msg);

        // Send a message about clients that were ready before this client connected
        for (int i = 0; i < gameController.playersReady.Length; i++)
        {
            if (gameController.playersReady[i] == true)
            {
                MessagePlayerReady messageSend = new MessagePlayerReady();
                messageSend.pid = i;
                NetworkServer.SendToClient(networkMessage.conn.connectionId, MsgTypes.MessagePlayerReady, messageSend);
            }
        }
    }

    public void OnServerReceivePlayerReady(NetworkMessage networkMessage)
    {
        MessagePlayerReady msg = networkMessage.ReadMessage<MessagePlayerReady>();
        NetworkServer.SendToAll(MsgTypes.MessagePlayerReady, msg);
    }

    public void OnServerReceiveCommand(NetworkMessage networkMessage)
    {
        MessageCommand msg = networkMessage.ReadMessage<MessageCommand>();
        NetworkServer.SendToAll(MsgTypes.MessageCommand, msg);
    }

    // --- CLIENT --- ///

    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Connected to server");
    }

    public void SendCommandFromClient(MessageCommand command)
    {
        myClient.Send(MsgTypes.MessageCommand, command);
    }

    public void OnClientReceivePlayerID(NetworkMessage networkMessage)
    {
        MessagePlayerID messageReceived = networkMessage.ReadMessage<MessagePlayerID>();
        gameController.playerID = messageReceived.pid;

        Debug.Log("Client set Player ID to " + gameController.playerID);

        MessagePlayerReady messageSend = new MessagePlayerReady();
        messageSend.pid = gameController.playerID;
        myClient.Send(MsgTypes.MessagePlayerReady, messageSend);
    }

    public void OnClientReceivePlayerReady(NetworkMessage networkMessage)
    {
        MessagePlayerReady msg = networkMessage.ReadMessage<MessagePlayerReady>();
        gameController.playersReady[msg.pid] = true;
        Debug.Log("Player " + msg.pid + " is Ready");
    }

    public void OnClientReceiveCommand(NetworkMessage networkMessage)
    {
        MessageCommand command = networkMessage.ReadMessage<MessageCommand>();
        gameController.HandleReceivedCommand(command);
    }
}

public static class MsgTypes
{
    public static short Message = 1000;
    public static short MessageCommand = 1001;
    public static short MessagePlayerID = 1002;
    public static short MessagePlayerReady = 1003;
};

public class Message : MessageBase
{
    public int id;
}

public class MessageCommand : MessageBase
{
    public int turn;
    public int pid; // player id
    public int cid = -1; // command id
    public int x; // optional
    public int y; // optional
}

public class MessagePlayerID : MessageBase
{
    public int pid;
}

public class MessagePlayerReady : MessageBase
{
    public int pid;
}