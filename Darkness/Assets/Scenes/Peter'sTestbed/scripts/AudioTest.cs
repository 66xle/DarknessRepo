using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour
{
    public AudioSource footsteps;
    public AudioClip deadman;
    public float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        footsteps = this.GetComponent<AudioSource>();
        footsteps.Play();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        //if (timer > 5)
        //{
        //    footsteps.clip = deadman;
        //    footsteps.Play();
        //    timer = -100;
        //}
    }
}
