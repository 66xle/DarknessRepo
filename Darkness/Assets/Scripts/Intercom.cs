using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intercom : MonoBehaviour
{
    int initialize = 0;
    public int restartCount = 0;
    bool hasPlayed = false;

    public Transform player;

    public List<AudioClip> clipList;
    public AudioSource audioSource;

    public AudioClip hailClip;
    // Start is called before the first frame update
    void Start()
    {
        initialize = PlayerPrefs.GetInt("Initialize");
        if(initialize == 0)
        {
            PlayerPrefs.SetInt("Initialize", 1);

        }
        else
        {
            restartCount = PlayerPrefs.GetInt("restartCount");
            
            if (restartCount > 8)
                restartCount = 1;


            PlayerPrefs.SetInt("restartCount", restartCount);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > 4f)
            return;

        if(!audioSource.isPlaying && restartCount == 0 && !hasPlayed)
        {
            audioSource.clip = hailClip;
            audioSource.Play();
            hasPlayed = true;
        }
    }
}
