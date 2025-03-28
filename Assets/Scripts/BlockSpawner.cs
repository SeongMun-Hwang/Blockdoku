using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> blockPrefabs;
    [SerializeField] private List<Transform> spawnPos;
    private List<GameObject> spawnedBlocks = new List<GameObject>();

    private void Start()
    {
        SpawnBlocks();
    }
    private void SpawnBlocks()
    {
        for(int i=0;i<spawnPos.Count;i++)
        {
            GameObject go = Instantiate(blockPrefabs[Random.Range(0, blockPrefabs.Count)], spawnPos[i]);
            go.GetComponent<Block>().blockSpawner = this;
            spawnedBlocks.Add(go);
        }
    }
    public void RemoveBlock(GameObject go)
    {
        spawnedBlocks.Remove(go);
        if (spawnedBlocks.Count == 0)
        {
            SpawnBlocks();
        }
    }
}