using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<Reveal>().canReveal = true;

            other.gameObject.name = "entered";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<Reveal>().canReveal = false;

            other.gameObject.name = "exit";
        }
    }
}
