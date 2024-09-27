
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtility
{
    public static float CalculateArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return 0.5f * Mathf.Abs(a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));
    }

    public static Vector2 ConvertRelativeToAbsoluteLocationOnGrid(Vector2 location, int width, int height)
    {
        return new Vector2(
            (int)(location.x * width),
            (int)(location.y * height)
            );
    }

}



