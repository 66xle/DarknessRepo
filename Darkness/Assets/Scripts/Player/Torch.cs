using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Torch : MonoBehaviour
{
    [Header("Battery")]
    [SerializeField] float maxBattery = 100f;
    [SerializeField] float drainNormalBattery = 1f;
    [SerializeField] float drainUVBattery = 3f;

    [Header("Sphere Cast")]
    public float torchDistance = 10f;
    [SerializeField] float sphereCastRadius = 2f;

    [Header("References")]
    [SerializeField] GameObject torch;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] FixedHornetSpawn spawnScript;

    private Light normalTorchLight;
    private Light UVTorchLight;
    private float currentBattery;

    
    public bool isTorchActive = false, isUVActive = false;
    private Vector3 hitPos;

    // Start is called before the first frame update
    void Start()
    {
        torch.GetComponent<SphereCollider>().radius = (torchDistance + sphereCastRadius) * 2 ;

        normalTorchLight = torch.transform.GetChild(0).GetComponent<Light>();
        UVTorchLight = torch.transform.GetChild(1).GetComponent<Light>();

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
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isTorchActive && !isUVActive && currentBattery > 0f)
        {
            normalTorchLight.enabled = true;
            isTorchActive = true;
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0) && isTorchActive || currentBattery <= 0f)
        {
            normalTorchLight.enabled = false;
            isTorchActive = false;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1) && !isTorchActive && !isUVActive && currentBattery > 0f)
        {
            UVTorchLight.enabled = true;
            isTorchActive = true;
            isUVActive = true;
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1) && isUVActive || currentBattery <= 0f)
        {
            UVTorchLight.enabled = false;
            isTorchActive = false;
            isUVActive = false;
        }
    }

    void Battery()
    {
        if (isTorchActive)
        {
            float drainBattery = isUVActive ? drainUVBattery : drainNormalBattery; 

            currentBattery -= drainBattery * Time.deltaTime;

            //Debug.Log(currentBattery);
        }

        currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);
    }

    void Raycast()
    {
        if (isTorchActive)
        {
            hitPos = Vector3.zero;

            RaycastHit hit;

            if (Physics.SphereCast(torch.transform.position, sphereCastRadius, torch.transform.forward, out hit, torchDistance, ~groundLayer))
            {
                if (hit.transform.CompareTag("Enemy"))
                {
                    hitPos = hit.point + hit.normal * (sphereCastRadius / 2);

                    Enemy enemy = hit.collider.gameObject.GetComponent<Enemy>();

                    if (!enemy.isDead)
                    {
                        enemy.isDead = true;
                        StartCoroutine(enemy.Death(spawnScript));
                    }

                    
                    
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
