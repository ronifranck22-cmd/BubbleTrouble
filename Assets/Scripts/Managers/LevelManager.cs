using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Tooltip("One entry per level, in play order.")]
    public LevelConfig[] levels;

    [Tooltip("The single shared Bubble prefab (GDD 7: one prefab, not three).")]
    public GameObject bubblePrefab;

    [Tooltip("Scene-level SpriteRenderer that shows the current level's background.")]
    public SpriteRenderer backgroundRenderer;

    [Tooltip("Fraction of the camera's half-width/height kept clear of the very edges when picking a random spawn point.")]
    [Range(0.1f, 1f)]
    public float spawnAreaPadding = 0.6f;

    private readonly List<GameObject> activeBubbles = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnGameStarted += HandleGameStarted;
        GameManager.Instance.OnLevelClear += HandleLevelClear;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnGameStarted -= HandleGameStarted;
        GameManager.Instance.OnLevelClear -= HandleLevelClear;
    }

    private void HandleGameStarted()
    {
        LoadLevel(GameManager.Instance.CurrentLevelIndex);
    }

    private void HandleLevelClear()
    {
        LoadLevel(GameManager.Instance.CurrentLevelIndex);
        GameManager.Instance.ResumeAfterLevelClear();
    }

    private void LoadLevel(int index)
    {
        ClearActiveBubbles();

        if (levels == null || index < 0 || index >= levels.Length)
        {
            Debug.LogWarning($"LevelManager: no LevelConfig at index {index}. Assign levels in the Inspector.");
            return;
        }

        LevelConfig level = levels[index];
        SetBackground(level.background);

        foreach (LevelConfig.BubbleSpawn spawn in level.startingBubbles)
        {
            for (int i = 0; i < spawn.count; i++)
                SpawnBubble(spawn.config);
        }
    }

    private void SetBackground(Sprite sprite)
    {
        if (sprite == null || backgroundRenderer == null) return;

        backgroundRenderer.sprite = sprite;

        // Scale to cover the camera's whole view (plus a small margin) in both
        // directions, whatever the image's resolution/aspect or the screen's
        // aspect; images wider/taller than the view get cropped at the edges.
        Camera cam = Camera.main;
        float targetHeight = cam.orthographicSize * 2f + 2f;
        float targetWidth = targetHeight * cam.aspect + 2f;
        Vector2 native = sprite.bounds.size;
        if (native.x > 0f && native.y > 0f)
        {
            float scale = Mathf.Max(targetWidth / native.x, targetHeight / native.y);
            backgroundRenderer.transform.localScale = new Vector3(scale, scale, 1f);
        }

        // Centre on the camera (which sits at y = 1, not the origin).
        Vector3 camPos = cam.transform.position;
        backgroundRenderer.transform.position = new Vector3(camPos.x, camPos.y, backgroundRenderer.transform.position.z);
    }

    private void SpawnBubble(BubbleConfig config)
    {
        if (bubblePrefab == null)
        {
            Debug.LogError("LevelManager: bubblePrefab is not assigned.");
            return;
        }

        GameObject bubbleObj = Instantiate(bubblePrefab, GetRandomSpawnPosition(), Quaternion.identity);
        bubbleObj.GetComponent<Bubble>().Initialize(config);
        activeBubbles.Add(bubbleObj);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Camera cam = Camera.main;
        float halfWidth = cam.orthographicSize * cam.aspect * spawnAreaPadding;
        float x = Random.Range(-halfWidth, halfWidth);
        float y = cam.orthographicSize * 0.5f;
        return new Vector3(x, y, 0f);
    }

    // Bubble calls this on itself (for the two children it spawns when popped) so the
    // count LevelManager tracks stays correct without LevelManager knowing about splitting.
    public void RegisterSpawnedBubble(GameObject bubbleObj)
    {
        activeBubbles.Add(bubbleObj);
    }

    public void NotifyBubbleRemoved(GameObject bubbleObj)
    {
        activeBubbles.Remove(bubbleObj);

        if (activeBubbles.Count == 0)
        {
            bool isLastLevel = GameManager.Instance.CurrentLevelIndex >= levels.Length - 1;
            GameManager.Instance.NotifyLevelCleared(isLastLevel);
        }
    }

    private void ClearActiveBubbles()
    {
        foreach (GameObject bubble in activeBubbles)
        {
            if (bubble != null)
                Destroy(bubble);
        }

        activeBubbles.Clear();
    }
}
