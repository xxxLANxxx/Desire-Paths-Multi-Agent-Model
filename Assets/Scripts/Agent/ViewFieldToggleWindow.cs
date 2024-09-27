using UnityEngine;
using UnityEditor;

public class ViewFieldToggleWindow : EditorWindow
{
    private ViewField selectedViewField;

    [MenuItem("Window/Field Of View Toggle")]
    public static void ShowWindow()
    {
        GetWindow<ViewFieldToggleWindow>("Field Of View Toggle");
    }

    private void OnGUI()
    {
        // Show the field to select the ViewField object
        selectedViewField = (ViewField)EditorGUILayout.ObjectField("View Field", selectedViewField, typeof(ViewField), true);

        // Only show the toggle if a ViewField object is selected
        if (selectedViewField != null)
        {
            selectedViewField.showViewField = EditorGUILayout.Toggle("Show Field of View", selectedViewField.showViewField);

            // Mark the object as dirty to ensure the scene gets repainted when the value changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(selectedViewField);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please select a ViewField object.", MessageType.Warning);
        }
    }
}
