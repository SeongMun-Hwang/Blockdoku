using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq; // Needed for Distinct() in color selection

using static SavePaths;

public class BlockSpawner_2D : MonoBehaviour
{
    public static BlockSpawner_2D Instance { get; private set; }

    [System.Serializable]
    public struct HSVColor
    {
        [Range(0f, 1f)] public float h;
        [Range(0f, 1f)] public float s;
        [Range(0f, 1f)] public float v;
        public Color color; // For visual preview in Inspector

        public HSVColor(float h, float s, float v)
        {
            this.h = h;
            this.s = s;
            this.v = v;
            this.color = Color.HSVToRGB(h, s, v);
        }

        public Color ToColor()
        {
            return Color.HSVToRGB(h, s, v);
        }
    }

    [Header("Spawning Configuration")]
    [SerializeField] private GameObject blockContainerPrefab; // The empty container prefab with Block_2D script
    [SerializeField] private List<BlockArray> blockArrays; // List of all possible block shapes
    [SerializeField] private List<Transform> spawnPositions;

    [Header("Color Configuration")]
    [SerializeField] private List<HSVColor> availableColors = new List<HSVColor>()
    {
        new HSVColor(0.00f, 0.5f, 1.0f), // Soft Red
        new HSVColor(0.33f, 0.5f, 1.0f), // Soft Green
        new HSVColor(0.60f, 0.5f, 1.0f), // Soft Blue
        new HSVColor(0.13f, 0.5f, 1.0f), // Soft Yellow
        new HSVColor(0.80f, 0.5f, 1.0f), // Soft Magenta
        new HSVColor(0.08f, 0.5f, 1.0f), // Soft Orange
    };

    private readonly List<GameObject> spawnedBlocks = new List<GameObject>();

    [System.Serializable]
    public class BlockSaveData_2D
    {
        public int blockArrayIndex;
        public int spawnIndex;
        public int rotationStep;
        public SerializableColor blockColor; // New: To save block color
    }

    [System.Serializable]
    public class BlockSaveDatas_2D
    {
        public List<BlockSaveData_2D> blocks;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Blocks are spawned by GameManager_2D.LoadGameData() or GameManager_2D.StartGame()
        // if (File.Exists(SavePaths.BlockDataPath))
        // {
        //     LoadBlockData_2D();
        // }
        // else
        // {
        //     SpawnBlocks();
        // }
    }

    public void SpawnBlocks()
    {
        // Clear any existing blocks first
        foreach (GameObject block in spawnedBlocks)
        {
            if (block != null)
            {
                Destroy(block);
            }
        }
        spawnedBlocks.Clear();

        if (blockArrays.Count == 0 || spawnPositions.Count == 0)
        {
            Debug.LogError("BlockSpawner_2D: Block Arrays or Spawn Positions are not set.");
            return;
        }

        List<Block_2D> potentialBlocks = new List<Block_2D>();
        bool isPlaceableSetFound = false;

        while (!isPlaceableSetFound)
        {
            // Clear potential blocks from previous loop
            foreach (var block in potentialBlocks)
            {
                if (block != null) Destroy(block.gameObject);
            }
            potentialBlocks.Clear();

            // Generate unique colors for the blocks based on HSV
            List<Color> chosenColors = new List<Color>();
            List<HSVColor> tempAvailableColors = new List<HSVColor>(availableColors); // Copy to choose from

            for (int i = 0; i < spawnPositions.Count && tempAvailableColors.Count > 0; i++)
            {
                int colorIndex = Random.Range(0, tempAvailableColors.Count);
                HSVColor hsvColor = tempAvailableColors[colorIndex];

                chosenColors.Add(hsvColor.ToColor());
                tempAvailableColors.RemoveAt(colorIndex); // Ensure unique colors
            }

            // 1. Generate a set of 3 temporary blocks
            HashSet<int> randomIndexes = new HashSet<int>();
            while (randomIndexes.Count < spawnPositions.Count && randomIndexes.Count < blockArrays.Count)
            {
                randomIndexes.Add(Random.Range(0, blockArrays.Count));
            }

            int colorCounter = 0;
            foreach (int index in randomIndexes)
            {
                // Instantiate the container, but keep it inactive and out of sight
                GameObject blockGO = Instantiate(blockContainerPrefab, new Vector3(-1000, -1000, 0), Quaternion.identity);
                blockGO.SetActive(false); // Keep it inactive for now
                Block_2D blockScript = blockGO.GetComponent<Block_2D>();

                int randomRot = Random.Range(0, 4);
                // Pass the chosen color to the block's Initialize method
                blockScript.Initialize(blockArrays[index], randomRot, chosenColors[colorCounter]);
                potentialBlocks.Add(blockScript);
                colorCounter++;
            }

            // 2. Check if any block in the set is placeable
            foreach (var block in potentialBlocks)
            {
                if (GameManager_2D.Instance.CanBlockBePlaced(block))
                {
                    isPlaceableSetFound = true;
                    break; // Found a placeable block, so this set is valid
                }
            }
        }

        // 3. Once a valid set is found, spawn them properly
        for (int i = 0; i < potentialBlocks.Count; i++)
        {
            Block_2D blockScript = potentialBlocks[i];
            Transform spawnPos = spawnPositions[i];

            blockScript.transform.SetParent(spawnPos, false);
            blockScript.transform.position = spawnPos.position;
            blockScript.gameObject.SetActive(true);
            spawnedBlocks.Add(blockScript.gameObject);
        }


        // Save game state after new blocks are spawned
        if (GameManager_2D.Instance != null)
        {
            GameManager_2D.Instance.SaveGameData();
        }
    }

