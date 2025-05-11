using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Serializable]
    public class Row
    {
        public GameObject[] col;
    }
    public Row[] grid;

    [SerializeField] MouseManager mouseManager;

    private int score = 0;
    private int combo = 0;
    private void OnEnable()
    {
        mouseManager.onMouseReleased += CheckBoard;
    }
    private void OnDisable()
    {
        mouseManager.onMouseReleased -= CheckBoard;
    }
    void CheckBoard()
    {
        HashSet<GameObject> erasableCube = new HashSet<GameObject>();

        // 가로줄 검사
        for (int i = 0; i < grid.Length; i++)
        {
            HashSet<GameObject> tempCube = new HashSet<GameObject>(); // 여기에 선언 (각 루프마다 새로 생성)
            bool isRowFilled = true;
            for (int j = 0; j < grid[i].col.Length; j++)
            {
                Cube cube = grid[i].col[j].GetComponent<Cube>();
                if (cube == null || !cube.isFilled)
                {
                    isRowFilled = false;
                    break;
                }
                tempCube.Add(cube.gameObject);
            }
            if (isRowFilled)
            {
                erasableCube.UnionWith(tempCube);
                combo++;
            }
        }

        // 세로줄 검사
        for (int j = 0; j < grid[0].col.Length; j++)
        {
            HashSet<GameObject> tempCube = new HashSet<GameObject>(); // 새로운 tempCube 사용
            bool isColFilled = true;
            for (int i = 0; i < grid.Length; i++)
            {
                Cube cube = grid[i].col[j].GetComponent<Cube>();
                if (cube == null || !cube.isFilled)
                {
                    isColFilled = false;
                    break;
                }
                tempCube.Add(cube.gameObject);
            }
            if (isColFilled)
            {
                erasableCube.UnionWith(tempCube);
                combo++;
            }
        }

        // 3x3 검사
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                HashSet<GameObject> tempCube = new HashSet<GameObject>(); // 3x3 검사에서도 새로 선언
                bool isBlockFilled = true;
                for (int x = i * 3; x < (i + 1) * 3; x++)
                {
                    for (int y = j * 3; y < (j + 1) * 3; y++)
                    {
                        Cube cube = grid[x].col[y].GetComponent<Cube>();
                        if (cube == null || !cube.isFilled)
                        {
                            isBlockFilled = false;
                            break;
                        }
                        tempCube.Add(cube.gameObject);
                    }
                    if (!isBlockFilled) break;
                }

                if (isBlockFilled)
                {
                    erasableCube.UnionWith(tempCube);
                    combo++;
                }
            }
        }
        if (erasableCube.Count > 0)
        {
            if (combo > 1)
            {
                UICanvas.Instance.ShowCombo(combo + " Combo!");
            }
            score += erasableCube.Count * combo;

            UICanvas.Instance.SetScore(score);
            // 지울 블록 비우기
            foreach (GameObject cube in erasableCube)
            {
                cube.GetComponent<Cube>().isFilled = false;
            }
        }
        else
        {
            combo = 0;
        }
        UpdateFilledBoard();
    }
    public void UpdateFilledBoard()
    {
        bool[,] tempArray = new bool[9, 9];
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                tempArray[i, j] = grid[i].col[j].GetComponent<Cube>().isFilled;
            }
        }
        GameManager.Instance.UpdateFilledCubeArray(tempArray);
    }
    public int ReturnScore()
    {
        return score;
    }
    public void SaveBoardData()
    {
        SaveData saveData = new SaveData();
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                saveData.cubeFilledStates[i * 9 + j] = grid[i].col[j].GetComponent<Cube>().isFilled;
            }
        }
        saveData.score = score;
        saveData.combo = combo;

        string json = JsonUtility.ToJson(saveData);
        string path = Application.persistentDataPath + "/save.json";
        File.WriteAllText(path, json);
        Debug.Log("Data saved to " + path);
    }
    public void LoadBoardData()
    {       
        if (File.Exists(SavePaths.BoardDataPath))
        {
            string json = File.ReadAllText(SavePaths.BoardDataPath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    grid[i].col[j].GetComponent<Cube>().isFilled = saveData.cubeFilledStates[i * 9 + j];
                }
            }
            score = saveData.score;
            combo = saveData.combo;
            UICanvas.Instance.SetScore(score);
        }
        else
        {
            Debug.Log("Save file not found at " + SavePaths.BoardDataPath);
        }
    }
}