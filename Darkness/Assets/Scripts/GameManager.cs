using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum Gate
    {
        Start,
        Gate1,
        Gate2,
        Gate3,
    }

    enum Tasks
    {
        Fuse,
        Scan,
        StartElevator
    }

    [Header("Elevator Setting")]
    [SerializeField] Transform environmentToMove;
    [SerializeField] float timeToReachGate;
    [SerializeField] float graceTimeWhenElevatorStop = 5f;


    [HideInInspector] public Gate currentGate;
    private Gate nextGate;

    [Header("Y Axis")]
    [SerializeField] float yAxisGate1;
    [SerializeField] float yAxisGate2;
    [SerializeField] float yAxisGate3;

    [Header("Console Text")]
    [SerializeField] string fuseConsoleText = "Missing Fuse";
    [SerializeField] string scanConsoleText = "Require Identification";
    [SerializeField] string startConsoleText = "Start Elevator";
    
    [Header("References")]
    [SerializeField] FixedHornetSpawn spawnScript;
    [SerializeField] TextMeshProUGUI consoleUI;

    [Header("Gate Order")]
    [SerializeField] List<Gate> gateOrderList;

    [Header("Task Order")]
    [SerializeField] List<Tasks> queue;


    private Tasks currentTask;
    private float currentYAxis;
    private float speedtoMove;
    private float yAxisToStop;
    private NavMeshSurface navSurface;

    [HideInInspector] public bool areAllTasksComplete = true;
    [HideInInspector] public bool canSpawnEnemy = false;


    // Start is called before the first frame update
    void Start()
    {
        currentTask = Tasks.StartElevator;

        navSurface = GetComponent<NavMeshSurface>();

        currentGate = gateOrderList[0];
        nextGate = gateOrderList[1];

        currentYAxis = environmentToMove.position.y;
    }


    public void StartElevator()
    {
        int index = gateOrderList.IndexOf(currentGate);
        nextGate = gateOrderList[index + 1];

        if (nextGate == Gate.Gate1)
            yAxisToStop = yAxisGate1;
        else if (nextGate == Gate.Gate2)
            yAxisToStop = yAxisGate2;
        else if (nextGate == Gate.Gate3)
            yAxisToStop = yAxisGate3;

        StartCoroutine(MoveEnvironment());
    }


    IEnumerator MoveEnvironment()
    {
        // Calculate move speed
        speedtoMove = (yAxisToStop - environmentToMove.position.y) / timeToReachGate;

        while (currentYAxis != yAxisToStop)
        {
            Vector3 newPosition = new Vector3(environmentToMove.position.x, environmentToMove.position.y + Time.deltaTime * speedtoMove, environmentToMove.position.z);

            if (newPosition.y > yAxisToStop)
            {
                newPosition.y = yAxisToStop;
            }

            environmentToMove.position = newPosition;

            currentYAxis = newPosition.y;

            yield return null;

        }

        yield return new WaitForSeconds(graceTimeWhenElevatorStop);

        // Bake navmesh here
        navSurface.BuildNavMesh();
        Debug.Log("build");

        currentGate = nextGate;

        // Get next gate
        int index = gateOrderList.IndexOf(currentGate);
        spawnScript.LoadNextGate(index - 1);
        canSpawnEnemy = true;

        LoadTask();
    }

    void LoadTask()
    {
        currentTask = queue[0];

        if (currentTask == Tasks.Fuse)
        {
            consoleUI.text = fuseConsoleText;
        }
        else  if (currentTask == Tasks.Scan)
        {
            consoleUI.text = scanConsoleText;
        }
        else if (currentTask == Tasks.StartElevator)
        {
            consoleUI.text = startConsoleText;
        }
    }

    public void FinishTask()
    {
        queue.Remove(currentTask);

        if (currentTask == Tasks.StartElevator)
            return;


        if (queue.Count > 0)
            LoadTask();
    }
}
