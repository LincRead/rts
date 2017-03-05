using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public static class MyMsgType
{
    public static short testMsg = 1000;
    public static short msg = 1003;
    public static short commandMsg = 1004;
    public static short setPlayerIDMsg = 1005;
};

public class LockstepManager : NetworkManager {

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler(MyMsgType.msg, OnMessage);
    }

    class ID : MessageBase
    {
        public int id;
    }

    int highestPlayerID = 0; // Host starts with 0
    void OnMessage(NetworkMessage netMsg)
    {
        var messageID = netMsg.ReadMessage<IntegerMessage>();

        // Request to know Player ID
        if(messageID.value == 0)
        {
            Debug.Log("Sending player ID " + highestPlayerID + " to client that connected");
            ID id = new ID();
            id.id = highestPlayerID;
            netMsg.conn.Send(MyMsgType.testMsg, id);
            highestPlayerID++;
        }
    }

    public override void OnServerConnect(NetworkConnection Conn)
    {
        base.OnServerConnect(Conn);
    }
}
