using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> blockPrefabs;
    [SerializeField] private List<Transform> spawnPos;
    private List<GameObject> spawnedBlocks = new List<GameObject>();

    private void Start()
    {
        if(System.IO.File.Exists(SavePaths.BlockDataPath))
        {
            LoadBlockData();
        }
        else
        {
            SpawnBlocks();
        }
    }
    private void SpawnBlocks()
    {
        for (int i = 0; i < spawnPos.Count; i++)
        {
            GameObject go = Instantiate(blockPrefabs[Random.Range(0, blockPrefabs.Count)], spawnPos[i]);
            int randomRot = Random.Range(0, 4);
            go.transform.rotation = Quaternion.Euler(0, randomRot * 90, 0);
            go.GetComponent<Block>().RotateShape(randomRot);
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
    //블록 데이터 저장
    public void SaveBlockData()
    {
        BlockSaveDatas blockSaveDatas = new BlockSaveDatas();
        blockSaveDatas.blocks = new List<BlockSaveData>();

        foreach (GameObject go in spawnedBlocks)
        {
            Block block = go.GetComponent<Block>();
            int spawnIndex = spawnPos.IndexOf(go.transform.parent);
            string prefabName = go.name.Replace("(Clone)", "").Trim();
            int rotStep = Mathf.RoundToInt(go.transform.rotation.eulerAngles.y / 90) % 4;

            blockSaveDatas.blocks.Add(new BlockSaveData
            {
                prefabName = prefabName,
                spawnIndex = spawnIndex,
                rotationStep = rotStep
            });
        }
        string json = JsonUtility.ToJson(blockSaveDatas);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/blockData.json", json);
    }
    public void LoadBlockData()
    {
        string json = System.IO.File.ReadAllText(SavePaths.BlockDataPath);
        BlockSaveDatas blockSaveDatas = JsonUtility.FromJson<BlockSaveDatas>(json);

        foreach (BlockSaveData data in blockSaveDatas.blocks)
        {
            GameObject prefab = blockPrefabs.Find(x => x.name == data.prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab {data.prefabName} not found.");
                continue;
            }
            Transform transform = spawnPos[data.spawnIndex];
            GameObject go = Instantiate(prefab, transform);
            go.transform.rotation = Quaternion.Euler(0, data.rotationStep * 90, 0);
            go.GetComponent<Block>().RotateShape(data.rotationStep);
            spawnedBlocks.Add(go);
        }
    }
}