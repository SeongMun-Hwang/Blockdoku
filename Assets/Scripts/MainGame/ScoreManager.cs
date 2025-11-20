using System;
using System.Collections;
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
        mouseManager.onMouseReleased += StartCheckBoardRoutine;
    }
    private void OnDisable()
    {
        mouseManager.onMouseReleased -= StartCheckBoardRoutine;
    }
    private void Start()
    {
        if (bestScoreTmp != null)
        {
            bestScoreTmp.text = LoadBestScore();
        }
    }
    void StartCheckBoardRoutine()
    {
        StartCoroutine(CheckBoardRoutine());
    }
    IEnumerator CheckBoardRoutine()
    {
        bool[,] currentFilledState = new bool[9, 9];
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                currentFilledState[i, j] = grid[i].col[j].GetComponent<Cube>().isFilled;
            }
        }

        HashSet<GameObject> erasableCube = GetErasableCubes(currentFilledState, true);

        if (erasableCube.Count > 0)
        {
            foreach (GameObject cube in erasableCube)
            {
                cube.GetComponent<Cube>().StopBlinking();
                cube.GetComponent<Cube>().SetToFillMaterial();
            }
            //yield return new WaitForSeconds(0.2f);

            AddScore(erasableCube.Count);

            if (combo > 1)
            {
                UICanvas.Instance.ShowCombo(combo + " Combo!\n" + "+" + erasableCube.Count * combo);
            }

            UICanvas.Instance.SetScore(score);
            // 지울 블록 비우기
            foreach (GameObject cube in erasableCube)
            {
                cube.GetComponent<Cube>().isFilled = false;
            }

            yield return new WaitForSeconds(0.5f);

            if (GameManager.Instance.itemManager != null)
            {
                if (itemScore >= 1)
                {
                    itemScore = 0;
                    GameManager.Instance.itemManager.SpawnItem();
                }
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

    public HashSet<GameObject> CheckBoardForPreview(HashSet<GameObject> previewCubes)
    {
        bool[,] previewFilled = new bool[9, 9];
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                previewFilled[i, j] = grid[i].col[j].GetComponent<Cube>().isFilled;
            }
        }

        foreach (GameObject cube in previewCubes)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i].col[j] == cube)
                    {
                        previewFilled[i, j] = true;
                    }
                }
            }
        }

        return GetErasableCubes(previewFilled, false);
    }

    private HashSet<GameObject> GetErasableCubes(bool[,] currentFilledState, bool countCombo)
    {
        HashSet<GameObject> erasableCube = new HashSet<GameObject>();

        // 가로줄 검사
        for (int i = 0; i < 9; i++)
        {
            bool isRowFilled = true;
            HashSet<GameObject> tempCube = new HashSet<GameObject>();
            for (int j = 0; j < 9; j++)
            {
                if (!currentFilledState[i, j])
                {
                    isRowFilled = false;
                    break;
                }
                tempCube.Add(grid[i].col[j]);
            }
            if (isRowFilled)
            {
                erasableCube.UnionWith(tempCube);
                if (countCombo) combo++;
            }
        }

        // 세로줄 검사
        for (int j = 0; j < 9; j++)
        {
            bool isColFilled = true;
            HashSet<GameObject> tempCube = new HashSet<GameObject>();
            for (int i = 0; i < 9; i++)
            {
                if (!currentFilledState[i, j])
                {
                    isColFilled = false;
                    break;
                }
                tempCube.Add(grid[i].col[j]);
            }
            if (isColFilled)
            {
                erasableCube.UnionWith(tempCube);
                if (countCombo) combo++;
            }
        }

        // 3x3 검사
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                bool isBlockFilled = true;
                HashSet<GameObject> tempCube = new HashSet<GameObject>();
                for (int x = i * 3; x < (i + 1) * 3; x++)
                {
                    for (int y = j * 3; y < (j + 1) * 3; y++)
                    {
                        if (!currentFilledState[x, y])
                        {
                            isBlockFilled = false;
                            break;
                        }
                        tempCube.Add(grid[x].col[y]);
                    }
                    if (!isBlockFilled) break;
                }

                if (isBlockFilled)
                {
                    erasableCube.UnionWith(tempCube);
                    if (countCombo) combo++;
                }
            }
        }
        return erasableCube;
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