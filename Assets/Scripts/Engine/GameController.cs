﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

    int currentCommunicationTurn = 0;
    float timeBetweenCommunicationTurns = 1f;
    float timeSinceCommunicationTurn = 0.0f;
    float timeBetweenGameplayTicks = .05f;
    float timeSinceLastGameplayTick = 0.0f;
    Command currentCommunicationTurnCommand = null;
    bool currentCommunicationTurnReceived = true;

    List<Turn> turns = new List<Turn>();

    int numPlayers = 1;

    void Start () {
        currentCommunicationTurnCommand = new Command();
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
        ReceiveCommand(currentCommunicationTurnCommand);

    }
}

public class Command
{
    public int turn = -1;
    public int pid; // player id
    public int cid; // command id
    public int x; // optional
    public int y; // optional
}

public class Turn
{
    public int turn;
    public Command[] command;
}
