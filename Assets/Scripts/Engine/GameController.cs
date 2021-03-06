﻿using UnityEngine;
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
    // Singleton
    public static GameController Manager;

    // Components
    NetworkManager networkManager;
    InputHoveringUI inputHoveringUI;
    CameraRTS cameraRTS;

    // Turn
    List<Turn> turns = new List<Turn>();
    int currentTurn = 0;
    float timeBetweenTurns = .2f;
    float ticksSinceLastCommunicationTurn = 0.0f;

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

    List<Squad> squads = new List<Squad>(12);
    List<FActor> obstacles = new List<FActor>();

    // Todo: make dynamic based on players connected to game
    private int numPlayers = 2;

    void Awake()
    {
        // Singleton
        Manager = this;
    }

    void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        inputHoveringUI = GetComponent<InputHoveringUI>();
        cameraRTS = Camera.main.GetComponent<CameraRTS>();
        commandToSend = new MessageCommand();

        GameObject[] squadPrefabs = GameObject.FindGameObjectsWithTag("Squad");
        for(int i = 0; i < squadPrefabs.Length; i++)
            squads.Add(squadPrefabs[i].GetComponent<Squad>());


        GameObject[] obstaclePrefabs = GameObject.FindGameObjectsWithTag("Obstacle");
        for (int i = 0; i < obstaclePrefabs.Length; i++)
            obstacles.Add(obstaclePrefabs[i].GetComponent<FActor>());

        for (int i = 0; i < numPlayers; i++)
            playersReady[i] = false;

        // Make sure we don't set a valid number until Server shares our id
        if(multiplayer)
            playerID = -1;
    }

    void FixedUpdate()
    {
        // Wait until game is ready
        if (!gameReady)
        {
            // Don't have to wait for other players
            if (!multiplayer)
            {
                gameReady = true;

                // Skip first
                return;
            }

            // All players are ready
            if (playersReady[0] == true && playersReady[1] == true)
                gameReady = true;
            else
                return;
        }

        ticksSinceLastCommunicationTurn += Time.deltaTime;
        if (ticksSinceLastCommunicationTurn >= timeBetweenTurns)
            RunCommunicationTurn();

        if (currentTurnReceivedFromAllPlayers || !multiplayer)
            RunGameplayTick();
    }

    void RunGameplayTick()
    {
        for (int i = 0; i < squads.Count; i++)
            squads[i].LockStepUpdate();

        for (int i = 0; i < obstacles.Count; i++)
            obstacles[i].LockStepUpdate();
    }

    void RunCommunicationTurn()
    {
        if(!multiplayer)
        {
            gameReady = true;
            ticksSinceLastCommunicationTurn = 0.0f;
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
            ticksSinceLastCommunicationTurn = 0.0f;
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
                GetSquadWithPlayerID(command.pid).MoveToTarget(command.x, command.y);

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

    Squad GetSquadWithPlayerID(int playerID)
    {
        for(int i = 0; i < squads.Count; i++)
        {
            if (squads[i].playerID == playerID)
                return squads[i];
        }

        return null;
    }

    public Squad GetSquadLocalPlayer()
    {
        return squads[playerID];
    }

    public List<FActor> GetObstacles()
    {
        return obstacles;
    }

    public bool IsMultiplayer()
    {
        return multiplayer;
    }

    public bool IsHoveringUI()
    {
        return inputHoveringUI.IsHoveringUI();
    }

    public bool IsValidSquadInput()
    {
        return !inputHoveringUI.IsHoveringUI() && !cameraRTS.IsMoving();
    }
}

