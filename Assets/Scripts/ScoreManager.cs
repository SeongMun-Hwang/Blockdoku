using System;
using System.Collections.Generic;
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
        HashSet<GameObject> tempCube = new HashSet<GameObject>();
        //가로줄 검사
        for (int i = 0; i < grid.Length; i++)
        {
            bool isRowFilled = true;
            for (int j = 0; j < grid[i].col.Length; j++)
            {
                Cube cube = grid[i].col[j].GetComponent<Cube>();
                tempCube.Add(cube.gameObject);
                if (cube != null && !cube.isFilled)
                {
                    isRowFilled = false;
                    break;
                }
            }
            if (isRowFilled)
            {
                erasableCube.UnionWith(tempCube);
            }
        }
        tempCube.Clear();
        //세로줄 검사
        for (int j = 0; j < grid[0].col.Length; j++)
        {
            bool isColFilled = true;
            for (int i = 0; i < grid.Length; i++)
            {
                Cube cube = grid[i].col[j].GetComponent<Cube>();
                tempCube.Add(cube.gameObject);
                if (cube != null && !cube.isFilled)
                {
                    isColFilled = false;
                    break;
                }
            }
            if (isColFilled)
            {
                erasableCube.UnionWith(tempCube);
            }
        }
        tempCube.Clear();
        //3*3 검사
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                bool isBlockFilled = true;
                for (int x = i * 3; x < (i + 1) * 3; x++)
                {
                    for (int y = j * 3; y < (j + 1) * 3; y++)
                    {
                        Cube cube = grid[x].col[y].GetComponent<Cube>();
                        tempCube.Add(cube.gameObject);
                        if (cube != null && !cube.isFilled)
                        {
                            isBlockFilled = false;
                            break; // 블록 검사 중단
                        }
                    }
                    if (!isBlockFilled) break;
                }

                if (isBlockFilled)
                {
                    erasableCube.UnionWith(tempCube);
                }
            }
        }
        foreach(GameObject cube in erasableCube)
        {
            cube.GetComponent<Cube>().isFilled = false;
        }
    }
}