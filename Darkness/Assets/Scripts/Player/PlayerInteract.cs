using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] float sphereCastRadius = 2f;
    [SerializeField] float interactDistance = 5f;
    [SerializeField] Transform headCamera;
    [SerializeField] LayerMask interactableLayer;

    [Header("UI")]
    [SerializeField] GameObject interactUI;
    [SerializeField] TextMeshProUGUI interactUIText;
    [SerializeField] string fuseText = "Collect Fuse";
    [SerializeField] string insertFuseText = "Insert Fuse";
    private List<GameObject> fuseList = new List<GameObject>();

    bool isInteractUIActive = false;


    private Vector3 hitPosition;



    // Update is called once per frame
    void Update()
    {
        InteractRaycast();
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
