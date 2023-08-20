using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedHornetSpawn : MonoBehaviour
{
    [Header("References")]
    public GameObject spawnObject;
    public Material shaderMaterial;
    public Transform playerTransform;
    public List<Transform> spawnPosList;
    private List<Transform> spawnDistanceList = new List<Transform> ();
    [HideInInspector] public List<string> spawnedEnemiesList;

    [Header("Spawn Settings")]
    public float timer = 0;
    public float spawnInterval = 10;
    public int maxSpawnCount = 5;
    public float distance = 20f;
    Quaternion rot;
    // Start is called before the first frame update
    void Start()
    {
        rot = spawnObject.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        CheckDistance();

        SpawnEnemy();
    }

    void CheckDistance()
    {
        foreach (Transform t in spawnPosList)
        {
            float d = Vector3.Distance(playerTransform.position, t.position);

            if (d > distance && !spawnDistanceList.Contains(t))
            {
                spawnDistanceList.Add(t);
            }
            else if (d <= distance && spawnDistanceList.Contains(t))
            {
                spawnDistanceList.Remove(t);
            }
        }
    }

    void SpawnEnemy()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && spawnedEnemiesList.Count < maxSpawnCount)
        {
            // Random number
            int index = UnityEngine.Random.Range(0, spawnDistanceList.Count);

            // Spawn enemy and set references
            Enemy newEnemy = Instantiate(spawnObject, spawnDistanceList[index].position, rot).GetComponent<Enemy>();
            newEnemy.targetTransform = playerTransform;
            newEnemy.SetupMaterial(new Material(shaderMaterial));

            // Create and assign guid to enemy to keep track
            string newGUID = Guid.NewGuid().ToString();
            newEnemy.guid = newGUID;

            spawnedEnemiesList.Add(newGUID);

            timer = 0;
        }
    }

    private void OnDrawGizmos()
    {
        foreach (Transform t in spawnPosList)
        {
            if (spawnDistanceList.Contains(t))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(t.position, playerTransform.position);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(t.position, playerTransform.position);
            }
        }
    }
}
