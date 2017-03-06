using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Turn
{
    public int turn;
    public MessageCommand[] commands;
}

public class GameController : MonoBehaviour
{
    // Components
    NetworkManagerExtended networkManager;

    // Turn
    List<Turn> turns = new List<Turn>();
    int currentTurn = 0;
    float timeBetweenTurns = .5f;
    float timeSinceLastTurn = 0.0f;

    // Gameplay tick
    float timeBetweenGameplayTicks = .05f;
    float timeSinceLastGameplayTick = 0.0f;

    // Command
    MessageCommand commandToSend = null;
    bool currentTurnReceivedFromAllPlayers = true;

    [HideInInspector]
    public int playerID = 0;

    [HideInInspector]
    public bool[] playersReady = new bool[2];

    [HideInInspector]
    public bool gameReady = false;

    private bool multiplayer = false;

    // Todo: make dynamic based on players connected to game
    private int numPlayers = 2;

    void Start()
    {
        networkManager = GetComponent<NetworkManagerExtended>();
        commandToSend = new MessageCommand();

        for (int i = 0; i < numPlayers; i++)
            playersReady[i] = false;

        // Don't have to wait for other players
        if (!multiplayer)
            gameReady = true;
        // Make sure we don't set a valid number until Server shares our id
        else
            playerID = -1;
    }

    void FixedUpdate()
    {
        // Wait until game is ready
        if (!gameReady)
        {
            // All players are ready
            if(playersReady[0] == true && playersReady[1] == true)
                gameReady = true;
            else
                return;
        }

        timeSinceLastGameplayTick += Time.deltaTime;
        timeSinceLastTurn += Time.deltaTime;

        if (timeSinceLastTurn >= timeBetweenTurns)
            RunCommunicationTurn();

        if (timeSinceLastGameplayTick >= timeBetweenGameplayTicks 
            && (currentTurnReceivedFromAllPlayers || !multiplayer))
            RunGameplayTick();
    }

    void RunGameplayTick()
    {
        timeSinceLastGameplayTick = 0.0f;

        GameObject[] squads = GameObject.FindGameObjectsWithTag("Squad");
        for (int i = 0; i < squads.Length; i++)
            squads[i].GetComponent<Squad>().LockStepUpdate();

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        for (int i = 0; i < obstacles.Length; i++)
            obstacles[i].GetComponent<FActor>().LockStepUpdate();
    }

    void RunCommunicationTurn()
    {
        if(!multiplayer)
        {
            gameReady = true;
            timeSinceLastTurn = 0.0f;
        }

        // Sending command two turns in the future, so skip first two turns
        else if(ReceivedCommandFromAllPlayersForCurrentTurn() || currentTurn < 2)
        {
            gameReady = true;

            if (currentTurn > 1)
            {
                // Execute command from all players
                ExecuteTurn(GetTurnOfNumber(currentTurn));

                // Remove turn with commands
                turns.Remove(GetTurnOfNumber(currentTurn));
            }

            SendNextCommand();

            currentTurnReceivedFromAllPlayers = true;

            // Reset for next turn
            timeSinceLastTurn = 0.0f;
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
        switch (command.cid)
        {
            // Move squad to Node
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
    public void SetNextCommand(int cid, int x, int y)
    {
        commandToSend.cid = cid;
        commandToSend.x = x;
        commandToSend.y = y;
    }

    public void SendNextCommand()
    {
        // Prepare
        commandToSend.pid = playerID; // Todo - relect actual current player ID
        commandToSend.turn = currentTurn + 2; // Execute two turns in the future

        networkManager.SendCommandFromClient(commandToSend);

        // Reset for next turn
        commandToSend.cid = -1;
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

    public bool IsMultiplayer()
    {
        return multiplayer;
    }
}

