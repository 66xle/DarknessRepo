using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    [Tooltip("Max force applied to the player")] public float maxVelocityChange = 10f;

    [Header("Camera Settings")]
    public float mouseSensitivity = 5f;

    
    [Header("References")]
    public Transform playerHead;
    public GameManager gameManager;

    [HideInInspector]
    public Vector3 targetVelocity = Vector3.zero; // Made public for head bobbing

    #region Internal Variables

    Rigidbody rb;

    float xRotation = 0f;
    Vector2 mouseInput;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        if (gameManager.isPaused)
            return;

        Movement();
    }

    void LateUpdate()
    {
        if (gameManager.isPaused)
            return;

        LookAround();
    }

    void Movement()
    {
        // Calculate how fast we should be moving
        targetVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        #region Calculate Velocity

        // Player is moving diagonally
        if (targetVelocity.z == 1 && targetVelocity.x == 1 || targetVelocity.z == 1 && targetVelocity.x == -1 || targetVelocity.z == -1 && targetVelocity.x == 1 || targetVelocity.z == -1 && targetVelocity.x == -1)
        {
            targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed / 1.414214f; // Magic number
        }
        else
        {
            targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;
        }

        // Apply a force that attempts to reach our target velocity
        Vector3 velocity = rb.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        #endregion

        // Move Player
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void LookAround()
    {
        // Camera Look Around
        mouseInput = new Vector2(Input.GetAxis("Mouse X") * mouseSensitivity, Input.GetAxis("Mouse Y") * mouseSensitivity);

        xRotation -= mouseInput.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);


        // Camera Tilt
        float rotZ = -Input.GetAxis("Horizontal");

        // Look Up and Down
        Quaternion finalRot = Quaternion.Euler(xRotation, 0, 0);
        playerHead.localRotation = Quaternion.Lerp(playerHead.localRotation, finalRot, 1f);

        // Look Left/Right
        transform.Rotate(Vector3.up * mouseInput.x);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            gameManager.Death();
            // Play animation
        }
    }
}
