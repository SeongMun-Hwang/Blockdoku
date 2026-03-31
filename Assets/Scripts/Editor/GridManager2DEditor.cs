using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager_2D))]
public class GridManager2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields
        DrawDefaultInspector();

        GridManager_2D manager = (GridManager_2D)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Symmetry Effect Debug Tools", EditorStyles.boldLabel);
        
        // Buttons only work in Play Mode
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);

        if (GUILayout.Button("Test H-Symmetry Effect (Left-Right)"))
        {
            manager.PlaySymmetryAnimation("H");
        }

        if (GUILayout.Button("Test V-Symmetry Effect (Top-Bottom)"))
        {
            manager.PlaySymmetryAnimation("V");
        }

        if (GUILayout.Button("Test Diagonal 1 Effect (TL to BR)"))
        {
            manager.PlaySymmetryAnimation("D1");
        }

        if (GUILayout.Button("Test Diagonal 2 Effect (TR to BL)"))
        {
            manager.PlaySymmetryAnimation("D2");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Full Clear Effect Debug Tool", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Test Full Clear Effect"))
        {
            manager.PlayFullClearAnimation();
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Symmetry effects can only be tested during Play Mode.", MessageType.Info);
        }

        EditorGUI.EndDisabledGroup();
    }
}
