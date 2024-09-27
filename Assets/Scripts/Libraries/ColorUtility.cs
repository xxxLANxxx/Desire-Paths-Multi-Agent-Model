using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorUtility
{
    public static ColorHSV LerpHSV(ColorHSV a, ColorHSV b, float t)
    {
        // Hue interpolation
        float h;
        float d = b.h - a.h;

        if (a.h > b.h)
        {
            // Swap (a.h, b.h)
            var hTemp = b.h;
            b.h = a.h;
            a.h = hTemp;
            d = -d;
            t = 1 - t;
        }

        if (d > 0.5f) // 180deg
        {
            a.h = a.h + 1; // 360deg
            h = (a.h + t * (b.h - a.h)) % 1; // 360deg
        }

        else // 180deg
        {
            h = a.h + t * d;
        }

        // Interpolates the rest
        return new ColorHSV
        (
            h,            // H
            a.s + t * (b.s - a.s),    // S
            a.v + t * (b.v - a.v),    // V
            a.a + t * (b.a - a.a)    // A
        );

    }
}
