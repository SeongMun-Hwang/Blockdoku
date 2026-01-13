using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Create2DScene
{
    [MenuItem("Tools/Create 2D Scene")]
    public static void CreateScene()
    {
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create a Canvas
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        // Create the GameBoard
        GameObject gameBoardObject = new GameObject("GameBoard");
        gameBoardObject.transform.SetParent(canvasObject.transform);
        RectTransform gameBoardRect = gameBoardObject.AddComponent<RectTransform>();
        gameBoardRect.anchorMin = new Vector2(0.5f, 0.5f);
        gameBoardRect.anchorMax = new Vector2(0.5f, 0.5f);
        gameBoardRect.pivot = new Vector2(0.5f, 0.5f);
        gameBoardRect.sizeDelta = new Vector2(900, 900); // 9x9 board with 100x100 cells
        gameBoardRect.anchoredPosition = Vector2.zero;


        GridLayoutGroup gridLayout = gameBoardObject.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(100, 100);
        gridLayout.spacing = new Vector2(5, 5);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 9;

        // Create a Cell prefab
        GameObject cellObject = new GameObject("Cell");
        cellObject.AddComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        Image cellImage = cellObject.AddComponent<Image>();
        cellImage.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray
        cellObject.AddComponent<Cell>();

        // Create a prefab from the cell
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }
        PrefabUtility.SaveAsPrefabAsset(cellObject, "Assets/Prefabs/UI/Cell.prefab");
        Object.DestroyImmediate(cellObject);

        // Populate the board with cells
        for (int i = 0; i < 81; i++)
        {
            GameObject cell = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Cell.prefab"));
            cell.transform.SetParent(gameBoardObject.transform);
        }

        // Save the scene at the end
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/SingleGame_2D.unity");
    }
}
