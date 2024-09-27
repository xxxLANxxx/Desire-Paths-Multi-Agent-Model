using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GeneralSettings : ScriptableObject
{
    [Range(1, 120)] public int stepsPerFrame = 1;
    public int width = 2048;
    public int height = 1024;
    [Min(1)] public int numAgents = 1000;
    public SpawnMode spawnMode = SpawnMode.RandomSite;
}
