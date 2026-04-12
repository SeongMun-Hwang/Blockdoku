using UnityEngine;
using System;

namespace Games._2048
{
    public class GameManager2048 : MonoBehaviour, IGameManager
    {
        public static GameManager2048 Instance { get; private set; }

        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public bool IsGameOver { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action<int> OnBestScoreChanged;
        public event Action<bool> OnGameOver;

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
            StartGame();
        }

        public void StartGame()
        {
            bool hasSave = System.IO.File.Exists(SavePaths._2048DataPath);
            if (hasSave)
            {
                StartLoadedGame();
            }
            else
            {
                StartNewGame();
            }
        }

        public void StartNewGame()
        {
            Score = 0;
            IsGameOver = false;
            OnScoreChanged?.Invoke(Score);
            OnBestScoreChanged?.Invoke(HighScore);
            OnGameOver?.Invoke(false);
            GridManager2048.Instance.InitializeGrid(false);
        }

        public void StartLoadedGame()
        {
            IsGameOver = false;
            OnGameOver?.Invoke(false);
            OnBestScoreChanged?.Invoke(HighScore);
            // GridManager2048.InitializeGrid(true) will call SetScore via LoadBoard
            GridManager2048.Instance.InitializeGrid(true);
        }

        public void SetScore(int score)
        {
            Score = score;
            OnScoreChanged?.Invoke(Score);
        }

        private void HandleMove(MoveDirection direction)
        {
            if (IsGameOver) return;

            if (GridManager2048.Instance.Move(direction))
            {
                GridManager2048.Instance.SpawnTile();
                GridManager2048.Instance.SaveBoard();
                
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
                OnBestScoreChanged?.Invoke(HighScore);
                SaveHighScore();
            }
        }

        public void EndGame()
        {
            GameOver();
        }

        private void GameOver()
        {
            IsGameOver = true;
            GridManager2048.Instance.ClearSave();
            
            OnGameOver?.Invoke(true);
            Debug.Log("Game Over Panel Activated");

            AdEventBus.TriggerGamePlayEnded(MinigameType._2048, () => {
                Debug.Log("Game Over Ad sequence finished");
            });
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
