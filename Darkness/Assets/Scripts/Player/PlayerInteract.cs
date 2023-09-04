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
    private List<GameObject> fuseList = new List<GameObject>();

    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] Transform headCamera;
    [SerializeField] GameObject interactUI;
    [SerializeField] TextMeshProUGUI interactUIText;


    bool isInteractUIActive = false;

    private Vector3 hitPosition;


    public void LoadScanners(GateLevel.Gate gate)
    {
        isScanTaskActive = true;

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

        Vector3 direction = colliders[0].transform.position - headCamera.position;

        RaycastHit hit;

        if (Physics.Raycast(headCamera.position, direction, out hit, interactDistance, interactableLayer))
        {
            if (hit.collider.CompareTag("Fuse"))
            {
                if (!isInteractUIActive)
                    ToggleUI(fuseText);

                CollectFuse(hit.collider.gameObject);
            }
            else if (hit.collider.CompareTag("FusePlate") && fuseList.Count > 0)
            {

                if (!isInteractUIActive)
                    ToggleUI(insertFuseText);

                InsertFuse(hit.collider.transform.GetChild(0).gameObject);
            }
            else if (hit.collider.CompareTag("Console") && gameManager.areAllTasksComplete)
            {
                if (!isInteractUIActive)
                    ToggleUI(consoleText);

                ConsoleInteract();
            }
            else if (isInteractUIActive)
            {
                ToggleUI();
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

    void InsertFuse(GameObject fuse)
    {
        if (Input.GetKeyDown(KeyCode.E) && isInteractUIActive)
        {
            fuseList.Clear();
            fuse.SetActive(true);

            ToggleUI();
            gameManager.FinishTask();
        }
    }


    void ConsoleInteract()
    {
        if (Input.GetKeyDown(KeyCode.E) && isInteractUIActive )
        {
            gameManager.StartElevator();

            gameManager.areAllTasksComplete = false;

            ToggleUI();

            gameManager.FinishTask();
        }
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
