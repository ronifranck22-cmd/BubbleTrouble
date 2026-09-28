using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Wiring (do this in the Editor, see the setup checklist):
// - Put this on a UIManager GameObject under the Canvas.
// - Drag the four screen root GameObjects (Start/HUD/GameOver/Win panels) into the
//   matching fields below, and the Text components into the HUD/result fields.
// - Hook the Start button's OnClick to UIManager.OnStartButtonPressed, and the
//   GameOver/Win screens' restart buttons to UIManager.OnRestartButtonPressed.
// Runs after Player so the Space press that starts the game doesn't also fire a shot that frame.
[DefaultExecutionOrder(100)]
public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject startScreen;
    public GameObject hudScreen;
    public GameObject gameOverScreen;
    public GameObject winScreen;
    public CanvasGroup startScreenCanvasGroup;

    [Header("HUD")]
    public Text scoreText;
    public Text livesText;
    public Text levelText;

    [Header("Result screens")]
    public Text gameOverScoreText;
    public Text gameOverHighScoreText;
    public Text winScoreText;
    public Text winHighScoreText;

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnLivesChanged += UpdateLives;
        GameManager.Instance.OnLevelChanged += UpdateLevel;
        GameManager.Instance.OnGameStarted += HandleGameStarted;
        GameManager.Instance.OnGameOver += ShowGameOver;
        GameManager.Instance.OnWin += ShowWin;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnScoreChanged -= UpdateScore;
        GameManager.Instance.OnLivesChanged -= UpdateLives;
        GameManager.Instance.OnLevelChanged -= UpdateLevel;
        GameManager.Instance.OnGameStarted -= HandleGameStarted;
        GameManager.Instance.OnGameOver -= ShowGameOver;
        GameManager.Instance.OnWin -= ShowWin;
    }

    private void Start()
    {
        SetActiveScreens(start: true);
    }

    // GDD 5: "press Space to start" on the Start screen (the button still works too).
    private void Update()
    {
        if (startScreen && startScreen.activeSelf && Input.GetKeyDown(KeyCode.Space))
            OnStartButtonPressed();
    }

    // Hook to the Start screen's button OnClick.
    public void OnStartButtonPressed()
    {
        // The Start screen stays up during its fade-out; ignore a second press/click then.
        if (GameManager.Instance.State != GameManager.GameState.Start) return;

        GameManager.Instance.StartGame();
    }

    // Hook to the GameOver/Win screens' restart button OnClick.
    public void OnRestartButtonPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void HandleGameStarted() => StartCoroutine(FadeOutStartScreen());

    private IEnumerator FadeOutStartScreen()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        if (startScreenCanvasGroup != null)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                startScreenCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }
            startScreenCanvasGroup.alpha = 1f; // reset for a future scene reload
        }
        ShowHud();
    }

    private void ShowHud() => SetActiveScreens(hud: true);

    private void ShowGameOver()
    {
        SetActiveScreens(gameOver: true);

        if (gameOverScoreText) gameOverScoreText.text = $"Score: {GameManager.Instance.Score}";
        if (gameOverHighScoreText) gameOverHighScoreText.text = HighScoreLine();
    }

    private void ShowWin()
    {
        SetActiveScreens(win: true);

        if (winScoreText) winScoreText.text = $"Score: {GameManager.Instance.Score}";
        if (winHighScoreText) winHighScoreText.text = HighScoreLine();
    }

    private string HighScoreLine()
    {
        int highScore = GameManager.Instance.GetHighScore();
        return GameManager.Instance.IsNewHighScore ? $"NEW High Score: {highScore}!" : $"High Score: {highScore}";
    }

    private void SetActiveScreens(bool start = false, bool hud = false, bool gameOver = false, bool win = false)
    {
        if (startScreen) startScreen.SetActive(start);
        if (hudScreen) hudScreen.SetActive(hud);
        if (gameOverScreen) gameOverScreen.SetActive(gameOver);
        if (winScreen) winScreen.SetActive(win);
    }

    private void UpdateScore(int score)
    {
        if (scoreText) scoreText.text = $"Score: {score}";
    }

    private void UpdateLives(int lives)
    {
        if (livesText) livesText.text = $"Lives: {lives}";
    }

    private void UpdateLevel(int levelIndex)
    {
        if (levelText) levelText.text = $"Level: {levelIndex + 1}";
    }
}
