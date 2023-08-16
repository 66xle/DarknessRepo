using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedHornetSpawn : MonoBehaviour
{
    public GameObject spawnObject;
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
     

            Object.Instantiate(spawnObject, transform.position, rot);

            timer = 0;
        }
    }
}
