using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class GameController : NetworkBehaviour {

    int currentCommunicationTurn = 0;
    float timeBetweenCommunicationTurns = 1f;
    float timeSinceCommunicationTurn = 0.0f;
    float timeBetweenGameplayTicks = .05f;
    float timeSinceLastGameplayTick = 0.0f;
    Command currentCommunicationTurnCommand = null;
    bool currentCommunicationTurnReceived = true;

    List<Turn> turns = new List<Turn>();

    [HideInInspector]
    public int playerID = -1;

    int numPlayers = 2;

    NetworkClient myClient;

    [HideInInspector]
    public bool gameReady = false;

    void Start()
    {
        currentCommunicationTurnCommand = new Command();
    }

    public void Init()
    {
        myClient = new NetworkClient();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        myClient.RegisterHandler(MyMsgType.Msg, OnPlayerRegistered);
        myClient.RegisterHandler(MyMsgType.commandMessage, OnCommand);
        myClient.Connect("127.0.0.1", 7777);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Init();
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        var msg = new IntegerMessage(0);
        myClient.Send(MyMsgType.msgID, msg);
    }

    public void OnPlayerRegistered(NetworkMessage netMsg)
    {
        var message = netMsg.ReadMessage<IntegerMessage>();
        Debug.Log("Received Player ID:  " + message.value);
    }

    public void OnCommand(NetworkMessage netMsg)
    {
        Command cmd = netMsg.ReadMessage<Command>();
        Debug.Log("Received command: " + cmd.turn);
        ReceiveCommand(cmd);
    }

    void FixedUpdate()
    {
        timeSinceLastGameplayTick += Time.deltaTime;
        timeSinceCommunicationTurn += Time.deltaTime;

        if (timeSinceCommunicationTurn >= timeBetweenCommunicationTurns)
            RunCommunicationTurn();

        if (timeSinceLastGameplayTick >= timeBetweenGameplayTicks && currentCommunicationTurnReceived)
            RunGameplayTick();
    }

    void RunGameplayTick()
    {
        timeSinceLastGameplayTick = 0.0f;
        LockStepUpdate();
    }

    void RunCommunicationTurn()
    {
        if (timeSinceCommunicationTurn >= timeBetweenCommunicationTurns)
        {
            if(ReceivedMessageFromAllPlayersForCurrentTurn() || currentCommunicationTurn < 2)
            {
                gameReady = true;

                // Execute commands from all Players


                // Remove turn with commands
                turns.Remove(GetTurnOfNumber(currentCommunicationTurn));

                SendNextCommand();

                // Reset for next turn
                timeSinceCommunicationTurn = 0.0f;
                currentCommunicationTurn++;
                currentCommunicationTurnReceived = true;
            }

            else
            {
                currentCommunicationTurnReceived = false;
            }
        }
            
    }

    bool ReceivedMessageFromAllPlayersForCurrentTurn()
    {
        Turn turn = GetTurnOfNumber(currentCommunicationTurn);

        if (turn == null)
            return false;

        for(int i = 0; i < numPlayers; i++)
        {
            if (turn.command[i] == null)
                return false;
        }

        return true;
    }

    Turn GetTurnOfNumber(int turn)
    {
        for (int i = 0; i < turns.Count; i++)
        {
            if (turns[i].turn == turn)
                return turns[i];
        }

        return null;
    }

    void LockStepUpdate()
    {
        GameObject[] squads = GameObject.FindGameObjectsWithTag("Squad");
        for(int i = 0; i < squads.Length; i++)
            squads[i].GetComponent<Squad>().LockStepUpdate();

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        for (int i = 0; i < obstacles.Length; i++)
            obstacles[i].GetComponent<FActor>().LockStepUpdate();
    }

    public void ReceiveCommand(Command command)
    {
        bool createTurn = true;
        for(int i = 0; i < turns.Count; i++)
        {
            if (turns[i].turn == command.turn)
            {
                turns[i].command[command.pid] = command;
                createTurn = false;
            }
        }

        if(createTurn)
        {
            Turn turn = new Turn();
            turn.turn = command.turn;
            turn.command = new Command[numPlayers];
            turn.command[command.pid] = command;
            turns.Add(turn);
        }
    }

    // TODO
    // Do this in intervals
    // Check if a command is stored for current intervals
    public void StoreCommand(int cid, int x, int y)
    {
        currentCommunicationTurnCommand.cid = cid;
        currentCommunicationTurnCommand.x = x;
        currentCommunicationTurnCommand.y = y;
    }

    public void SendNextCommand()
    {
        // Prepare
        currentCommunicationTurnCommand.pid = 0; // Todo - relect actual current player ID
        currentCommunicationTurnCommand.turn = currentCommunicationTurn + 2; // Execute two turns in the future

        // Send to self for now
        // ReceiveCommand(currentCommunicationTurnCommand);

        NetworkServer.SendToAll(MyMsgType.commandMessage, currentCommunicationTurnCommand);
    }

    public class MyMessage
    {
        public static short Msg = MsgType.Highest + 1;
    };
}

public class Command : MessageBase
{
    public int turn;
    public int pid; // player id
    public int cid = -1; // command id
    public int x; // optional
    public int y; // optional
}

public class Turn
{
    public int turn;
    public Command[] command;
}
