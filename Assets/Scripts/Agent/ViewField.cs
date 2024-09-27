using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ViewField : MonoBehaviour
{
    [Range(0f, 15f)]
    public float viewRadius = 2f;

    [Range(0f, 180f)]
    public float halfViewAngle = 90f;

    [Range(0f, 360f)]
    public float directionAngle = 90f;

    [Range(0f, 3f)]
    public float halfViewWidth = 1f;

    [Range(1f, 6f)]
    public float lineThickness = 1f;

    public bool showViewField;


    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }

}

