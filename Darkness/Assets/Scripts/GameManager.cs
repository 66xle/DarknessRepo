using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Elevator Setting")]
    [SerializeField] Transform environmentToMove;
    [SerializeField] float timeToReachGate;
    [SerializeField] float graceTimeWhenElevatorStop = 5f;
    [SerializeField] float lowerPlatformYAxis = 5f;
    [SerializeField] float clearColldierMaxRadius = 1f;
    [HideInInspector] public GateLevel currentGateLevel;
    private GateLevel nextGateLevel;

    [Header("Console Text")]
    [SerializeField] string fuseConsoleText = "Missing Fuse";
    [SerializeField] string scanConsoleText = "Require Identification";
    [SerializeField] string startConsoleText = "Start Elevator";
    [SerializeField] string movingConsoleText = "Continuing Descent";
    [SerializeField] string reachedConsoleText = "Destination Reached";
    [SerializeField] string restartConsoleText = "Restarting";
    [SerializeField] string errorConsoleText = "Error!";
    
    [Header("References")]
    [SerializeField] FixedHornetSpawn spawnScript;
    [SerializeField] PlayerInteract interactScript;
    [SerializeField] TextMeshProUGUI consoleUI;
    [SerializeField] Transform lowerPlatform;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject gameoverMenu;
    [SerializeField] Animator animController;
    [SerializeField] BoxCollider hatchA;
    [SerializeField] BoxCollider hatchB;
    [SerializeField] SphereCollider clearCollider;

    [Header("Gate Order")]
    [SerializeField] List<GateLevel> gateOrderList;

    private List<GateLevel> gateQueue;
    private List<GateLevel.Tasks> taskQueue;


    private GateLevel.Tasks currentTask;
    private float currentYAxis;
    private float speedtoMove;
    private float yAxisToStop;
    private float yAxisBreakDown;
    private NavMeshSurface navSurface;

    [HideInInspector] public bool isElevatorBroken;
    [HideInInspector] public bool areAllTasksComplete = true;
    [HideInInspector] public bool canSpawnEnemy = false;

    [HideInInspector] public bool isPaused = false;

    private bool isLowerPlatformReached = false;


    // Start is called before the first frame update
    void Start()
    {
        areAllTasksComplete = true;
        canSpawnEnemy = false;
        isPaused = false;
        isLowerPlatformReached = false;
        isElevatorBroken = false;
        yAxisBreakDown = 10000f;

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
        if (isElevatorBroken)
        {
            animController.SetBool("isConsoleOpen", false);
            isElevatorBroken = false;
            yAxisBreakDown = 10000f;
            return;
        }

        consoleUI.text = movingConsoleText;

        gateQueue.Remove(currentGateLevel);
        nextGateLevel = gateQueue[0];

        yAxisToStop = nextGateLevel.yAxis;

        if (!isLowerPlatformReached)
        {
            StartCoroutine(MoveLowerPlatform());
        }
        else
        {
            animController.SetBool("isConsoleOpen", false);
        }
    }

    void CalculateStopPoint()
    {
        yAxisBreakDown = UnityEngine.Random.Range(currentYAxis + 10f, yAxisToStop - 20f);

    }

    IEnumerator MoveLowerPlatform()
    {
        float currentPlatformYAxis = lowerPlatform.transform.position.y;

        speedtoMove = (yAxisToStop - environmentToMove.position.y) / timeToReachGate;

        while (currentPlatformYAxis != lowerPlatformYAxis)
        {
            Vector3 newEnvironmentPosition = new Vector3(environmentToMove.position.x, environmentToMove.position.y + Time.deltaTime * speedtoMove, environmentToMove.position.z);
            Vector3 newPlatformPosition = new Vector3(lowerPlatform.position.x, lowerPlatform.position.y + Time.deltaTime * speedtoMove, lowerPlatform.position.z);

            if (newPlatformPosition.y > lowerPlatformYAxis)
            {
                newPlatformPosition.y = lowerPlatformYAxis;
            }

            environmentToMove.position = newEnvironmentPosition;
            lowerPlatform.position = newPlatformPosition;

            currentPlatformYAxis = newPlatformPosition.y;
            currentYAxis = newEnvironmentPosition.y;

            yield return null;
        }

        isLowerPlatformReached = true;

        CalculateStopPoint();
        StartCoroutine(MoveEnvironment());
    }

    public IEnumerator MoveEnvironment()
    {
        // Calculate move speed
        speedtoMove = (yAxisToStop - environmentToMove.position.y) / timeToReachGate;

        while (currentYAxis != yAxisToStop)
        {
            #region Move Elevator

            Vector3 newPosition = new Vector3(environmentToMove.position.x, environmentToMove.position.y + Time.deltaTime * speedtoMove, environmentToMove.position.z);

            if (newPosition.y > yAxisToStop)
            {
                newPosition.y = yAxisToStop;
            }

            environmentToMove.position = newPosition;

            currentYAxis = newPosition.y;

            #endregion

            // Bake navmesh here
            navSurface.BuildNavMesh();

            if (currentYAxis >= yAxisBreakDown)
            {
                Debug.Log(yAxisBreakDown);

                isElevatorBroken = true;
                break;
            }

            yield return null;
        }

        #region Open Console

        clearCollider.enabled = true;
        animController.SetBool("isConsoleOpen", true);

        while (clearCollider.radius < clearColldierMaxRadius)
        {
            float raduis = clearCollider.radius + Time.deltaTime * 5f;

            clearCollider.radius = raduis;

            Debug.Log(raduis);

            yield return null;
        }

        clearCollider.enabled = false;

        consoleUI.text = reachedConsoleText;

        #endregion

        yield return new WaitForSeconds(graceTimeWhenElevatorStop);

        #region Is Elevator Broken

        if (!isElevatorBroken)
        {
            // Set gate level
            currentGateLevel = nextGateLevel;
            taskQueue = new List<GateLevel.Tasks>(currentGateLevel.taskList);

            // Load spawning enemies
            spawnScript.LoadSpawnPoints(currentGateLevel.currentGate);
            canSpawnEnemy = true;
        }
        else
        {
            spawnScript.LoadElevatorSpawnPoint();
            canSpawnEnemy = true;
            areAllTasksComplete = true;
        }

        #endregion

        hatchA.enabled = true;
        hatchB.enabled = true;

        LoadTask();
    }

    void LoadTask()
    {
        if (isElevatorBroken)
        {
            currentTask = GateLevel.Tasks.RestartElevator;
            consoleUI.text = errorConsoleText;

            return;
        }

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
            spawnScript.CanSpawnHornet = false;
        }
    }

    public void FinishTask()
    {
        if (isElevatorBroken) return;

        taskQueue.Remove(currentTask);

        if (currentTask == GateLevel.Tasks.StartElevator)
        {
            return;
        }

        if (taskQueue.Count > 0)
            LoadTask();
    }
}
