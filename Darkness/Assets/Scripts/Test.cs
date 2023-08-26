using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    MeshRenderer rend;
    Material mat;

    float currentTime = 3f;

    // Start is called before the first frame update
    void Start()
    {
        MeshRenderer rend = GetComponent<MeshRenderer>();
        mat = rend.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        mat.SetFloat("_EdgeWidth", currentTime -= Time.deltaTime);
    }
}
