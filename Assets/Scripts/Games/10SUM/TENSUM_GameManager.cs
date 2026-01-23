
using UnityEngine;
using UnityEngine.SceneManagement;

public class TENSUM_GameManager : MonoBehaviour
{
    public static TENSUM_GameManager Instance { get; private set; }

    public UIManager uiManager;
    public GridManager gridManager;

    private int score = 0;
    private float timer;
    private bool isGameOver = false;

    private const float GAME_DURATION = 120f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (isGameOver) return;

        timer -= Time.deltaTime;
        uiManager.UpdateTimer(timer);

        if (timer <= 0)
        {
            uiManager.UpdateTimer(0);
            EndGame();
        }
    }

    public void StartGame()
    {
        score = 0;
        timer = GAME_DURATION;
        isGameOver = false;
        uiManager.UpdateScore(score);
        uiManager.ShowGameOverPanel(false);
        gridManager.InitializeGrid();
    }

    public void AddScore(int amount)
    {
        score += amount;
        uiManager.UpdateScore(score);
    }

    private void EndGame()
    {
        isGameOver = true;
        uiManager.ShowGameOverPanel(true, score);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene("Title");
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}
