using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu()]
public class TrailSettings : ScriptableObject
{
    [Header("Trail Settings")]
    [Range(0, 1)] public float diffuseRate;
    [Range(0, 1)] public float decayRate;

}
