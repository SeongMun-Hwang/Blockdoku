using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner_2D : MonoBehaviour
{
    public static BlockSpawner_2D Instance { get; private set; }

    [Header("Spawning Configuration")]
    [SerializeField] private GameObject blockContainerPrefab; // The empty container prefab with Block_2D script
    [SerializeField] private List<BlockArray> blockArrays; // List of all possible block shapes
    [SerializeField] private List<Transform> spawnPositions;

    private readonly List<GameObject> spawnedBlocks = new List<GameObject>();

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
        SpawnBlocks();
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

        // Create a list of random, unique indices from the blockArrays list
        HashSet<int> randomIndexes = new HashSet<int>();
        while (randomIndexes.Count < spawnPositions.Count && randomIndexes.Count < blockArrays.Count)
        {
            randomIndexes.Add(Random.Range(0, blockArrays.Count));
        }

        int i = 0;
        foreach (int index in randomIndexes)
        {
            Transform spawnPos = spawnPositions[i];
            
            // Instantiate the empty container
            GameObject blockGO = Instantiate(blockContainerPrefab, spawnPos.position, Quaternion.identity, spawnPos);
            
            // Get the script and initialize it with data and a random rotation
            Block_2D blockScript = blockGO.GetComponent<Block_2D>();
            if (blockScript != null)
            {
                int randomRot = Random.Range(0, 4);
                blockScript.Initialize(blockArrays[index], randomRot);
            }
            else
            {
                Debug.LogError($"BlockSpawner_2D: The prefab '{blockContainerPrefab.name}' is missing the Block_2D script.");
            }

            spawnedBlocks.Add(blockGO);
            i++;
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
}
