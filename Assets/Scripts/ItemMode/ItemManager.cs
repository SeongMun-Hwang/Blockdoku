using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] List<GameObject> itemPrefabs;
    private List<GameObject> blocks = new List<GameObject>();
    void Start()
    {
        blocks=GameManager.Instance.scoreManager.grid.SelectMany(row=>row.col).ToList();
    }

    public void SpawnItem()
    {
        int randomPos = Random.Range(0, blocks.Count);
        int randomItem = Random.Range(0, itemPrefabs.Count);
        
        GameObject targetCube = blocks[randomPos];
        targetCube.GetComponent<Cube>().SetItemMarkActive();
    }
    public void GetItem()
    {
        Debug.Log("Get Item");
    }
}
