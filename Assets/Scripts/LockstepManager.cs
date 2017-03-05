using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public static class MyMsgType
{
    public static short Msg = 1000;
    public static short msgID = 1003;
    public static short commandMessage = 1004;
    public static short playerID = 1005;
};

public class LockstepManager : NetworkManager {

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler(MyMsgType.msgID, OnMessage);
    }

    class ID : MessageBase
    {
        public int id;
    }

    int highestPlayerID = 0; // Host starts with 0
    void OnMessage(NetworkMessage netMsg)
    {
        Debug.Log("Sending player ID " + highestPlayerID + " to client that connected");
        ID id = new ID();
        id.id = highestPlayerID;
        netMsg.conn.Send(MyMsgType.Msg, id);
        highestPlayerID++;
    }

    public override void OnServerConnect(NetworkConnection Conn)
    {
        base.OnServerConnect(Conn);
        Debug.Log(Conn.connectionId);
    }
}
