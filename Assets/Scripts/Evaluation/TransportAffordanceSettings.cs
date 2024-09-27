using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TransportAffordanceSettings : ScriptableObject
{
    //[Range(0, 1)] public float trailThreshold;

    //public Color[] evalAffordColor;

    public TransportMode[] transportModes;

    [System.Serializable]
    public struct TransportMode
    {
        public Color affColor;
        [Range(0, 1)] public float minAffTh;
        [Range(0, 1)] public float maxAffTh;
        [Range(0, 200)] public float TMSpeed;
        [Range(0, 1)] public float numPassing;
        //[Range(0, 10)] public float useCost;
        //[Range(0, 10)] public float infraCost;
    }
}
