using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;

public class Enemy : MonoBehaviour
{
    [SerializeField] float resetTimer = 1f;
    private float currentResetTimer;
    public float disabletime;
    public float deathTime;
    public float lightTime;
    public float edgeWidth = 0.5f;
    private Material deathMat;
    private NavMeshAgent agent;
    private Animator animator;
    AudioSource audioSource;
    public AudioClip deathgroanClip;
    public AudioClip killClip;
    public AudioClip scuttle1;
    public AudioClip scuttle2;
    public AudioClip scuttle3;
    public AudioClip hissClip;
    int scuttleClipNumber = 1;
    public float scuttleTimer = 0;
    public float scuttleTimerMax = 4;
    public float rotationAngle = 50;

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

        audioSource = this.GetComponent<AudioSource>();


        audioSource.clip = RandomiseScuttleClip();
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        scuttleTimer += Time.deltaTime;

        distanceToPlayer = Vector3.Distance(targetTransform.position, this.transform.position);

        if (!isDead && !isCringeAnimator && agent.isOnNavMesh)
        {
            agent.SetDestination(targetTransform.position);
            if(!audioSource.isPlaying && distanceToPlayer <= 4)
            {
                audioSource.clip = hissClip;
                audioSource.Play();
            }
            else if(!audioSource.isPlaying && scuttleTimer >= scuttleTimerMax)
            {
                audioSource.clip = RandomiseScuttleClip();
                audioSource.Play();
                scuttleTimerMax = Random.Range(4, 6);
                scuttleTimer = 0;
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
        animator.SetBool("IsLightStillOn", true);
        if (!audioSource.isPlaying)
        {
            audioSource.clip = deathgroanClip;
            audioSource.Play();
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

    AudioClip RandomiseScuttleClip()
    {
        scuttleClipNumber = Random.Range(1, 3);
        if (scuttleClipNumber == 1)        
            return scuttle1;
        
        if (scuttleClipNumber == 2)        
            return scuttle2;
        
        if (scuttleClipNumber == 3)        
            return scuttle3;
        
        return scuttle1;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            audioSource.clip = killClip;

        }
    }

 



    
}
