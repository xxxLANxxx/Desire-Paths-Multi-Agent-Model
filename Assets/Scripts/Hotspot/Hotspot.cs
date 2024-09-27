using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Hotspot
{
    public Vector2 location;
    public int visitFreq;
    public float attractiveness;

    public Hotspot(Vector2 location, int visitFreq, float attractiveness)
    {
        this.location = location;
        this.visitFreq = visitFreq;
        this.attractiveness = attractiveness;
    }
}
