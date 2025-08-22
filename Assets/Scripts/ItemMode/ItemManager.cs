using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] List<GameObject> itemPrefabs;
    private List<Cube> blocks = new List<Cube>();
    void Start()
    {
        ScoreManager scoreManager = GameManager.Instance.scoreManager;
        foreach (var row in scoreManager.grid)
        {
            foreach (var col in row.col)
            {
                blocks.Add(col.GetComponent<Cube>());
            }
        }
    }

    public void SpawnItem()
    {
        int randomPos = Random.Range(0, blocks.Count);
        int randomItem = Random.Range(0, itemPrefabs.Count);

        Cube targetCube = blocks[randomPos];
        targetCube.SetItemMarkActive();
    }
    public void GetItem()
    {
        Debug.Log("Get Item");
    }
}
