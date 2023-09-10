using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField] GameManager gameManager;

    public void ConsoleClosedAnimation()
    {
        StartCoroutine(gameManager.MoveEnvironment());
    }
}
