using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float deathTime;
    public float edgeWidth = 0.5f;
    private Material deathMat;
    private NavMeshAgent agent;
    private Animator animator;
    AudioSource footSteps;
    public AudioClip otherClip;
    /*[HideInInspector]*/ public bool isDead;

    [HideInInspector] public Transform targetTransform;
    [HideInInspector] public string guid;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        SkinnedMeshRenderer meshRend = GetComponentInChildren<SkinnedMeshRenderer>();

        deathMat = new Material(meshRend.material);

        meshRend.material = deathMat;
    }

    // Update is called once per frame
    void Update()
    {
        
    
        if (!isDead && agent.isOnNavMesh)
        {
            agent.SetDestination(targetTransform.position);
            //if(!footSteps.isPlaying)
            //{
            //    //footSteps.Play();

            //}

        }
        else if (isDead && agent.isOnNavMesh)
        {
            agent.SetDestination(transform.position);
            //footSteps.clip = otherClip;
            //footSteps.Play();

        }
            
        
    }

    public IEnumerator Death(FixedHornetSpawn spawnScript)
    {
        spawnScript.spawnedEnemiesList.Remove(guid);

        animator.SetTrigger("Death");

        float currentTime = 0f;

        deathMat.SetFloat("_EdgeWidth", edgeWidth);

        while (currentTime < deathTime)
        {
            currentTime += Time.deltaTime;

            deathMat.SetFloat("_Dissolve", Mathf.Clamp01(currentTime / deathTime));
            float percentage = currentTime / deathTime;

            


            yield return null;
        }
        
        if (transform.gameObject != null)
            Destroy(transform.gameObject);

    }
}
