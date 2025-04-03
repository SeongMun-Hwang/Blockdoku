using System.Collections;
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
    }
    private void CheckAvailableSpace()
    {

    }
}
