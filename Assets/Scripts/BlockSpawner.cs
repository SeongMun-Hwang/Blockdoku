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
        for (int i = 0; i < spawnPos.Count; i++)
        {
            GameObject go = Instantiate(blockPrefabs[Random.Range(0, blockPrefabs.Count)], spawnPos[i]);
            int randomRot = Random.Range(0, 4);
            go.transform.rotation = Quaternion.Euler(0, randomRot * 90, 0);

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
    public List<GameObject> ReturnSpawnedBlocks()
    {
        return spawnedBlocks;
    }
}