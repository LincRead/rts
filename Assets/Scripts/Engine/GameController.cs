using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;


public class GameController : MonoBehaviour
{
    int currentTurn = 0;
    float timeBetweenCommunicationTurns = .5f;
    float timeSinceCommunicationTurn = 0.0f;
    float timeBetweenGameplayTicks = .05f;
    float timeSinceLastGameplayTick = 0.0f;
    MessageCommand currentCommand = null;
    bool currentTurnReceivedFromAllPlayers = true;

    List<Turn> turns = new List<Turn>();

    NetworkManagerExtended networkManager;

    [HideInInspector]
    public int playerID = 0;

    int numPlayers = 2;

    [HideInInspector]
    public bool[] playersReady = new bool[2];

    [HideInInspector]
    public bool gameReady = false;

    private bool multiplayer = true;

    void Start()
    {
        networkManager = GetComponent<NetworkManagerExtended>();
        currentCommand = new MessageCommand();

        for(int i = 0; i < numPlayers; i++)
            playersReady[i] = false;

        // Don't have to wait for other players
        if (!multiplayer)
            gameReady = true;

        playerID = 0;
    }

    void Update()
    {
 
    }

    void FixedUpdate()
    {
        if (!gameReady)
        {
            if(playersReady[0] == true && playersReady[1] == true)
                gameReady = true;
            else
                return;
        }

        timeSinceLastGameplayTick += Time.deltaTime;
        timeSinceCommunicationTurn += Time.deltaTime;

        if (timeSinceCommunicationTurn >= timeBetweenCommunicationTurns)
            RunCommunicationTurn();

        if (timeSinceLastGameplayTick >= timeBetweenGameplayTicks 
            && (currentTurnReceivedFromAllPlayers || !multiplayer))
            RunGameplayTick();
    }

    void RunGameplayTick()
    {
        timeSinceLastGameplayTick = 0.0f;
        LockStepUpdate();
    }

    void RunCommunicationTurn()
    {
        if(!multiplayer)
        {
            gameReady = true;
            timeSinceCommunicationTurn = 0.0f;
        }

        else if(ReceivedCommandFromAllPlayersForCurrentTurn() || currentTurn < 2)
        {
            gameReady = true;

            if (currentTurn > 1)
            {
                // Execute command from all Players
                ExecuteTurn(GetTurnOfNumber(currentTurn));

                // Remove turn with commands
                turns.Remove(GetTurnOfNumber(currentTurn));
            }

            SendNextCommand();

            currentTurnReceivedFromAllPlayers = true;

            // Reset for next turn
            timeSinceCommunicationTurn = 0.0f;
            currentTurn++;
        }

        else
        {
            currentTurnReceivedFromAllPlayers = false;
        }     
    }

    void ExecuteTurn(Turn turn)
    {
        for(int i = 0; i < turn.commands.Length; i++)
        {
            ExecuteCommand(turn.commands[i]);
        }
    }

    void ExecuteCommand(MessageCommand command)
    {
        if(command.cid != -1)
            Debug.Log("Executed command " + command.cid + " in turn " + currentTurn);

        switch (command.cid)
        {
            // Move squad to node
            case 0:
                FindSquadWithID(command.pid).MoveToNode(command.x, command.y);

                break;

            default: break;
        }
    }

    bool ReceivedCommandFromAllPlayersForCurrentTurn()
    {
        Turn turn = GetTurnOfNumber(currentTurn);

        if (turn == null)
            return false;

        for(int i = 0; i < numPlayers; i++)
        {
            if (turn.commands[i] == null)
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

    public void HandleReceivedCommand(MessageCommand command)
    {
        bool createTurn = true;
        for(int i = 0; i < turns.Count; i++)
        {
            if (turns[i].turn == command.turn)
            {
                turns[i].commands[command.pid] = command;
                createTurn = false;
            }
        }

        if(createTurn)
        {
            Turn turn = new Turn();
            turn.turn = command.turn;
            turn.commands = new MessageCommand[numPlayers];
            turn.commands[command.pid] = command;
            turns.Add(turn);
        }
    }

    // TODO
    // Do this in intervals
    // Check if a command is stored for current intervals
    public void SetCommand(int cid, int x, int y)
    {
        currentCommand.cid = cid;
        currentCommand.x = x;
        currentCommand.y = y;
    }

    public void SendNextCommand()
    {
        // Prepare
        currentCommand.pid = playerID; // Todo - relect actual current player ID
        currentCommand.turn = currentTurn + 2; // Execute two turns in the future

        if(currentCommand.cid != -1)
            Debug.Log("Send command " + currentCommand.cid + " to exexute in turn " + currentCommand.turn);

        networkManager.SendCommandFromClient(currentCommand);

        // Reset for next turn
        currentCommand.cid = -1;
    }

    public bool IsMultiplayer()
    {
        return multiplayer;
    }

    Squad FindSquadWithID(int playerID)
    {
        GameObject[] squads = GameObject.FindGameObjectsWithTag("Squad");
        for(int i = 0; i < squads.Length; i++)
        {
            Squad squad = squads[i].GetComponent<Squad>();
            if (squad.playerID == playerID)
                return squad;
        }

        return null;
    }
}

public class Turn
{
    public int turn;
    public MessageCommand[] commands;
}

