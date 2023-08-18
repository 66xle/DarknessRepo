using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedHornetSpawn : MonoBehaviour
{
    public Transform playerTransform;
    public GameObject spawnObject;
    public Material shaderMaterial;
    public List<Transform> spawnPosList;
    public List<string> spawnedEnemiesList;
    public float timer = 0;
    public float spawnInterval = 10;
    public int maxSpawnCount = 5;
    Quaternion rot;
    // Start is called before the first frame update
    void Start()
    {
        rot = spawnObject.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
      
        if (timer >= spawnInterval && spawnedEnemiesList.Count < maxSpawnCount)
        {
            int index = UnityEngine.Random.Range(0, spawnPosList.Count);

            Enemy newEnemy = Instantiate(spawnObject, spawnPosList[index].position, rot).GetComponent<Enemy>();
            newEnemy.targetTransform = playerTransform;
            newEnemy.SetupMaterial(new Material(shaderMaterial));

            string newGUID = Guid.NewGuid().ToString();
            newEnemy.guid = newGUID;

            spawnedEnemiesList.Add(newGUID);

            timer = 0;
        }
    }
}
