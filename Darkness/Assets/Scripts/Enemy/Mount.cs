using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Mount : MonoBehaviour
{
    [HideInInspector] public Transform targetTransform;
    Transform camGuide;

    public void MountCamera()
    {
        camGuide = GetComponentInChildren<EventSystem>().transform;

        Transform camTransform = targetTransform.GetComponentInChildren<Camera>().transform;
        camTransform.SetParent(camGuide);
        camTransform.rotation = Quaternion.Euler(0, 180f, 0);
    }

    void Bite()
    {
        this.GetComponentInParent<AudioSource>().Play();
    }
}
