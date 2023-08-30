using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Elevator Setting")]
    [SerializeField] Transform environmentToMove;
    [SerializeField] float timeToReachGate;
    [SerializeField] float graceTimeWhenElevatorStop = 5f;


    [HideInInspector] public GateLevel currentGateLevel;
    private GateLevel nextGateLevel;

    [Header("Console Text")]
    [SerializeField] string fuseConsoleText = "Missing Fuse";
    [SerializeField] string scanConsoleText = "Require Identification";
    [SerializeField] string startConsoleText = "Start Elevator";
    [SerializeField] string movingConsoleText = "Continuing Descent";
    [SerializeField] string reachedConsoleText = "Destination Reached";
    
    [Header("References")]
    [SerializeField] FixedHornetSpawn spawnScript;
    [SerializeField] PlayerInteract interactScript;
    [SerializeField] TextMeshProUGUI consoleUI;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject gameoverMenu;

    [Header("Gate Order")]
    [SerializeField] List<GateLevel> gateOrderList;

    private List<GateLevel> gateQueue;
    private List<GateLevel.Tasks> taskQueue;


    private GateLevel.Tasks currentTask;
    private float currentYAxis;
    private float speedtoMove;
    private float yAxisToStop;
    private NavMeshSurface navSurface;

    [HideInInspector] public bool areAllTasksComplete = true;
    [HideInInspector] public bool canSpawnEnemy = false;

    [HideInInspector] public bool isPaused = false;


    // Start is called before the first frame update
    void Start()
    {
        areAllTasksComplete = true;
        canSpawnEnemy = false;
        isPaused = false;

        navSurface = GetComponent<NavMeshSurface>();

        gateQueue = gateOrderList;

        currentGateLevel = gateQueue[0];
        taskQueue = new List<GateLevel.Tasks>(currentGateLevel.taskList);


        currentTask = taskQueue[0];
        currentYAxis = currentGateLevel.yAxis;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {// unpause
                ResumeGame();
            }
            else
            {// pause
                PauseGame();
            }
        }
    }

    public void Death()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;

        isPaused = true;

        gameoverMenu.SetActive(true);
    }

    void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        isPaused = true;
    }

    public void ResumeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        isPaused = false;
    }

    public void MainMenu()
    {
        Time.timeScale = 1;

        SceneManager.LoadScene(0);
    }

    public void StartElevator()
    {
        consoleUI.text = movingConsoleText;

        gateQueue.Remove(currentGateLevel);
        nextGateLevel = gateQueue[0];

        yAxisToStop = nextGateLevel.yAxis;

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


            // Bake navmesh here
            navSurface.BuildNavMesh();
            

            yield return null;

        }

        consoleUI.text = reachedConsoleText;

        yield return new WaitForSeconds(graceTimeWhenElevatorStop);

        // Set gate level
        currentGateLevel = nextGateLevel;
        taskQueue = new List<GateLevel.Tasks>(currentGateLevel.taskList);

        // Load spawning enemies
        spawnScript.LoadSpawnPoints(currentGateLevel.currentGate);
        canSpawnEnemy = true;

        Debug.Log("spawn enemies");

        LoadTask();
    }

    void LoadTask()
    {
        currentTask = taskQueue[0];

        if (currentTask == GateLevel.Tasks.Fuse)
        {
            consoleUI.text = fuseConsoleText;
        }
        else  if (currentTask == GateLevel.Tasks.Scan)
        {
            interactScript.LoadScanners(currentGateLevel.currentGate);
            consoleUI.text = scanConsoleText;
        }
        else if (currentTask == GateLevel.Tasks.StartElevator)
        {
            consoleUI.text = startConsoleText;
            areAllTasksComplete = true;
        }
    }

    public void FinishTask()
    {
        taskQueue.Remove(currentTask);

        if (currentTask == GateLevel.Tasks.StartElevator)
        {
            
            return;
        }


        if (taskQueue.Count > 0)
            LoadTask();
    }
}
