using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LandFieldSettings : ScriptableObject
{
    //public enum LandType { Grass, Soil, Sand, Road, Water }

    [Header("RandomMap Settings")]
    [Range(0, 5)] public float XOffset;
    [Range(0, 5)] public float YOffset;

    public Octave[] octaves;

    [Header("LandField Settings")]
    public Land[] landsSettings;

    [System.Serializable]
    public struct Octave
    {
        [Min(0)] public float frequency;
        [Range(0, 1)] public float amplitude;
    }

    [System.Serializable]
    public struct Land
    {
        [Range(0, 5)] public float landResistanceIndex;
        public float appearanceThreshold;
        [Range(0, 1)] public float landXTh;
        public Color landColor;
    }

    
}
