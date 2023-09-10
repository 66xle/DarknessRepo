using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intercom : MonoBehaviour
{
    int initialize = 0;
    public int restartCount = 0;
    bool hasPlayed = false;

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
            restartCount++;
            PlayerPrefs.SetInt("restartCount", restartCount);
        }
        
        audioSource = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!audioSource.isPlaying && restartCount == 0 && !hasPlayed)
        {
            audioSource.clip = hailClip;
            audioSource.Play();
            hasPlayed = true;
        }
    }
}
