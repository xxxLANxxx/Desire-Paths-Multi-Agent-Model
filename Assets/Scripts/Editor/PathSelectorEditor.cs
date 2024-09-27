using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathSelector))]
public class PathSelectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Reference to the PathSelector script
        PathSelector myScript = (PathSelector)target;

        // Begin a horizontal group
        EditorGUILayout.BeginHorizontal();

        // Draw the text field for the absoluteDataPath
        myScript.absoluteDataPath = EditorGUILayout.TextField("Absolute Data Path", myScript.absoluteDataPath);

        // Draw the button next to the text field
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", "", "C:/Users/CRL-louis/Documents/TDU/Research/LastSimData");
            if (!string.IsNullOrEmpty(path))
            {
                myScript.absoluteDataPath = path;
            }
        }

        // End the horizontal group
        EditorGUILayout.EndHorizontal();
    }
}