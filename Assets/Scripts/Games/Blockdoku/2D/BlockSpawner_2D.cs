using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockSpawner_2D : MonoBehaviour
{
    public static BlockSpawner_2D Instance { get; private set; }
    
    [SerializeField] private List<GameObject> blockPrefabs;
    [SerializeField] private List<Transform> spawnPositions;

    private List<GameObject> spawnedBlocks = new List<GameObject>();

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
            Destroy(block);
        }
        spawnedBlocks.Clear();

        // Create a list of random, unique indices from the blockDataList
        HashSet<int> randomIndexes = new HashSet<int>();
        while (randomIndexes.Count < spawnPositions.Count)
        {
            randomIndexes.Add(Random.Range(0, blockPrefabs.Count));
        }

        int i = 0;
        foreach (int index in randomIndexes)
        {
            Transform spawnPos = spawnPositions[i];
            // Instantiate with no rotation, as visuals are now data-driven
            GameObject blockGO = Instantiate(blockPrefabs[index], spawnPos.position, Quaternion.identity, spawnPos);

            int randomRot = UnityEngine.Random.Range(0, 4);
            // RotateShape now handles both logic and visuals. No more direct transform rotation.
            if(randomRot > 0)
            {
                blockGO.GetComponent<Block_2D>().RotateShape(randomRot);
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
