using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GateLevel : ScriptableObject
{
    public enum Gate
    {
        Start,
        Gate1,
        Gate2,
        Gate3,
    }

    public enum Tasks
    {
        Fuse,
        Scan,
        StartElevator,
        RestartElevator
    }

    public float yAxis;
    public Gate currentGate = Gate.Gate1;
    public List<Tasks> taskList;

}
