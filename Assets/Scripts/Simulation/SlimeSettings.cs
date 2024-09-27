using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu()]
public class SlimeSettings : ScriptableObject
{
    //[Range(0, 1)] public float proportionAgentSpeedWhenLost;

    public SpeciesSettings[] speciesSettings;

    [System.Serializable]
    public struct SpeciesSettings
    {
        //[Range(0, 1024)] public int agentImpactFactor;
        //[Range(0, 2)] public float anticipationFactor;

        [Header("Movement Settings")]
        [Range(0, 80)] public float moveSpeed;

        [Header("View Settings")]
        [Range(0, 180)] public int viewAngle;
        [Range(0, 50)] public int depthViewOffset;
        [Range(1, 7)] public int viewSensorWidth;

        [Header("Display settings")]
        public Color colour;
    }
}
