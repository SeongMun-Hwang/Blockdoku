using UnityEditor;
using UnityEngine;
using System.IO;

public class SaveDataEditor : EditorWindow
{
    private Vector2 scrollPosition;

    [MenuItem("Custom Tools/Save Data Manager")]
    public static void ShowWindow()
    {
        GetWindow<SaveDataEditor>("Save Data Manager");
    }

    void OnGUI()
    {
        GUILayout.Label("Save Data Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawDeleteButton("Board Data", SavePaths.BoardDataPath);
        DrawDeleteButton("Block Data", SavePaths.BlockDataPath);
        DrawDeleteButton("Personal Data (Best Score)", SavePaths.PersonalDataPath);
        DrawDeleteButton("Setting Data (Audio Mute)", SavePaths.SettingDataPath);

        EditorGUILayout.Space();

        GUI.color = Color.red;
        if (GUILayout.Button("DELETE ALL SAVE DATA", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm Delete All",
                                            "Are you sure you want to delete ALL save data? This cannot be undone.",
                                            "Delete All", "Cancel"))
            {
                DeleteFile(SavePaths.BoardDataPath);
                DeleteFile(SavePaths.BlockDataPath);
                DeleteFile(SavePaths.PersonalDataPath);
                DeleteFile(SavePaths.SettingDataPath);
                Debug.Log("ALL save data deleted.");
            }
        }
        GUI.color = Color.white; // Reset color

        EditorGUILayout.EndScrollView();
    }

    void DrawDeleteButton(string label, string path)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label + ":", path);
        if (File.Exists(path))
        {
            if (GUILayout.Button("Delete", GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Confirm Delete",
                                                $@"Are you sure you want to delete {label}?
Path: {path}
This cannot be undone.",
                                                "Delete", "Cancel"))
                {
                    DeleteFile(path);
                    Debug.Log($"{label} deleted: {path}");
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField("File does not exist.", EditorStyles.miniLabel);
        }
        GUILayout.EndHorizontal();
    }

    void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            AssetDatabase.Refresh(); // Refresh the Asset Database to reflect changes
        }
    }
}
