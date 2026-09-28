using System;
using UnityEngine;

// Runs before other scripts so Instance exists when LevelManager/UIManager subscribe in OnEnable.
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Start, Playing, LevelClear, GameOver, Win }

    public int startingLives = 3;

    public int Score { get; private set; }
    public int Lives { get; private set; }
    public int CurrentLevelIndex { get; private set; }
    public GameState State { get; private set; } = GameState.Start;
    public bool IsNewHighScore { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<int> OnLivesChanged;
    public event Action<int> OnLevelChanged;
    public event Action OnGameStarted;
    public event Action OnLevelClear;
    public event Action OnGameOver;
    public event Action OnWin;

    private const string HighScoreKey = "BubbleTrouble_HighScore";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int GetHighScore() => PlayerPrefs.GetInt(HighScoreKey, 0);

    public void StartGame()
    {
        Score = 0;
        IsNewHighScore = false;
        Lives = startingLives;
        CurrentLevelIndex = 0;
        State = GameState.Playing;

        OnScoreChanged?.Invoke(Score);
        OnLivesChanged?.Invoke(Lives);
        OnLevelChanged?.Invoke(CurrentLevelIndex);
        OnGameStarted?.Invoke();
    }

    public void AddScore(int amount)
    {
        if (State != GameState.Playing) return;

        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    public void LoseLife()
    {
        if (State != GameState.Playing) return;

        Lives--;
        OnLivesChanged?.Invoke(Lives);

        if (Lives <= 0)
            TriggerGameOver();
    }

    public void GainLife()
    {
        if (State != GameState.Playing) return;

        Lives++;
        OnLivesChanged?.Invoke(Lives);
    }

    // isLastLevel is decided by LevelManager, which owns the level list.
    public void NotifyLevelCleared(bool isLastLevel)
    {
        if (State != GameState.Playing) return;

        if (isLastLevel)
        {
            TriggerWin();
            return;
        }

        State = GameState.LevelClear;
        CurrentLevelIndex++;
        OnLevelChanged?.Invoke(CurrentLevelIndex);
        OnLevelClear?.Invoke();
    }

    // Called by LevelManager once the next level has finished spawning.
    public void ResumeAfterLevelClear()
    {
        State = GameState.Playing;
    }

    private void TriggerGameOver()
    {
        State = GameState.GameOver;
        SaveHighScoreIfNeeded();
        OnGameOver?.Invoke();
    }

    private void TriggerWin()
    {
        State = GameState.Win;
        SaveHighScoreIfNeeded();
        OnWin?.Invoke();
    }

    private void SaveHighScoreIfNeeded()
    {
        if (Score <= GetHighScore()) return;

        IsNewHighScore = true;
        PlayerPrefs.SetInt(HighScoreKey, Score);
        PlayerPrefs.Save();
    }
}
