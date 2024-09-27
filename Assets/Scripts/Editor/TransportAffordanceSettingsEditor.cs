using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(TransportAffordanceSettings))]
public class TransportAffordanceSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TransportAffordanceSettings settings = (TransportAffordanceSettings)target;

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < settings.transportModes.Length; i++)
        {
            EditorGUILayout.LabelField("Transport Modes " + i, EditorStyles.boldLabel);

            settings.transportModes[i].affColor = EditorGUILayout.ColorField("Transport Mode Color", settings.transportModes[i].affColor);

            // Input fields for min and max values
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Min Val:", GUILayout.Width(80));
            settings.transportModes[i].minAffTh = EditorGUILayout.FloatField(settings.transportModes[i].minAffTh, GUILayout.Width(80));
            EditorGUILayout.LabelField("Max Val:", GUILayout.Width(80));
            settings.transportModes[i].maxAffTh = EditorGUILayout.FloatField(settings.transportModes[i].maxAffTh, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            // MinMaxSlider for min and max values
            EditorGUILayout.MinMaxSlider(ref settings.transportModes[i].minAffTh, ref settings.transportModes[i].maxAffTh, 0f, 1f);

            // TM Speed Slider
            settings.transportModes[i].TMSpeed = EditorGUILayout.Slider("TM Speed", settings.transportModes[i].TMSpeed, 0, 180);

            // Num Passing Slider
            settings.transportModes[i].numPassing = EditorGUILayout.Slider("Num Passing", settings.transportModes[i].numPassing, 0, 100);

            EditorGUILayout.Space();
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(settings);
        }
    }
}
