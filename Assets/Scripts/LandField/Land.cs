using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Land
{
    public float affordanceIndex;
    public float appearanceThreshold;
    public float landXTh;
    public Color landColor;

    public Land(float affordanceIndex, float appearanceThreshold, float landXTh, Color landColor)
    {
        this.affordanceIndex = affordanceIndex;
        this.appearanceThreshold = appearanceThreshold;
        this.landXTh = landXTh; 
        this.landColor = landColor;

    }
}
