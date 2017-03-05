using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class Client : NetworkBehaviour
{
    public static class MsgTypes
    {
        public static short testMsg = 1000;
        public static short message = 1003;
        public static short commandMsg = 1004;
    };

    GameController gameController;
    NetworkClient myClient;

    // Use this for initialization
    void Start () {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    public override void OnStartClient()
    {
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(MsgTypes.commandMsg, OnCommand);
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("CONNECTED");
        Message msg = new Message();
        msg.id = 0;
        myClient = NetworkClient.allClients[0];
        myClient.Send(MsgTypes.message, msg);
        //Connect();
        /*var msg = netMsg.ReadMessage<IntegerMessage>();
        gameController.playerID = msg.value;
        Debug.Log("Client connected to server with player ID " + msg.value);
        CmdSetPlayerAsReady();*/
    }

    public void OnCommand(NetworkMessage netMsg)
    {
        Command cmd = netMsg.ReadMessage<Command>();
        Debug.Log(gameController.playerID + ": received turn " + cmd.turn + " from " + cmd.pid);
        gameController.ReceiveCommand(cmd);
    }

    [Command]
    void CmdSetPlayerAsReady()
    {
        if (isLocalPlayer)
            return;

        RpcRegisterPlayerAsReady(gameController.playerID);
    }

    [ClientRpc]
    void RpcRegisterPlayerAsReady(int playerID)
    {
        if (isLocalPlayer)
            return;

        gameController.playersReady[playerID] = true;
        Debug.Log("Player " + playerID + " is ready");
    }
    
}
