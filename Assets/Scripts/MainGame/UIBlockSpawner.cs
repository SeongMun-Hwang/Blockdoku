using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIBlockSpawner : MonoBehaviour
{
    public static UIBlockSpawner Instance;

    [SerializeField] private List<GameObject> uiBlockPrefabs;
    [SerializeField] private List<RectTransform> spawnPositions;
    private List<GameObject> spawnedBlocks = new List<GameObject>();

    private void Awake()
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

    private void Start()
    {
        SpawnBlocks();
    }

    private void SpawnBlocks()
    {
        // Clear any remaining blocks just in case
        foreach (GameObject block in spawnedBlocks)
        {
            Destroy(block);
        }
        spawnedBlocks.Clear();

        List<int> randomIndexes = GetRandomPrefabIndexes();
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            if (i >= randomIndexes.Count) continue;

            GameObject block = Instantiate(uiBlockPrefabs[randomIndexes[i]], spawnPositions[i]);
            spawnedBlocks.Add(block);

            // Make sure the dragged block can get pointer events
            block.AddComponent<UIDragDrop>();
        }
    }

    private List<int> GetRandomPrefabIndexes()
    {
        if (uiBlockPrefabs.Count == 0) return new List<int>();
        
        int count = Mathf.Min(uiBlockPrefabs.Count, spawnPositions.Count);
        
        HashSet<int> randomIndexes = new HashSet<int>();
        while (randomIndexes.Count < count)
        {
            int randomIndex = Random.Range(0, uiBlockPrefabs.Count);
            randomIndexes.Add(randomIndex);
        }
        return randomIndexes.ToList();
    }

    public void RemoveBlock(GameObject block)
    {
        if (spawnedBlocks.Contains(block))
        {
            spawnedBlocks.Remove(block);
        }

        if (spawnedBlocks.Count == 0)
        {
            SpawnBlocks();
        }
    }
}
