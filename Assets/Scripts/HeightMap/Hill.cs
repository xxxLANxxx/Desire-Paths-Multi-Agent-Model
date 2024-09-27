using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Hill
{
    public Vector2 coord;
    public float width;
    public float amplitude;

    public Hill(Vector2 coord, float width, float amplitude)
    {
        this.coord = coord;
        this.width = width;
        this.amplitude = amplitude;

    }
}
