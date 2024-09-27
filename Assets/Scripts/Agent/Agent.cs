using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Agent
{
    public Vector2 position;
    public float angle;
    public Vector3Int speciesMask;
    int unusedSpeciesChannel;
    public int speciesIndex;

    public int siteIndexOrigin;
    public int siteIndexTarget;
    public int TMIndex;
    public int completeTravels;
    public int previousCompleteTravels;

    public Agent(Vector2 position, float angle, Vector3Int speciesMask, int unusedSpeciesChannel, int speciesIndex, int siteIndexTarget, int siteIndexOrigin, int TMIndex, int completeTravels, int previousCompleteTravels)
    {
        this.position = position;
        this.angle = angle;
        this.speciesMask = speciesMask;
        this.unusedSpeciesChannel = unusedSpeciesChannel;
        this.speciesIndex = speciesIndex;

        this.siteIndexOrigin = siteIndexOrigin;
        this.siteIndexTarget = siteIndexTarget;
        this.TMIndex = TMIndex;
        this.completeTravels = completeTravels;
        this.previousCompleteTravels = previousCompleteTravels;

    }
}
