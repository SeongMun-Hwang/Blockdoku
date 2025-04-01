using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public ScoreManager scoreManager;
    [SerializeField] public BlockSpawner blockSpawner;
    [SerializeField] public MouseManager mouseManager;

    private static GameManager instance;
    public static GameManager Instance
    {
        get { return instance; }
    }
    private void OnEnable()
    {
        instance = this;
    }
}
