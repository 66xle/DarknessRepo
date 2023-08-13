using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] List<GameObject> enemyList;

    public void ToggleMeshRenderer(bool isVisorActive)
    {
        foreach (GameObject enemy in enemyList)
        {
            SkinnedMeshRenderer[] meshArr = enemy.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach(SkinnedMeshRenderer mesh in meshArr)
            {
                mesh.enabled = isVisorActive ? true : false;
            }
        }
    }
}
