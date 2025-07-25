using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
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
    [SerializeField] TextMeshProUGUI bestScoreTmp;
    private int bestScore;
    private int score = 0;
    private int combo = 0;
    private int itemScore = 0;
    private void OnEnable()
    {
        mouseManager.onMouseReleased += CheckBoard;
    }
    private void OnDisable()
    {
        mouseManager.onMouseReleased -= CheckBoard;
    }
    private void Start()
    {
        bestScoreTmp.text = LoadBestScore();
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
            AddScore(erasableCube.Count);

            if (combo > 1)
            {
                UICanvas.Instance.ShowCombo(combo + " Combo!\n"+"+"+ erasableCube.Count * combo);
            }

            UICanvas.Instance.SetScore(score);
            // 지울 블록 비우기
            foreach (GameObject cube in erasableCube)
            {
                cube.GetComponent<Cube>().isFilled = false;
            }
            if (itemScore >= 1)
            {
                itemScore = 0;
                GameManager.Instance.itemManager.SpawnItem();
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
        string path = SavePaths.BoardDataPath;
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
    public void SaveBestScore()
    {
        PersonalData personalData;

        if (File.Exists(SavePaths.PersonalDataPath))
        {
            string json = File.ReadAllText(SavePaths.PersonalDataPath);
            personalData = JsonUtility.FromJson<PersonalData>(json);
        }
        else
        {
            personalData = new PersonalData();
        }

        if (score > personalData.bestScore)
        {
            personalData.bestScore = score;
            string newJson = JsonUtility.ToJson(personalData);
            File.WriteAllText(SavePaths.PersonalDataPath, newJson);
        }
    }

    public string LoadBestScore()
    {
        if (File.Exists(SavePaths.PersonalDataPath))
        {
            string json = File.ReadAllText(SavePaths.PersonalDataPath);
            PersonalData personalData = JsonUtility.FromJson<PersonalData>(json);
            bestScore = personalData.bestScore;
            return personalData.bestScore.ToString();
        }
        return "";
    }
    public int GetCombo()
    {
        return combo;
    }
    public void AddScore(int amount)
    {
        score += amount * combo;
        itemScore += amount * combo;
    }
}