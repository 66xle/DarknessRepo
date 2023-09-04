using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FixedHornetSpawn : MonoBehaviour
{
    [Header("References")]
    public GameObject spawnObject;
    public Transform playerTransform;
    public GameManager gameManager;

    private List<Transform> currentSpawnPosList = new List<Transform>();
    private List<Transform> spawnDistanceList = new List<Transform> ();
    [HideInInspector] public List<string> spawnedEnemiesList;

    [Header("Spawn Settings")]
    public float spawnInterval = 10;
    public int maxSpawnCount = 5;
    public float distance = 20f;
    private float timer = 0;
    Quaternion rot;

    [Header("Spawnpoint")]
    [SerializeField] Transform spawnPointGate1;
    [SerializeField] Transform spawnPointGate2;
    [SerializeField] Transform spawnPointGate3;
    private Transform gateParent;

    // Start is called before the first frame update
    void Start()
    {
        rot = spawnObject.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.canSpawnEnemy && !gameManager.isPaused)
        {
            CheckDistance();
            SpawnEnemy();
        }
    }

    public void LoadSpawnPoints(GateLevel.Gate gate)
    {
        currentSpawnPosList.Clear();

        if (gate == GateLevel.Gate.Gate1)
            gateParent = spawnPointGate1;
        else if (gate == GateLevel.Gate.Gate2)
            gateParent = spawnPointGate2;
        else if (gate == GateLevel.Gate.Gate3)
            gateParent = spawnPointGate3;

        for (int i = 0; i < gateParent.childCount; i++)
        {
            currentSpawnPosList.Add(gateParent.GetChild(i));
        }
    }

    void CheckDistance()
    {
        foreach (Transform t in currentSpawnPosList)
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

            GameObject enemyObject = Instantiate(spawnObject, spawnDistanceList[index].position, rot);
            enemyObject.transform.SetParent(transform);

            Enemy newEnemy = enemyObject.GetComponentInChildren<Enemy>();
            newEnemy.targetTransform = playerTransform;
            newEnemy.spawnScript = this;

            // Create and assign guid to enemy to keep track
            string newGUID = Guid.NewGuid().ToString();
            newEnemy.guid = newGUID;

            spawnedEnemiesList.Add(newGUID);

            timer = 0;
        }
    }

    private void OnDrawGizmos()
    {
        foreach (Transform t in currentSpawnPosList)
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
