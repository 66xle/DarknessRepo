using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [Header("Sphere Cast")]
    [SerializeField] float sphereCastRadius = 2f;
    [SerializeField] float interactDistance = 5f;
    [SerializeField] Transform headCamera;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask scannerLayer;

    [Header("Scanner")]
    [SerializeField] float scanMaxProgress = 100f;
    [SerializeField] float timeToMaxProgress = 10f;
    [SerializeField] List<Transform> scanList;
    private List<float> scanProgressList = new List<float>();

    [Header("Door")]
    [SerializeField] Transform door;
    private bool isDoorTriggered = false;

    

    [Header("UI")]
    [SerializeField] GameObject interactUI;
    [SerializeField] TextMeshProUGUI interactUIText;
    [SerializeField] string fuseText = "Collect Fuse";
    [SerializeField] string insertFuseText = "Insert Fuse";
    [SerializeField] string consoleText = "Start Elevator";
    private List<GameObject> fuseList = new List<GameObject>();

    [Header("References")]
    [SerializeField] GameManager gameManager;

    bool isInteractUIActive = false;
    


    private Vector3 hitPosition;

    private void Start()
    {
        foreach (Transform t in scanList)
        {
            scanProgressList.Add(0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ScanRayCast();
        InteractRaycast();
    }

    void ScanRayCast()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, scannerLayer))
        {
            // Get scanner were standing on
            int index = scanList.IndexOf(hit.transform);
            Scanner scan = scanList[index].GetComponent<Scanner>();

            
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
        foreach (Transform transform in scanList)
        {
            Scanner scan = transform.GetComponent<Scanner>();

            if (scan.IsScanFinished(scanMaxProgress))
            {
                continue;
            }

            return;
        }

        gameManager.FinishTask();

        Debug.Log("Play");

        if (!isDoorTriggered)
        {
            isDoorTriggered = true;
            door.GetComponent<Animation>().Play();
        }
    }

    void InteractRaycast()
    {
        RaycastHit hit;

        if (Physics.SphereCast(headCamera.position, sphereCastRadius, headCamera.forward, out hit, interactDistance, interactableLayer))
        {
            hitPosition = hit.point + hit.normal * (sphereCastRadius / 2);

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
        }
        else if (isInteractUIActive)
        {
            ToggleUI();
        }
    }

    void CollectFuse(GameObject fuseObject)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            fuseList.Add(fuseObject);
            Destroy(fuseObject);
        }
    }

    void InsertFuse(GameObject fuse)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            fuseList.Clear();
            fuse.SetActive(true);

            ToggleUI();
            gameManager.FinishTask();
        }
    }


    void ConsoleInteract()
    {
        if (Input.GetKeyDown(KeyCode.E))
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPosition, sphereCastRadius);
    }
}
