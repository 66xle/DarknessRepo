using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    private float currentProgress = 0f;

    public float CurrentProgress { get { return currentProgress; } }

    bool isScanFinished = false;


    public void AddProgress(float progress)
    {
        currentProgress += progress * Time.deltaTime;
    }

    public bool IsScanFinished(float maxProgress)
    {
        if (currentProgress >= maxProgress)
        {
            isScanFinished = true;
            return isScanFinished;
        }

        return false;
    }
}
