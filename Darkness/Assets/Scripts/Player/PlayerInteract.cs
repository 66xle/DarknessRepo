using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [Header("Box Detection")]
    [SerializeField] float boxWidth = 1f;
    [SerializeField] float boxHeight = 1f;
    [SerializeField] float interactDistance = 5f;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] bool enableBoxGizmos = false;

    [Header("Restart Elevator")]
    [SerializeField] float restartTimer = 20f;
    private float currentRestartTimer = 20f;

    [Header("Scanner")]
    [SerializeField] float scanMaxProgress = 100f;
    [SerializeField] float timeToMaxProgress = 10f;
    [SerializeField] LayerMask scannerLayer;
    [SerializeField] Transform scanListGate1;
    [SerializeField] Transform scanListGate2;
    [SerializeField] Transform scanListGate3;
    private Transform currentScanParent;
    private List<Transform> currentScanList = new List<Transform>();
    private bool isScanTaskActive = false;

    [Header("Door")]
    [SerializeField] Transform door;
    private bool isDoorTriggered = false;

    [Header("UI")]
    [SerializeField] string fuseText = "Collect Fuse";
    [SerializeField] string insertFuseText = "Insert Fuse";
    [SerializeField] string consoleText = "Start Elevator";
    [SerializeField] string fixConsoleText = "Restart Elevator";
    [SerializeField] string intercomText = "Interact with intercom";
    private List<GameObject> fuseList = new List<GameObject>();

    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] Transform headCamera;
    [SerializeField] GameObject interactUI;
    [SerializeField] TextMeshProUGUI interactUIText;


    bool isInteractUIActive = false;
    bool hasIntercomActivated = false;
    [HideInInspector] public bool canCollectFuse = false;


    public GameObject intercom;


    public void LoadScanners(GateLevel.Gate gate)
    {
        isScanTaskActive = true;
        hasIntercomActivated = false;

        currentScanList.Clear();

        if (gate == GateLevel.Gate.Gate1)
            currentScanParent = scanListGate1;
        else if (gate == GateLevel.Gate.Gate2)
            currentScanParent = scanListGate2;
        else if (gate == GateLevel.Gate.Gate3)
            currentScanParent = scanListGate3;

        for (int i = 0; i < currentScanParent.childCount; i++)
        {
            currentScanList.Add(currentScanParent.GetChild(i));
        }
    }

    private void Start()
    {
        isInteractUIActive = false;
        canCollectFuse = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.isPaused)
            return;

        ScanRayCast();
        InteractRaycast();
    }

    void ScanRayCast()
    {
        if (!isScanTaskActive)
            return;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, scannerLayer))
        {
            // Get scanner were standing on
            int index = currentScanList.IndexOf(hit.transform);
            Scanner scan = currentScanList[index].GetComponent<Scanner>();

            
            float addProgress = scanMaxProgress / timeToMaxProgress;
            scan.AddProgress(addProgress);

            
            if (scan.IsScanFinished(scanMaxProgress))
            {
                AreAllScannersFinished();
            }
            else
            {
                Debug.Log("Scanning : " + scan.CurrentProgress);
            }
        }
    }
    
    void AreAllScannersFinished()
    {
        foreach (Transform transform in currentScanList)
        {
            Scanner scan = transform.GetComponent<Scanner>();

            if (scan.IsScanFinished(scanMaxProgress))
            {
                continue;
            }

            return;
        }

        isScanTaskActive = false;
        gameManager.FinishTask();
    }

    void InteractRaycast()
    {
        Vector3 center = headCamera.position + headCamera.forward * interactDistance / 2;

        Vector3 boxSize = new Vector3(boxWidth, boxHeight, interactDistance);

        Collider[] colliders = Physics.OverlapBox(center, boxSize / 2, headCamera.rotation, interactableLayer);

        if (colliders.Length == 0)
        {
            if (isInteractUIActive)
            {
                ToggleUI();
            }

            return;
        }

        Vector3 direction = colliders[0].bounds.center - headCamera.position;

        Debug.DrawRay(headCamera.position, direction * interactDistance);

        RaycastHit hit;

        if (Physics.Raycast(headCamera.position, direction, out hit, interactDistance, interactableLayer))
        {
            if (hit.collider.CompareTag("Fuse") && canCollectFuse)
            {
                if (!isInteractUIActive)
                    ToggleUI(fuseText);

                CollectFuse(hit.collider.transform.parent.gameObject);
            }
            else if (hit.collider.CompareTag("Console") && fuseList.Count > 0)
            {
                if (!isInteractUIActive)
                    ToggleUI(insertFuseText);

                InsertFuse();
            }
            else if (hit.collider.CompareTag("Console") && gameManager.areAllTasksComplete)
            {
                if (!isInteractUIActive)
                {
                    if (!gameManager.isElevatorBroken)
                        ToggleUI(consoleText);
                    else
                        ToggleUI(fixConsoleText);
                }
                ConsoleInteract();
            }
            else if (hit.collider.CompareTag("Intercom") && !hasIntercomActivated)
            {
                if (!isInteractUIActive)
                {
                    ToggleUI(intercomText);
                }

                IntercomInteract(hit.collider.GetComponent<Intercom>());
            }
            else if (isInteractUIActive)
            {
                ToggleUI();
            }
        }
    }

    void IntercomInteract(Intercom script)
    {
        if (Input.GetKeyDown(KeyCode.E) && isInteractUIActive)
        {
            hasIntercomActivated = true;

            ToggleUI();

            if (script.restartCount == 0)
            {
                script.audioSource.clip = script.clipList[0];
                script.audioSource.Play();

            }
            if (script.restartCount == 1)
            {
                script.audioSource.clip = script.clipList[1];
                script.audioSource.Play();

            }
            if (script.restartCount == 2)
            {
                script.audioSource.clip = script.clipList[2];
                script.audioSource.Play();

            }
            if (script.restartCount == 3)
            {
                script.audioSource.clip = script.clipList[3];
                script.audioSource.Play();

            }
            if (script.restartCount == 4)
            {
                script.audioSource.clip = script.clipList[4];
                script.audioSource.Play();

            }
            if (script.restartCount == 5)
            {
                script.audioSource.clip = script.clipList[5];
                script.audioSource.Play();

            }
            if (script.restartCount == 6)
            {
                script.audioSource.clip = script.clipList[6];
                script.audioSource.Play();

            }
            if (script.restartCount == 7)
            {
                script.audioSource.clip = script.clipList[7];
                script.audioSource.Play();

            }
            if (script.restartCount == 8)
            {
                script.audioSource.clip = script.clipList[8];
                script.audioSource.Play();

            }

        }
    }

    void CollectFuse(GameObject fuseObject)
    {
        if (Input.GetKeyDown(KeyCode.E) && isInteractUIActive)
        {
            fuseList.Add(fuseObject);

            if (fuseObject != null)
            {
                Destroy(fuseObject);
            }
        }
    }

    void InsertFuse(GameObject fuse = null)
    {
        if (Input.GetKeyDown(KeyCode.E) && isInteractUIActive)
        {
            fuseList.Clear();
            //fuse.SetActive(true);

            ToggleUI();
            gameManager.FinishTask();

            canCollectFuse = true;
        }
    }


    void ConsoleInteract()
    {
        if (Input.GetKeyDown(KeyCode.E) && isInteractUIActive )
        {
            currentRestartTimer = restartTimer;

            gameManager.areAllTasksComplete = false;

            ToggleUI();

            gameManager.FinishTask();

            if (gameManager.isElevatorBroken)
                StartCoroutine(RestartingElevator());
            else
                gameManager.StartElevator();
        }
    }

    IEnumerator RestartingElevator()
    {
        while (currentRestartTimer > 0f)
        {
            gameManager.SetConsoleUI((int)currentRestartTimer);

            currentRestartTimer -= Time.deltaTime;

            yield return null;
        }

        gameManager.StartElevator();
    }

    void ToggleUI(string text = null)
    {
        if (!isInteractUIActive)
        {
            isInteractUIActive = true;
            interactUI.SetActive(isInteractUIActive);
        }
        else
        {
            isInteractUIActive = false;
            interactUI.SetActive(isInteractUIActive);
        }
        

        interactUIText.text = text;
    }


    private void OnDrawGizmos()
    {
        if (!enableBoxGizmos)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(headCamera.position, headCamera.forward * interactDistance);

        Gizmos.color = Color.yellow;

        Vector3 center = headCamera.position + headCamera.forward * interactDistance / 2;

        Gizmos.matrix = Matrix4x4.TRS(center, headCamera.rotation, headCamera.lossyScale);


        Vector3 boxSize = new Vector3(boxWidth, boxHeight, interactDistance);
        Gizmos.DrawWireCube(Vector3.zero, boxSize * 2);
    }
}
