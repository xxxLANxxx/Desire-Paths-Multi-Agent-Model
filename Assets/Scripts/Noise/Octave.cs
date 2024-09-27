using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Octave
{
    public float frequency;
    public float amplitude;

    public Octave(float frequency, float amplitude)
    {
        this.frequency = frequency;
        this.amplitude = amplitude;
    }
}
