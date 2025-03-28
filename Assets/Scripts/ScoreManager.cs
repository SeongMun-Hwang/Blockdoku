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

    private int score = 0;
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
                }
            }
        }
        if (erasableCube.Count > 0)
        {
            score += erasableCube.Count;
            UICanvas.Instance.SetScore(score);
            // 지울 블록 비우기
            foreach (GameObject cube in erasableCube)
            {
                cube.GetComponent<Cube>().isFilled = false;
            }
        }
    }

}