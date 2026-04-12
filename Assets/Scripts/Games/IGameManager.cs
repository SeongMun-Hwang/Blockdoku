using System;

public interface IGameManager
{
    event Action<int> OnScoreChanged;
    event Action<int> OnBestScoreChanged;
    event Action<bool> OnGameOver;

    void StartGame();
    void EndGame();
    void AddScore(int amount);
}
