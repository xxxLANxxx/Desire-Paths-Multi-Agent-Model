using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


[CustomEditor(typeof(Simulation))]
public class SimulationEditor : Editor
{
    Editor settingsEditor;
    bool settingsFoldout;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Simulation sim = target as Simulation;

        // Calculate the progress for the progress bar
        float progress = (float)sim.numIt / sim.numItPerGeneration;

        //// Use GUILayoutUtility.GetRect to get a rect for the progress bar
        //Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
        //EditorGUI.ProgressBar(rect, progress, "Progress In Gen");

        // Draw custom colored progress bar
        DrawProgressBar(progress, "Progress In Generation",
            //new Color(26.0f/255.0f, 56.0f/255.0f, 99.0f/255.0f, 1.0f)
            new Color(0.0f / 255.0f, 150.0f / 255.0f, 0.0f / 255.0f, 1.0f)
            );

        if (sim.slimeSettings != null)
        {
            DrawSettingsEditor(sim.generalSettings, ref settingsFoldout, ref settingsEditor);
            //DrawSettingsEditor(sim.trailSettings, ref settingsFoldout, ref settingsEditor);
            DrawSettingsEditor(sim.slimeSettings, ref settingsFoldout, ref settingsEditor);
            DrawSettingsEditor(sim.distribSettings, ref settingsFoldout, ref settingsEditor);
            DrawSettingsEditor(sim.heightMapSettings, ref settingsFoldout, ref settingsEditor);

            EditorPrefs.SetBool(nameof(settingsFoldout), settingsFoldout);
        }
    }

    void DrawProgressBar(float value, string label, Color color)
    {
        // Reserve a rect for the progress bar
        Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");

        // Draw the background of the progress bar
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 1.0f));

        // Draw the filled part of the progress bar
        Rect fillRect = new Rect(rect.x, rect.y, rect.width * value, rect.height);
        EditorGUI.DrawRect(fillRect, color);

        // Draw the text over the progress bar
        EditorGUI.DropShadowLabel(rect, label);
    }

    void DrawSettingsEditor(Object settings, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            if (foldout)
            {
                CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();
            }

        }
    }

    private void OnEnable()
    {
        settingsFoldout = EditorPrefs.GetBool(nameof(settingsFoldout), false);
    }


}
