using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CustomEditor(typeof(ViewField))]
public class FieldOfViewEditor : Editor
{
    // Boolean to control the visibility of the field of view
    //private bool showFieldOfView = true;

    // Method to draw the custom inspector
    public override void OnInspectorGUI()
    {
        ViewField fow = (ViewField)target;

        // Draw the default inspector
        DrawDefaultInspector();

        // Add a toggle button to control the field of view visibility
        //showFieldOfView = EditorGUILayout.Toggle("Show Field of View", showFieldOfView);

        // Mark the object as dirty to ensure the scene gets repainted when the value changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(fow);
        }
    }

    void OnSceneGUI()
    {
        ViewField fow = (ViewField)target;

        if (fow.showViewField)
        {
            Handles.color = Color.white;

            Vector3 viewAngleLeft = fow.DirFromAngle(-fow.halfViewAngle + fow.directionAngle, false);
            Vector3 viewAngleRight = fow.DirFromAngle(fow.halfViewAngle + fow.directionAngle, false);

            DrawFieldOfView(fow, viewAngleLeft, viewAngleRight);

        }
    }

    private void DrawFieldOfView(ViewField fow, Vector3 viewAngleLeft, Vector3 viewAngleRight)
    {
        float segmentLength = 0.02f;
        if (fow.halfViewAngle == 180f)
        {
            if (fow.halfViewWidth >= fow.viewRadius)
            {
                Handles.DrawWireArc(fow.transform.position, Vector3.forward, viewAngleRight, 2 * fow.halfViewAngle, fow.viewRadius + fow.halfViewWidth, fow.lineThickness);
            }
            else
            {
                DrawDottedWireArc(fow.transform.position, Vector3.forward, viewAngleRight, 2 * fow.halfViewAngle, fow.viewRadius, fow.lineThickness, segmentLength);
                Handles.DrawWireArc(fow.transform.position, Vector3.forward, viewAngleRight, 2 * fow.halfViewAngle, fow.viewRadius + fow.halfViewWidth, fow.lineThickness);
                Handles.DrawWireArc(fow.transform.position, Vector3.forward, viewAngleRight, 2 * fow.halfViewAngle, fow.viewRadius - fow.halfViewWidth, fow.lineThickness);
            }

        }
        else
        {
            if (fow.halfViewWidth >= fow.viewRadius)
            {
                Handles.DrawWireArc(fow.transform.position, Vector3.forward, viewAngleRight, 2 * fow.halfViewAngle, fow.viewRadius + fow.halfViewWidth, fow.lineThickness);
                Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleLeft * (fow.viewRadius + fow.halfViewWidth), fow.lineThickness);
                Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleRight * (fow.viewRadius + fow.halfViewWidth), fow.lineThickness);
            }
            else
            {
                
                DrawDottedWireArc(fow.transform.position, Vector3.forward, viewAngleRight, 2 * fow.halfViewAngle, fow.viewRadius, fow.lineThickness, segmentLength);
                Handles.DrawWireArc(fow.transform.position, Vector3.forward, viewAngleRight, 2 * fow.halfViewAngle, fow.viewRadius + fow.halfViewWidth, fow.lineThickness);
                Handles.DrawWireArc(fow.transform.position, Vector3.forward, viewAngleRight, 2 * fow.halfViewAngle, fow.viewRadius - fow.halfViewWidth, fow.lineThickness);
                Handles.DrawLine(fow.transform.position + viewAngleLeft * (fow.viewRadius - fow.halfViewWidth), fow.transform.position + viewAngleLeft * (fow.viewRadius + fow.halfViewWidth), fow.lineThickness);
                Handles.DrawLine(fow.transform.position + viewAngleRight * (fow.viewRadius - fow.halfViewWidth), fow.transform.position + viewAngleRight * (fow.viewRadius + fow.halfViewWidth), fow.lineThickness);
            }
        }
    }

    private void DrawDottedWireArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, float thickness, float segmentLength)
    {
        float circumference = 2 * Mathf.PI * radius * (angle / 360f);
        int segments = Mathf.CeilToInt(circumference / segmentLength);
        float segmentAngle = angle / segments;

        for (int i = 0; i < segments; i++)
        {
            float currentAngle = i * segmentAngle;
            Vector3 start = center + Quaternion.AngleAxis(currentAngle, normal) * from * radius;
            float nextAngle = (i + 1) * segmentAngle;
            Vector3 end = center + Quaternion.AngleAxis(nextAngle, normal) * from * radius;

            if (i % 2 == 0) // Only draw every other segment to create a dotted effect
            {
                Handles.DrawLine(start, end, thickness);
            }
        }
    }
}