    public void BlockPlaced(GameObject blockGO, Vector2Int gridPosition, List<Vector2Int> shape, Color blockColor)
    {
        spawnedBlocks.Remove(blockGO);
        StartCoroutine(BlockPlacedRoutine(gridPosition, shape, blockColor));
    }

    private System.Collections.IEnumerator BlockPlacedRoutine(Vector2Int gridPosition, List<Vector2Int> shape, Color blockColor)
    {
        // Actually place the block on the grid
        int clearCount = GridManager_2D.Instance.PlaceBlock(gridPosition, shape, blockColor);

        if (clearCount > 0)
        {
            // Wait for sequential clear animations to finish
            float waitTime = clearCount * GridManager_2D.Instance.clearAnimationSequentialDelay + 0.4f;
            yield return new WaitForSeconds(waitTime);
        }

        if (spawnedBlocks.Count == 0)
        {
            SpawnBlocks();
        }
        else
        {
            if (GameManager_2D.Instance != null)
            {
                GameManager_2D.Instance.SaveGameData();
            }
            CheckForGameOver();
        }
    }

    public void CheckForGameOver()
    {
        GameManager_2D.Instance.CheckGameOver();
    }

    public List<GameObject> GetSpawnedBlocks()
    {
        return spawnedBlocks;
    }

    public void SaveBlockData_2D()
    {
        BlockSaveDatas_2D blockSaveDatas = new BlockSaveDatas_2D();
        blockSaveDatas.blocks = new List<BlockSaveData_2D>();

        foreach (GameObject go in spawnedBlocks)
        {
            Block_2D block = go.GetComponent<Block_2D>();
            if (block == null)
            {
                Debug.LogWarning($"BlockSpawner_2D: GameObject {go.name} in spawnedBlocks is missing Block_2D component. Skipping save.");
                continue;
            }
            int blockArrayIndex = blockArrays.IndexOf(block.BlockData);
            int spawnIndex = spawnPositions.IndexOf(go.transform.parent);
            int rotationStep = block.CurrentRotationStep;
            SerializableColor blockColor = block.blockColor; // Get the block's color

            blockSaveDatas.blocks.Add(new BlockSaveData_2D
            {
                blockArrayIndex = blockArrayIndex,
                spawnIndex = spawnIndex,
                rotationStep = rotationStep,
                blockColor = blockColor // Save the block's color
            });
        }
        string json = JsonUtility.ToJson(blockSaveDatas);
        File.WriteAllText(BlockDataPath, json);
        Debug.Log("2D Block data saved to " + BlockDataPath);
    }

