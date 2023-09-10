using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredDoor : MonoBehaviour
{
    Animator doorAnimator;
    AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    float timer = 3;
    bool countdown = false;

    // Start is called before the first frame update
    void Start()
    {
        doorAnimator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (countdown)
        {
            timer -= Time.deltaTime;
        }

        if(timer <= 0)
        {
            doorAnimator.SetBool("isOpen", false);
            audioSource.clip = closeSound;
            audioSource.Play();
            ResetTimer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            doorAnimator.SetBool("isOpen", true);
            audioSource.clip = openSound;
            audioSource.Play();
            countdown = false;
            ResetTimer();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            countdown = true;
        }
    }


    void ResetTimer()
    {
        countdown = false;
        timer = 3;
    }
}
