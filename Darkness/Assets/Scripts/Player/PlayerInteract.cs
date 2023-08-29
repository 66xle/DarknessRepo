using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [Header("Box Detection")]
    [SerializeField] Vector3 boxCheckSize;
    [SerializeField] float boxOffsetFromPlayer = 1f;
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

    [Header("Interactable Objects")]
    [SerializeField] List<Transform> interactableList;

    [Header("UI")]
    [SerializeField] GameObject interactUI;
    [SerializeField] TextMeshProUGUI interactUIText;
    [SerializeField] string fuseText = "Collect Fuse";
    [SerializeField] string insertFuseText = "Insert Fuse";
    [SerializeField] string consoleText = "Start Elevator";
    private List<GameObject> fuseList = new List<GameObject>();

    [Header("References")]
    [SerializeField] Camera interactCamera;
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

        Vector3 center = headCamera.position + headCamera.forward * boxOffsetFromPlayer;

        Collider[] colliders = Physics.OverlapBox(center, boxCheckSize / 2, headCamera.rotation, interactableLayer);

        if (colliders.Length == 0)
        {
            if (isInteractUIActive)
            {
                ToggleUI();
            }

            return;
        }

        Debug.Log("hit");

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
        

        foreach (Transform transform in interactableList)
        {
            Collider collider = transform.GetComponent<Collider>();

            
        }
    }

    bool IsLookingAtInteractable(Camera camera, Collider collider)
    {
        Bounds bounds = collider.bounds;

        Plane[] cameraFustrum = GeometryUtility.CalculateFrustumPlanes(camera);


        if (GeometryUtility.TestPlanesAABB(cameraFustrum, bounds))
        {
            Debug.Log("yes");
            return true;
        }
        else
        {
            if (isInteractUIActive)
                ToggleUI();

            return false;
        }
    }

    void CollectFuse(GameObject fuseObject)
    {
        if (Input.GetKeyDown(KeyCode.E) && isInteractUIActive)
        {
            fuseList.Add(fuseObject);

            if (fuseObject != null)
            {
                interactableList.Remove(fuseObject.transform);
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
        Gizmos.color = Color.yellow;

        Vector3 center = headCamera.position + headCamera.forward * boxOffsetFromPlayer;
        
        Gizmos.matrix = Matrix4x4.TRS(center, headCamera.rotation, headCamera.lossyScale);


        Gizmos.DrawWireCube(Vector3.zero, boxCheckSize);
       
    }
}
