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

    [HideInInspector] public bool isDead;

    [HideInInspector] public Transform targetTransform;
    [HideInInspector] public string guid;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        SkinnedMeshRenderer meshRend = GetComponentInChildren<SkinnedMeshRenderer>();

        deathMat = new Material(meshRend.material);

        meshRend.material = deathMat;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
            agent.SetDestination(targetTransform.position);
        else
            agent.SetDestination(transform.position);
    }

    public IEnumerator Death(FixedHornetSpawn spawnScript)
    {
        spawnScript.spawnedEnemiesList.Remove(guid);

        float currentTime = 0f;

        while (currentTime < deathTime)
        {
            currentTime += Time.deltaTime;

            deathMat.SetFloat("_Weight", Mathf.Clamp01(currentTime / deathTime));
            
            yield return null;
        }

        
        if (transform.gameObject != null)
            Destroy(transform.parent.gameObject);

    }
}
