using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;

public class Enemy : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] float deathTime;
    [SerializeField] float lightTime;
    [SerializeField] float edgeWidth = 0.5f;
    

    [Header("Take Damage")]
    [SerializeField] float resetTimer = 1f;
    private float currentResetTimer;
    
    
    
    private int scuttleClipNumber = 1;
    [SerializeField] float scuttleTimer = 0;
    [SerializeField] float scuttleTimerMax = 4;
    [SerializeField] float rotationAngle = 50;

    [Header("Audio")]
    [SerializeField] AudioClip deathgroanClip;
    public AudioClip killClip;
    [SerializeField] List<AudioClip> hissSoundList;

    [Header("References")]
    public AudioSource hissAudioSource;
    public AudioSource walkAudioSource;

    private Material deathMat;
    private NavMeshAgent agent;
    private Animator animator;

    float distanceToPlayer;
    /*[HideInInspector]*/ public bool isDead;
    private bool isCringeAnimator 
    { 
        get { return animator.GetBool("IsLightShownOn"); } 
        set { animator.SetBool("IsLightShownOn", value); } 
    }

    [HideInInspector] public bool rotateToPlayer;

    [HideInInspector] public Transform rootJnt;


    [HideInInspector] public Transform targetTransform;
    [HideInInspector] public string guid;

    [HideInInspector] public FixedHornetSpawn spawnScript;

    private Light pointLight;

    // Start is called before the first frame update
    void Start()
    {
        GetComponentInChildren<Mount>().targetTransform = targetTransform;

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        pointLight = GetComponentInChildren<Light>();

        SkinnedMeshRenderer meshRend = GetComponentInChildren<SkinnedMeshRenderer>();

        deathMat = new Material(meshRend.material);

        meshRend.material = deathMat;
    }

    // Update is called once per frame
    void Update()
    {
        scuttleTimer += Time.deltaTime;

        

        if (!isDead && !isCringeAnimator && agent.isOnNavMesh)
        {
            agent.SetDestination(targetTransform.position);

            distanceToPlayer = Vector3.Distance(targetTransform.position, transform.position);
            if (!walkAudioSource.isPlaying && distanceToPlayer <= 4)
            {
                // play waling sound
                walkAudioSource.Play();
            }
            
           

        }
        else if (isDead && agent.isOnNavMesh || isCringeAnimator && agent.isOnNavMesh)
        {
            agent.SetDestination(transform.position);
           

        }


        ResetTimer();
    }
     
    void ResetTimer()
    {
        currentResetTimer -= Time.deltaTime;

        if (currentResetTimer <= 0f)
        {
            animator.SetBool("IsLightStillOn", false);
        }
    }

    public void TakeDamage()
    {
        currentResetTimer = resetTimer;

        isCringeAnimator = true;
        HissAtPlayer();

        animator.SetBool("IsLightStillOn", true);
    }


    public void HissAtPlayer()
    {
        if (!hissAudioSource.isPlaying)
        {
            int index = Random.Range(0, hissSoundList.Count);

            hissAudioSource.clip = hissSoundList[index];
            hissAudioSource.Play();
        }
    }

    public void StartDeathCoroutine()
    {
        StartCoroutine(Death());
    }

    IEnumerator Death()
    {
        // Light init
        pointLight.enabled = true;
        float maxIntensity = pointLight.intensity;

        // Death init
        isDead = true;
        spawnScript.spawnedEnemiesList.Remove(guid);
        animator.SetTrigger("Death");
        hissAudioSource.clip = deathgroanClip;
        hissAudioSource.Play();

        // Disable colldier to not trigger player death
        GetComponent<BoxCollider>().enabled = false;

        // Shader init
        float currentTime = 0f;
        deathMat.SetFloat("_EdgeWidth", edgeWidth);

        while (currentTime < deathTime)
        {
            // Dissolve
            currentTime += Time.deltaTime;
            deathMat.SetFloat("_Dissolve", Mathf.Clamp01(currentTime / deathTime));

            // Fade light
            float currentIntensity = maxIntensity / lightTime;
            pointLight.intensity -= currentIntensity * Time.deltaTime;

            yield return null;
        }
        
        if (transform.gameObject != null)
            Destroy(transform.gameObject);

    }

    public IEnumerator RotateToPlayer()
    {
        agent.updateRotation = false;

        while (rotateToPlayer)
        {
            Vector3 dir = targetTransform.position - transform.position;

            // Calculate angle from hornet to player
            float angle = Mathf.Acos(Vector3.Dot(transform.forward, dir.normalized)) * Mathf.Rad2Deg;

            // Rotation speed
            float speed = angle / rotationAngle;

            if (speed > 6f)
            {
                speed = 6f;
            }

            // Get Rotation
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * speed);

            yield return null;
        }


        agent.updateRotation = true;
    }

 



    
}