    public void LoadBlockData_2D()
    {
        // Clear any existing blocks first
        foreach (GameObject block in spawnedBlocks)
        {
            if (block != null)
            {
                Destroy(block);
            }
        }
        spawnedBlocks.Clear();

        if (File.Exists(BlockDataPath))
        {
            string json = File.ReadAllText(BlockDataPath);
            BlockSaveDatas_2D blockSaveDatas = JsonUtility.FromJson<BlockSaveDatas_2D>(json);

            foreach (BlockSaveData_2D data in blockSaveDatas.blocks)
            {
                if (data.blockArrayIndex < 0 || data.blockArrayIndex >= blockArrays.Count)
                {
                    Debug.LogWarning($"BlockSpawner_2D: Block array index {data.blockArrayIndex} is out of bounds. Skipping block.");
                    continue;
                }
                BlockArray blockArray = blockArrays[data.blockArrayIndex];

                if (data.spawnIndex < 0 || data.spawnIndex >= spawnPositions.Count)
                {
                    Debug.LogWarning($"BlockSpawner_2D: Spawn index {data.spawnIndex} is out of bounds. Skipping block.");
                    continue;
                }
                Transform spawnPos = spawnPositions[data.spawnIndex];

                GameObject blockGO = Instantiate(blockContainerPrefab, spawnPos.position, Quaternion.identity, spawnPos);
                Block_2D blockScript = blockGO.GetComponent<Block_2D>();
                if (blockScript != null)
                {
                    // Pass the loaded color to the block's Initialize method
                    blockScript.Initialize(blockArray, data.rotationStep, data.blockColor);
                }
                else
                {
                    Debug.LogWarning($"BlockSpawner_2D: BlockGO {blockGO.name} is missing Block_2D component after instantiation. Skipping initialization.");
                }
                spawnedBlocks.Add(blockGO);
            }
            Debug.Log("2D Block data loaded from " + BlockDataPath);
        }
        else
        {
            Debug.Log("2D Block save file not found at " + BlockDataPath);
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(BlockSpawner_2D.HSVColor))]
public class HSVColorDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        UnityEditor.EditorGUI.BeginProperty(position, label, property);

        position = UnityEditor.EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        float fieldWidth = position.width / 4f;
        Rect hRect = new Rect(position.x, position.y, fieldWidth - 5, position.height);
        Rect sRect = new Rect(position.x + fieldWidth, position.y, fieldWidth - 5, position.height);
        Rect vRect = new Rect(position.x + fieldWidth * 2, position.y, fieldWidth - 5, position.height);
        Rect colorRect = new Rect(position.x + fieldWidth * 3, position.y, fieldWidth, position.height);

        var hProp = property.FindPropertyRelative("h");
        var sProp = property.FindPropertyRelative("s");
        var vProp = property.FindPropertyRelative("v");
        var colorProp = property.FindPropertyRelative("color");

        UnityEditor.EditorGUI.BeginChangeCheck();
        float h = UnityEditor.EditorGUI.FloatField(hRect, hProp.floatValue);
        float s = UnityEditor.EditorGUI.FloatField(sRect, sProp.floatValue);
        float v = UnityEditor.EditorGUI.FloatField(vRect, vProp.floatValue);
        if (UnityEditor.EditorGUI.EndChangeCheck())
        {
            hProp.floatValue = Mathf.Clamp01(h);
            sProp.floatValue = Mathf.Clamp01(s);
            vProp.floatValue = Mathf.Clamp01(v);
            colorProp.colorValue = Color.HSVToRGB(hProp.floatValue, sProp.floatValue, vProp.floatValue);
        }

        UnityEditor.EditorGUI.BeginChangeCheck();
        Color newColor = UnityEditor.EditorGUI.ColorField(colorRect, GUIContent.none, colorProp.colorValue, false, false, false);
        if (UnityEditor.EditorGUI.EndChangeCheck())
        {
            colorProp.colorValue = newColor;
            float newH, newS, newV;
            Color.RGBToHSV(newColor, out newH, out newS, out newV);
            hProp.floatValue = newH;
            sProp.floatValue = newS;
            vProp.floatValue = newV;
        }

        UnityEditor.EditorGUI.EndProperty();
    }
}
#endif
