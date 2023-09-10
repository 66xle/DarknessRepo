using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor.ShaderGraph.Internal;

public class Torch : MonoBehaviour
{
    [Header("Battery")]
    [SerializeField] float maxBattery = 100f;
    [SerializeField] float drainNormalBattery = 1f;
    [SerializeField] float rechargeCooldown = 3f;
    [SerializeField] float rechargeRate = 20f;
    [SerializeField] float maxIntensity = 600f;
    [SerializeField] float fadeInTime = 1;
    [SerializeField] float fadeOutTime = 1;
    private float currentRechargeCooldown;

    [Header("Flicker")]
    [SerializeField] float emptyCooldown = 2f;
    private float currentEmptyCooldown;
    private bool isFlickering;
    private bool disableTorch;

    [Header("Box Cast")]
    public float torchDistance = 10f;
    [SerializeField] float boxWidth = 1f;
    [SerializeField] float boxHeight = 1f;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] bool enableBoxGizmos = false;

    [Header("References")]
    [SerializeField] GameObject torch;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] GameManager gameManager;
    [SerializeField] Slider batteryPercentage;

    #region Internal Variables

    private Light normalTorchLight;
    private float currentBattery;

    [HideInInspector] public bool isTorchActive = false, isUVActive = false;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        isFlickering = false;
        disableTorch = false;

        normalTorchLight = torch.transform.GetChild(0).GetComponent<Light>();
        normalTorchLight.intensity = maxIntensity;

        currentBattery = maxBattery;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.isPaused)
            return;

        ToggleTorch();

        Raycast();

        Battery();

        Flicker();
    }

    void ToggleTorch()
    {
        if (Input.GetKey(KeyCode.Mouse0) && isFlickering)
        {
            disableTorch = true;
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            disableTorch = false;
        }

        if (Input.GetKey(KeyCode.Mouse0) && currentBattery > 0f && !disableTorch && !isFlickering)
        {
            normalTorchLight.enabled = true;
            isTorchActive = true;

            float speed = maxIntensity / fadeInTime;
            float currentIntensity = normalTorchLight.intensity + Time.deltaTime * speed;

            if (currentIntensity >= maxIntensity)
                currentIntensity = maxIntensity;

            normalTorchLight.intensity = currentIntensity;

        }
        else if (isTorchActive || currentBattery <= 0f)
        {
            float speed = maxIntensity / fadeOutTime;
            float currentIntensity = normalTorchLight.intensity - Time.deltaTime * speed;

            if (currentIntensity <= 0f)
            {
                normalTorchLight.enabled = false;
                isTorchActive = false;
                currentIntensity = 0f;
            }

            normalTorchLight.intensity = currentIntensity;
        }
    }

    void Flicker()
    {
        if (currentBattery <= 0f)
        {
            isTorchActive = false;
            currentEmptyCooldown = emptyCooldown;
        }

        if (currentEmptyCooldown > 0f)
        {
            currentEmptyCooldown -= Time.deltaTime;

            if (!isFlickering)
            {
                StartCoroutine(FlickerTorch());
            }
        }
    }

    IEnumerator FlickerTorch()
    {
        isFlickering = true;
        normalTorchLight.intensity = maxIntensity;
        normalTorchLight.color = Color.red;

        normalTorchLight.enabled = true;

        yield return new WaitForSeconds(emptyCooldown / 10f);

        normalTorchLight.enabled = false;

        yield return new WaitForSeconds(emptyCooldown / 10f);

        normalTorchLight.intensity = 0f;
        normalTorchLight.color = Color.white;
        isFlickering = false;

        Debug.Log("done");
    }


    void Battery()
    {
        if (isTorchActive)
        {
            currentBattery -= drainNormalBattery * Time.deltaTime;

            // Reset recharge timer when active
            currentRechargeCooldown = rechargeCooldown;
        }
        else
        {
            currentRechargeCooldown -= Time.deltaTime;

            if (currentRechargeCooldown <= 0f)
            {
                currentBattery += rechargeRate * Time.deltaTime;
            }
        }

        // Clamp
        currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);

        // Get percentage of battery, set to ui
        float currentBatteryPercentage = currentBattery / maxBattery;
        batteryPercentage.value = currentBatteryPercentage;

    }

    void Raycast()
    {
        if (isTorchActive)
        {
            #region BoxCheck

            Vector3 center = torch.transform.position + torch.transform.forward * torchDistance / 2;

            Vector3 boxSize = new Vector3(boxWidth, boxHeight, torchDistance);

            Collider[] colliders = Physics.OverlapBox(center, boxSize / 2, torch.transform.rotation, enemyLayer);

            if (colliders.Length == 0)
            {
                return;
            }

            #endregion

            // Loop every enemy hit
            foreach (Collider collider in colliders)
            {

                RaycastHit hit;
                Vector3 direction = collider.transform.position - transform.position;

                // If there is nothing blocking
                if (Physics.Raycast(torch.transform.position, direction, out hit, torchDistance, ~ignoreLayer))
                {
                    if (hit.transform.CompareTag("Enemy"))
                    {
                        Enemy enemy = hit.collider.gameObject.GetComponent<Enemy>();

                        if (!enemy.isDead)
                        {
                            enemy.TakeDamage();  
                        }
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!enableBoxGizmos)
            return;


        Gizmos.color = Color.red;
        Gizmos.DrawRay(torch.transform.position, torch.transform.forward * torchDistance);

        Gizmos.color = Color.blue;
        Vector3 center = torch.transform.position + torch.transform.forward * torchDistance / 2;
        Gizmos.matrix = Matrix4x4.TRS(center, torch.transform.rotation, transform.lossyScale);


        Vector3 boxSize = new Vector3(boxWidth, boxHeight, torchDistance);
        Gizmos.DrawWireCube(Vector3.zero, boxSize * 2);
    }
}
