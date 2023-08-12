using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Tooltip("Bobs depending on player velocity")] public float bobbingSpeed = 1f;
    [Tooltip("Amount of distance camera bobs up and down")] public float bobbingAmount = 0.05f;

    #region Internal Variables

    // Classes
    PlayerMovement playerScript;

    // Components
    Transform cam;
    Rigidbody rb;

    // Variables
    float defaultPosY = 0;
    float timer = 0;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // Get Components
        cam = GetComponentInChildren<Camera>().transform;
        rb = GetComponentInParent<Rigidbody>();
        playerScript = GetComponentInParent<PlayerMovement>();

        // Set Variables
        defaultPosY = cam.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        Bobbing();
    }

    void Bobbing()
    {
        // If player is moving
        if (Mathf.Abs(playerScript.targetVelocity.x) > 0.1f || Mathf.Abs(playerScript.targetVelocity.z) > 0.1f)
        {
            // Camera bobs when player moves
            timer += Time.deltaTime * rb.velocity.magnitude * (bobbingSpeed * Mathf.PI);
            cam.localPosition = new Vector3(cam.localPosition.x, defaultPosY + Mathf.Sin(timer) * bobbingAmount, cam.localPosition.z);
        }
        else
        {
            // Reset camera position
            timer = 0;
            cam.localPosition = new Vector3(cam.localPosition.x, Mathf.Lerp(cam.localPosition.y, defaultPosY, Time.deltaTime * bobbingSpeed), cam.localPosition.z);
        }
    }
}
