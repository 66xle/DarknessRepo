using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HornetSpawningScript : MonoBehaviour
{
    public GameObject spawnObject;
    Vector3 spawnCenter;
    Vector3 spawnPos;
    public float radius;
    public float timer = 0;
    Quaternion rot;

    // Start is called before the first frame update
    void Start()
    {       
        spawnCenter = transform.position;
        rot = spawnObject.transform.rotation;
        spawnPos = new Vector3(0, transform.position.y, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
        timer += Time.deltaTime;
        Debug.DrawRay(transform.position, spawnPos.normalized);
        if (timer >= 0.5f)
        {
            RaycastHit hitInfo;
            RandomiseSpawnPos();
            
            if (Physics.Raycast(transform.position, spawnPos.normalized, out hitInfo, radius))
            {
                
                if (hitInfo.collider.CompareTag("Obstacle"))
                {
                    Debug.Log("obstacle is in the way");
                    timer = 0;
                    return;
                }
            }

            Object.Instantiate(spawnObject, spawnPos + spawnCenter, rot);

            timer = 0;
        }

    }

    void RandomiseSpawnPos()
    {
        float x = Random.Range(-radius,radius);
        float z = Random.Range(-radius,radius);
        spawnPos.x = x;
        spawnPos.z = z;
        spawnPos.Normalize();
        spawnPos *= Random.Range(0,radius);
        //spawnPos += spawnCenter;
    }
}
