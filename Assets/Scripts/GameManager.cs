using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public ScoreManager scoreManager;
    [SerializeField] public BlockSpawner blockSpawner;
    [SerializeField] public MouseManager mouseManager;
    private bool[,] filledCubeArray = new bool[9, 9];

    private static GameManager instance;
    public static GameManager Instance
    {
        get { return instance; }
    }

    private void OnEnable()
    {
        instance = this;
    }
    public void UpdateFilledCubeArray(bool[,] array)
    {
        filledCubeArray = array;
        CheckAvailableSpace();
    }
    private void CheckAvailableSpace()
    {
        List<GameObject> spawnedBlocks = blockSpawner.ReturnSpawnedBlocks();
        bool canPlace = false;
        foreach (GameObject block in spawnedBlocks)
        {
            int[,] shapeArray = block.GetComponent<Block>().shape;
            int shapeRows = shapeArray.GetLength(0);
            int shapeCols = shapeArray.GetLength(1);

            for (int i = 0; i <= 9 - shapeRows; i++)
            {
                for (int j = 0; j <= 9 - shapeCols; j++)
                {
                    if (CanPlaceBlock(i, j, shapeArray))
                    {
                        canPlace = true;
                        break;
                    }
                }
                if (canPlace) break;
            }
            if (canPlace) break;
        }
        if (!canPlace)
        {
            UICanvas.Instance.ShowGameOverPanel();
        }
    }
    private bool CanPlaceBlock(int startX, int startY, int[,] shapeArray)
    {
        int shapeRows = shapeArray.GetLength(0);
        int shapeCols = shapeArray.GetLength(1);

        for (int i = 0; i < shapeRows; i++)
        {
            for (int j = 0; j < shapeCols; j++)
            {
                if (shapeArray[i, j] == 1 && filledCubeArray[startX + i, startY + j])
                {
                    return false;
                }
            }
        }
        return true;
    }
}
