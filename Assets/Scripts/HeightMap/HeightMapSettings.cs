using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : ScriptableObject
{
    [Header("HeightMap Settings")]
    [Range(0, 5)] public float XOffset;
    [Range(0, 5)] public float YOffset;

    public Octave[] octaves;

    [System.Serializable]
    public struct Octave
    {
        [Range(0, 20)] public float frequency;
        [Range(0, 1)] public float amplitude;
    }
}
