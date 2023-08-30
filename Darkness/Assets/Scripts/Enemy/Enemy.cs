using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float deathTime;
    private Material deathMat;
    private NavMeshAgent agent;
    AudioSource footSteps;
    public AudioClip otherClip;
    /*[HideInInspector]*/ public bool isDead;

    [HideInInspector] public Transform targetTransform;
    [HideInInspector] public string guid;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        SkinnedMeshRenderer meshRend = GetComponentInChildren<SkinnedMeshRenderer>();

        deathMat = new Material(meshRend.material);

        meshRend.material = deathMat;

        footSteps = this.GetComponent<AudioSource>();
        footSteps.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    
        if (!isDead && agent.isOnNavMesh)
        {
            agent.SetDestination(targetTransform.position);
            if(!footSteps.isPlaying)
            {
                //footSteps.Play();

            }

        }
        else if (isDead && agent.isOnNavMesh)
        {
            agent.SetDestination(transform.position);
            footSteps.clip = otherClip;
            footSteps.Play();

        }
            
        
    }

    public IEnumerator Death(FixedHornetSpawn spawnScript)
    {
        spawnScript.spawnedEnemiesList.Remove(guid);

        float currentTime = 0f;

        while (currentTime < deathTime)
        {
            currentTime += Time.deltaTime;

            deathMat.SetFloat("_Dissolve", Mathf.Clamp01(currentTime / deathTime));
            
            yield return null;
        }
        
        if (transform.gameObject != null)
            Destroy(transform.gameObject);

    }
}
