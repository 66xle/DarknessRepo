using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    [Tooltip("Max force applied to the player")] public float maxVelocityChange = 10f;

    [Header("Camera Settings")]
    public float mouseSensitivity = 5f;

    [Header("Death")]
    [SerializeField] float faceAIRotationSpeed = 5f;
    private bool enemyStartAttackAnim = false;
    private bool disablePlayerMovement = false;

    [Header("References")]
    public Transform playerHead;
    public MenuSystem menuSystem;

    

    [HideInInspector]
    public Vector3 targetVelocity = Vector3.zero; // Made public for head bobbing

    #region Internal Variables

    Rigidbody rb;

    float xRotation = 0f;
    Vector2 mouseInput;

    public GameObject spawnSystem;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        enemyStartAttackAnim = false;
        disablePlayerMovement = false;

        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        if (menuSystem.isPaused || disablePlayerMovement)
            return;

        Movement();
    }

    void LateUpdate()
    {
        if (menuSystem.isPaused || disablePlayerMovement)
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Vector3 dir = other.transform.position - transform.position;

            #region Raycast Check

            RaycastHit hit;

            if (Physics.Raycast(transform.position, dir, out hit, 10f))
            {
                if (!hit.collider.CompareTag("Enemy"))
                    return;
            }

            #endregion

            #region Audio

            // Disable audio
            other.GetComponent<AudioSource>().clip = other.GetComponent<Enemy>().killClip;
            StartCoroutine(DisableAudioSource());

            // Disable enemy audio
            for(int i = 0; i < spawnSystem.transform.childCount; i++)
            {
                spawnSystem.transform.GetChild(i).GetComponent<AudioSource>().enabled = false;
            }

            #endregion

            other.gameObject.GetComponent<BoxCollider>().enabled = false;
            other.gameObject.GetComponent<NavMeshAgent>().enabled = false;

            Animator animController = other.gameObject.GetComponentInChildren<Animator>();
            animController.SetTrigger("KillPlayer");

            rb.useGravity = false;
            disablePlayerMovement = true;
            transform.GetComponent<CapsuleCollider>().enabled = false;

            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            enemy.rotateToPlayer = true;

            StartCoroutine(LookAtHornet(enemy, animController, other.transform.position));
            StartCoroutine(enemy.RotateToPlayer());
        }
    }

    IEnumerator LookAtHornet(Enemy script, Animator animController, Vector3 enemyPos)
    {
        // AI
        Vector3 targetDir = enemyPos - transform.position;
        
        float angle = 0f;

        // Wait for attack animation to be played!
        while (!animController.GetCurrentAnimatorStateInfo(0).IsName("HornetKill"))
        {
            yield return new WaitForEndOfFrame();
        }

        animController.speed = 0f;
        rb.velocity = Vector3.zero;

        do
        {
            // If minotuar is on screen play animation
            if (RendererExtensions.IsVisibleFrom(transform.GetComponent<Renderer>(), transform.GetComponentInChildren<Camera>()) && !enemyStartAttackAnim || angle < 5f && !enemyStartAttackAnim)
            {
                animController.speed = 1f;
                enemyStartAttackAnim = true;
            }

            // Rotate player rotation to minotuar
            Vector3 playerHeadDir = transform.GetChild(0).forward;
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            Quaternion rot = Quaternion.Slerp(transform.GetChild(0).rotation, targetRot, Time.deltaTime * faceAIRotationSpeed);

            // Make camera not tilt
            rot.eulerAngles = new Vector3(rot.eulerAngles.x, rot.eulerAngles.y, 0f);

            transform.GetChild(0).rotation = rot;

            // Get angle between two directions
            angle = Mathf.Acos(Vector3.Dot(targetDir.normalized, playerHeadDir)) * Mathf.Rad2Deg;

            yield return new WaitForEndOfFrame();

        } while (angle > 2f);


        animController.speed = 1f;
        enemyStartAttackAnim = true;
    }


    IEnumerator DisableAudioSource()
    {
        yield return new WaitForSeconds(3.0f);

        for (int i = 0; i < spawnSystem.transform.childCount; i++)
        {
            spawnSystem.transform.GetChild(i).GetComponent<AudioSource>().enabled = false;
        }

        menuSystem.Death();
    }

}
