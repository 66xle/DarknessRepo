using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedHornetSpawn : MonoBehaviour
{
    public Transform playerTransform;
    public GameObject spawnObject;
    public List<Transform> spawnPosList;
    public float timer = 0;
    public float spawnInterval = 10;
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
      
        if (timer >= spawnInterval)
        {
            int index = Random.Range(0, spawnPosList.Count);

            GameObject newEnemy = Instantiate(spawnObject, spawnPosList[index].position, rot);
            newEnemy.GetComponent<Enemy>().targetTransform = playerTransform;

            timer = 0;
        }
    }
}
