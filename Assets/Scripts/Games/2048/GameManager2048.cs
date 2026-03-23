using UnityEngine;
using System;

namespace Games._2048
{
    public class GameManager2048 : MonoBehaviour
    {
        public static GameManager2048 Instance { get; private set; }

        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public bool IsGameOver { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action OnGameOver;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void OnEnable()
        {
            InputManager2048.OnMove += HandleMove;
        }

        void OnDisable()
        {
            InputManager2048.OnMove -= HandleMove;
        }

        void Start()
        {
            LoadHighScore();
            StartNewGame();
        }

        public void StartNewGame()
        {
            Score = 0;
            IsGameOver = false;
            OnScoreChanged?.Invoke(Score);
            GridManager2048.Instance.InitializeGrid();
        }

        private void HandleMove(MoveDirection direction)
        {
            if (IsGameOver) return;

            if (GridManager2048.Instance.Move(direction))
            {
                GridManager2048.Instance.SpawnTile();
                
                if (!GridManager2048.Instance.CanMove())
                {
                    GameOver();
                }
            }
        }

        public void AddScore(int points)
        {
            Score += points;
            OnScoreChanged?.Invoke(Score);
            
            if (Score > HighScore)
            {
                HighScore = Score;
                SaveHighScore();
            }
        }

        private void GameOver()
        {
            IsGameOver = true;
            OnGameOver?.Invoke();
            Debug.Log("Game Over!");
        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("2048_HighScore", HighScore);
            PlayerPrefs.Save();
        }

        private void LoadHighScore()
        {
            HighScore = PlayerPrefs.GetInt("2048_HighScore", 0);
        }
    }
}
