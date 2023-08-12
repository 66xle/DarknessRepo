using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Torch : MonoBehaviour
{
    [Header("Battery")]
    [SerializeField] float maxBattery = 100f;
    [SerializeField] float drainBattery = 1f;

    [Header("Sphere Cast")]
    [SerializeField] float torchDistance = 10f;
    [SerializeField] float sphereCastRadius = 2f;

    [Header("References")]
    [SerializeField] GameObject torch;
    [SerializeField] LayerMask groundLayer;


    private Light torchLight;
    private float currentBattery;

    [HideInInspector]
    public bool isTorchActive = false;
    private Vector3 hitPos;

    // Start is called before the first frame update
    void Start()
    {
        torchLight = torch.GetComponent<Light>();

        currentBattery = maxBattery;
    }

    // Update is called once per frame
    void Update()
    {
        ToggleTorch();

        Raycast();

        Battery();
    }

    void ToggleTorch()
    {
        if (Input.GetKey(KeyCode.Mouse0) && !isTorchActive && currentBattery > 0f)
        {
            torchLight.enabled = true;
            isTorchActive = true;
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0) && isTorchActive || currentBattery <= 0f)
        {
            torchLight.enabled = false;
            isTorchActive = false;
        }
    }

    void Battery()
    {
        if (isTorchActive)
        {
            currentBattery -= drainBattery * Time.deltaTime;

            Debug.Log(currentBattery);
        }

        currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);
    }

    void Raycast()
    {
        if (isTorchActive)
        {
            RaycastHit hit;
            
            if (Physics.SphereCast(torch.transform.position, sphereCastRadius, torch.transform.forward, out hit, torchDistance, ~groundLayer))
            {
                if (hit.transform.CompareTag("Enemy"))
                {
                    hitPos = hit.point + hit.normal * (sphereCastRadius / 2);   
                }
                else
                {
                    hitPos = Vector3.zero;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (isTorchActive && hitPos != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(hitPos, sphereCastRadius);
        }
    }
}
