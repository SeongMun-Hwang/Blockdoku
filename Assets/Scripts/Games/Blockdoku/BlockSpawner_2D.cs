using System.Collections.Generic;
using UnityEngine;
using System.IO;

using static SavePaths;

public class BlockSpawner_2D : MonoBehaviour
{
    public static BlockSpawner_2D Instance { get; private set; }

    [Header("Spawning Configuration")]
    [SerializeField] private GameObject blockContainerPrefab; // The empty container prefab with Block_2D script
    [SerializeField] private List<BlockArray> blockArrays; // List of all possible block shapes
    [SerializeField] private List<Transform> spawnPositions;

    private readonly List<GameObject> spawnedBlocks = new List<GameObject>();

    [System.Serializable]
    public class BlockSaveData_2D
    {
        public int blockArrayIndex;
        public int spawnIndex;
        public int rotationStep;
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

            // 1. Generate a set of 3 temporary blocks
            HashSet<int> randomIndexes = new HashSet<int>();
            while (randomIndexes.Count < spawnPositions.Count && randomIndexes.Count < blockArrays.Count)
            {
                randomIndexes.Add(Random.Range(0, blockArrays.Count));
            }

            foreach (int index in randomIndexes)
            {
                // Instantiate the container, but keep it inactive and out of sight
                GameObject blockGO = Instantiate(blockContainerPrefab, new Vector3(-1000, -1000, 0), Quaternion.identity);
                blockGO.SetActive(false); // Keep it inactive for now
                Block_2D blockScript = blockGO.GetComponent<Block_2D>();

                int randomRot = Random.Range(0, 4);
                blockScript.Initialize(blockArrays[index], randomRot);
                potentialBlocks.Add(blockScript);
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

    public void BlockPlaced(GameObject blockGO)
    {
        spawnedBlocks.Remove(blockGO);
        
        if (spawnedBlocks.Count == 0)
        {
            SpawnBlocks();
        }
        else
        {
            if (GameManager_2D.Instance != null)
            {
                GameManager_2D.Instance.SaveGameData(); // Save game data after a block is placed
            }
            else
            {
                Debug.LogWarning("GameManager_2D.Instance is null. Cannot save game data.");
            }
            // After a block is placed, check if any of the remaining blocks can be placed.
            // If not, it's game over.
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

            blockSaveDatas.blocks.Add(new BlockSaveData_2D
            {
                blockArrayIndex = blockArrayIndex,
                spawnIndex = spawnIndex,
                rotationStep = rotationStep
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
                    blockScript.Initialize(blockArray, data.rotationStep);
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
