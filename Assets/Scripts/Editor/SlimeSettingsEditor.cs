using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(SlimeSettings))]
public class SlimeSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SlimeSettings settings = (SlimeSettings)target;

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < settings.speciesSettings.Length; i++)
        {
            EditorGUILayout.LabelField("Species Settings " + i, EditorStyles.boldLabel);

            //settings.speciesSettings[i].agentImpactFactor = EditorGUILayout.IntSlider("Agent Impact Factor", settings.speciesSettings[i].agentImpactFactor, 0, 2048);
            settings.speciesSettings[i].moveSpeed = EditorGUILayout.Slider("Move Speed", settings.speciesSettings[i].moveSpeed, 0f, 200f);
            //settings.speciesSettings[i].turnSpeed = EditorGUILayout.Slider("Turn Speed", settings.speciesSettings[i].turnSpeed, 0f, 180f);
            settings.speciesSettings[i].viewAngle = EditorGUILayout.IntSlider("View Angle", settings.speciesSettings[i].viewAngle, 0, 180);
            settings.speciesSettings[i].depthViewOffset = EditorGUILayout.IntSlider("Depth View Offset", settings.speciesSettings[i].depthViewOffset, 0, 100);

            // Custom control for sensorCount
            int newSensorWidth = EditorGUILayout.IntSlider("View Sensor Width", settings.speciesSettings[i].viewSensorWidth, 1, 9);
            if (newSensorWidth % 2 == 0) // Ensure it's an odd value
            {
                if (newSensorWidth == 0)
                    newSensorWidth = 1; // Make sure it's not zero
                else
                    newSensorWidth--; // Round down to the nearest odd value
            }
            settings.speciesSettings[i].viewSensorWidth = newSensorWidth;

            // Custom control for sensorSize
            //int newSensorSize = EditorGUILayout.IntSlider("Sensor Size", settings.speciesSettings[i].sensorSize, 1, 9);
            //if (newSensorSize % 2 == 0) // Ensure it's an odd value
            //{
            //    if (newSensorSize == 0)
            //        newSensorSize = 1; // Make sure it's not zero
            //    else
            //        newSensorSize--; // Round down to the nearest odd value
            //}
            //settings.speciesSettings[i].sensorSize = newSensorSize;

            // Custom control for sensorCount
            //int newSensorCount = EditorGUILayout.IntSlider("Sensor Count", settings.speciesSettings[i].sensorCount, 1, 7);
            //if (newSensorCount % 2 == 0) // Ensure it's an odd value
            //{
            //    if (newSensorCount == 0)
            //        newSensorCount = 1; // Make sure it's not zero
            //    else
            //        newSensorCount--; // Round down to the nearest odd value
            //}
            //settings.speciesSettings[i].sensorCount = newSensorCount;

            settings.speciesSettings[i].colour = EditorGUILayout.ColorField("Species color", settings.speciesSettings[i].colour);

            EditorGUILayout.Space();
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(settings);
        }
    }
}
