using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu()]
public class HotspotDistribSettings : ScriptableObject
{
    [Range(1, 30)] public float scaleFactor;
    public Color displayColor;

    public HotspotSettings[] hotspotsSettings;

    [System.Serializable]
    public struct HotspotSettings
    {
        [Header("Site Settings")]
        public Vector2 location;
        [Range(1, 50)] public int visitFreq;
        [Range(1, 50)] public float attractiveness;
        public string name;
    }

}
