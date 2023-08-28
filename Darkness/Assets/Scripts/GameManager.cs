using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public enum Gate
    {
        Start,
        Gate1,
        Gate2,
        Gate3,
    }

    [Header("Settings")]
    [SerializeField] Transform environmentToMove;
    [SerializeField] float timeToReachGate;
    [SerializeField] List<Gate> gateOrderList;
    [SerializeField] FixedHornetSpawn spawnScript;
    [HideInInspector] public Gate currentGate;
    private Gate nextGate;

    [Header("Y Axis")]
    [SerializeField] float yAxisGate1;
    [SerializeField] float yAxisGate2;
    [SerializeField] float yAxisGate3;

    private float currentYAxis;
    private float speedtoMove;
    private float yAxisToStop;
    private NavMeshSurface navSurface;

    

    [HideInInspector] public bool areAllTasksComplete = true;
    [HideInInspector] public bool canSpawnEnemy = false;


    // Start is called before the first frame update
    void Start()
    {
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

        //SetStatic(false);


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

        //SetStatic(true);

        // Bake navmesh here
        navSurface.BuildNavMesh();
        Debug.Log("build");

        currentGate = nextGate;

        int index = gateOrderList.IndexOf(currentGate);

        spawnScript.SetInList(index - 1);
        canSpawnEnemy = true;

    }

    void SetStatic(bool setStatic)
    {
        // Set Static
        for (int i = 0; i < environmentToMove.childCount; i++)
        {
            GameObject child = environmentToMove.GetChild(i).gameObject;

            for (int j = 0; j < child.transform.childCount; j++)
            {
                child.transform.GetChild(j).gameObject.isStatic = setStatic;
            }
        }
    }
}
