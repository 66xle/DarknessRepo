using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] float resetTimer = 1f;
    private float currentResetTimer;

    public float deathTime;
    public float edgeWidth = 0.5f;
    private Material deathMat;
    private NavMeshAgent agent;
    private Animator animator;
    AudioSource audioSource;
    public AudioClip deathgroanClip;
    public AudioClip cringeClip;
    public AudioClip hissClip1;
    public AudioClip hissClip2;
    public AudioClip hissClip3;
    int hissClipNumber = 1;
    public float hissTimer = 0;
    public float hissTimerMax = 4;
    /*[HideInInspector]*/ public bool isDead;
    private bool isCringeAnimator 
    { 
        get { return animator.GetBool("IsLightShownOn"); } 
        set { animator.SetBool("IsLightShownOn", value); } 
    }




    [HideInInspector] public Transform targetTransform;
    [HideInInspector] public string guid;

    [HideInInspector] public FixedHornetSpawn spawnScript;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        SkinnedMeshRenderer meshRend = GetComponentInChildren<SkinnedMeshRenderer>();

        deathMat = new Material(meshRend.material);

        meshRend.material = deathMat;

        audioSource = this.GetComponent<AudioSource>();


        audioSource.clip = RandomiseHissClip();
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        hissTimer += Time.deltaTime;
    
        if (!isDead && !isCringeAnimator && agent.isOnNavMesh)
        {
            agent.SetDestination(targetTransform.position);
            if(!audioSource.isPlaying && hissTimer >= hissTimerMax)
            {
                audioSource.clip = RandomiseHissClip();
                audioSource.Play();
                hissTimerMax = Random.Range(4, 6);
                hissTimer = 0;
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
        isDead = true;
        spawnScript.spawnedEnemiesList.Remove(guid);
        animator.SetTrigger("Death");

        GetComponent<BoxCollider>().enabled = false;


        float currentTime = 0f;

        deathMat.SetFloat("_EdgeWidth", edgeWidth);

        while (currentTime < deathTime)
        {
            currentTime += Time.deltaTime;

            deathMat.SetFloat("_Dissolve", Mathf.Clamp01(currentTime / deathTime));

            yield return null;
        }
        
        if (transform.gameObject != null)
            Destroy(transform.gameObject);

    }

    AudioClip RandomiseHissClip()
    {
        hissClipNumber = Random.Range(1, 3);
        if (hissClipNumber == 1)        
            return hissClip1;
        
        if (hissClipNumber == 2)        
            return hissClip2;
        
        if (hissClipNumber == 3)        
            return hissClip3;
        
        return hissClip1;
    }
}
