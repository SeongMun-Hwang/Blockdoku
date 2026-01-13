using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class BlockPrefabCreatorWindow : EditorWindow
{
    private string prefabName = "UI_Block_New";
    private List<string> shapeRows = new List<string> { "1" };
    private Vector2 scrollPosition;

    [MenuItem("Tools/Block Prefab Creator")]
    public static void ShowWindow()
    {
        GetWindow<BlockPrefabCreatorWindow>("Block Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Block Prefab Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);

        EditorGUILayout.Space();
        GUILayout.Label("Shape (use '1' for cells, '0' for empty):", EditorStyles.label);

        // --- Shape Rows Editor ---
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
        for (int i = 0; i < shapeRows.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            shapeRows[i] = EditorGUILayout.TextField(shapeRows[i]);
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                shapeRows.RemoveAt(i);
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add Row"))
        {
            shapeRows.Add("");
        }
        // --- End Shape Rows Editor ---

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Prefab"))
        {
            CreateBlockPrefab();
        }
    }

    private void CreateBlockPrefab()
    {
        // --- Input Validation ---
        if (string.IsNullOrWhiteSpace(prefabName))
        {
            EditorUtility.DisplayDialog("Error", "Prefab name cannot be empty.", "OK");
            return;
        }
        if (shapeRows.Count == 0 || shapeRows.Any(r => string.IsNullOrWhiteSpace(r)))
        {
            EditorUtility.DisplayDialog("Error", "Shape rows cannot be empty.", "OK");
            return;
        }
        int expectedCols = shapeRows[0].Length;
        if (shapeRows.Any(r => r.Length != expectedCols))
        {
            EditorUtility.DisplayDialog("Error", "All shape rows must have the same length.", "OK");
            return;
        }

        // --- Prefab Creation Logic ---
        GameObject cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Cell.prefab");
        if (cellPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Cell prefab not found at 'Assets/Prefabs/UI/Cell.prefab'. Please create it first.", "OK");
            return;
        }

        GameObject rootObject = new GameObject(prefabName);
        int rows = shapeRows.Count;
        int cols = expectedCols;

        // Configure GridLayoutGroup
        GridLayoutGroup gridLayout = rootObject.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(100, 100);
        gridLayout.spacing = new Vector2(5, 5);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cols;

        // Add cells or spacers based on shape
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (shapeRows[r][c] == '1')
                {
                    GameObject cell = (GameObject)PrefabUtility.InstantiatePrefab(cellPrefab);
                    cell.transform.SetParent(rootObject.transform, false);
                }
                else
                {
                    // Add a spacer for empty parts of the shape
                    GameObject spacer = new GameObject("Spacer");
                    spacer.AddComponent<RectTransform>();
                    Image spacerImage = spacer.AddComponent<Image>();
                    spacerImage.color = Color.clear;
                    spacerImage.raycastTarget = false;
                    spacer.transform.SetParent(rootObject.transform, false);
                }
            }
        }
        
        // Resize RectTransform
        RectTransform rt = rootObject.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(cols * 100 + (cols - 1) * 5, rows * 100 + (rows - 1) * 5);

        // Add UIBlock and save shape
        UIBlock uiBlock = rootObject.AddComponent<UIBlock>();
        uiBlock.shapeRows = new List<string>(shapeRows);

        // Add CanvasGroup for drag/drop
        rootObject.AddComponent<CanvasGroup>();
        rootObject.AddComponent<UIDragDrop>();

        // --- Save as Prefab ---
        string path = $"Assets/Prefabs/UI/Blocks/{prefabName}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
             if (!EditorUtility.DisplayDialog("Warning", $"Prefab '{prefabName}' already exists. Overwrite?", "Yes", "No"))
             {
                 DestroyImmediate(rootObject);
                 return;
             }
        }
        
        PrefabUtility.SaveAsPrefabAsset(rootObject, path);
        DestroyImmediate(rootObject);

        EditorUtility.DisplayDialog("Success", $"Prefab '{prefabName}' created successfully!", "OK");
    }
}
