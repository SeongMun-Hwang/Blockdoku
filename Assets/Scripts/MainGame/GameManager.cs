using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public ScoreManager scoreManager;
    [SerializeField] public BlockSpawner blockSpawner;
    [SerializeField] public MouseManager mouseManager;
    [SerializeField] public AudioManager audioManager;
    [SerializeField] public ItemManager itemManager;
    [SerializeField] GameObject backgroundImg;
    private bool[,] filledCubeArray = new bool[9, 9];

    private static GameManager instance;
    public static GameManager Instance
    {
        get { return instance; }
    }

    private void OnEnable()
    {
        float targetWidthInWorldUnits = 0.5625f; // 기준이 되는 월드 너비
        float screenAspect = (float)Screen.width / Screen.height;
        Camera.main.orthographicSize = targetWidthInWorldUnits / screenAspect * 12f;
        Vector3 currrentImgScale = backgroundImg.transform.localScale;
        backgroundImg.transform.localScale = new Vector3(currrentImgScale.x * targetWidthInWorldUnits / screenAspect,
            currrentImgScale.y * targetWidthInWorldUnits / screenAspect, currrentImgScale.z * targetWidthInWorldUnits / screenAspect);
        instance = this;
    }
    private void Start()
    { 
        scoreManager.LoadBoardData();
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
    public void SaveGameData()
    {
        //blockSpawner.SaveBlockData();
        scoreManager.SaveBoardData();
        blockSpawner.SaveBlockData();
        audioManager.SaveAudioData();
    }
    public void RemoveGameData()
    {
        if (File.Exists(SavePaths.BoardDataPath))
        {
            File.Delete(SavePaths.BoardDataPath);
            Debug.Log("Board Save file deleted");
        }        
        if (File.Exists(SavePaths.BlockDataPath))
        {
            File.Delete(SavePaths.BlockDataPath);
            Debug.Log("Block Save file deleted");
        }
    }
}
