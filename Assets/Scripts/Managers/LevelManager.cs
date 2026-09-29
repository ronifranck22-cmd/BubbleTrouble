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

    [Header("Life Pickup")]
    public GameObject lifePickupPrefab;
    [Range(0f, 1f)] public float lifePickupChance = 0.3f;

    [Header("Time Freeze Pickup")]
    public GameObject timeFreezePickupPrefab;
    [Range(0f, 1f)] public float timeFreezePickupChance = 0.15f;

    [Header("Shield Pickup")]
    public GameObject shieldPickupPrefab;
    [Range(0f, 1f)] public float shieldPickupChance = 0.15f;

    [Tooltip("Minimum horizontal distance between pickups that spawn on the same level.")]
    public float minDistanceBetweenPickups = 1.5f;

    [Header("Mid-level Pickups")]
    [Tooltip("Seconds between extra pickup rolls while a level is being played.")]
    public float midLevelSpawnInterval = 7.5f;
    [Tooltip("Mid-level chance = start-of-level chance x this (0.5 -> heart 0.15, freeze/shield 0.075).")]
    [Range(0f, 1f)] public float midLevelChanceMultiplier = 0.5f;
    [Tooltip("No extra pickup is rolled while this many uncollected ones are already on screen.")]
    public int maxPickupsOnScreen = 2;

    [Tooltip("Fraction of the camera's half-width/height kept clear of the very edges when picking a random spawn point.")]
    [Range(0.1f, 1f)]
    public float spawnAreaPadding = 0.6f;

    private readonly List<GameObject> activeBubbles = new List<GameObject>();

    private float freezeRemaining;
    private float midLevelSpawnTimer;

    public bool BubblesFrozen => freezeRemaining > 0f;
    public float FreezeRemaining => freezeRemaining;

    private void Awake()
    {
        Instance = this;
    }

    // Scaled time, so the freeze also pauses during the Game Over freeze (timeScale 0).
    private void Update()
    {
        UpdateMidLevelPickups();

        if (freezeRemaining <= 0f) return;

        freezeRemaining -= Time.deltaTime;
        if (freezeRemaining <= 0f)
            SetBubblesFrozen(false);
    }

    // Extra pickups during a level: only while Playing (stops on level change, Game Over,
    // Win), at a lower chance than the start-of-level roll, and never more than
    // maxPickupsOnScreen uncollected at once.
    private void UpdateMidLevelPickups()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Playing) return;

        midLevelSpawnTimer += Time.deltaTime;
        if (midLevelSpawnTimer < midLevelSpawnInterval) return;
        midLevelSpawnTimer = 0f;

        var takenX = new List<float>();
        foreach (GameObject existing in GameObject.FindGameObjectsWithTag("Pickup"))
            takenX.Add(existing.transform.position.x);

        TrySpawnMidLevelPickup(lifePickupPrefab, lifePickupChance, takenX);
        TrySpawnMidLevelPickup(timeFreezePickupPrefab, timeFreezePickupChance, takenX);
        TrySpawnMidLevelPickup(shieldPickupPrefab, shieldPickupChance, takenX);
    }

    private void TrySpawnMidLevelPickup(GameObject prefab, float startChance, List<float> takenX)
    {
        if (takenX.Count >= maxPickupsOnScreen) return;
        TrySpawnPickup(prefab, startChance * midLevelChanceMultiplier, takenX);
    }

    // Time Freeze pickup: every bubble stops; picking another one while frozen extends it.
    public void FreezeBubbles(float duration)
    {
        bool wasFrozen = BubblesFrozen;
        freezeRemaining = Mathf.Max(freezeRemaining, duration);
        if (!wasFrozen)
            SetBubblesFrozen(true);
    }

    private void SetBubblesFrozen(bool frozen)
    {
        if (!frozen) freezeRemaining = 0f;

        foreach (GameObject bubbleObj in activeBubbles)
        {
            if (bubbleObj != null)
                bubbleObj.GetComponent<Bubble>().SetFrozen(frozen);
        }
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
        freezeRemaining = 0f;     // a new level never starts frozen
        midLevelSpawnTimer = 0f;  // and its mid-level pickup clock starts over

        if (levels == null || index < 0 || index >= levels.Length)
        {
            Debug.LogWarning($"LevelManager: no LevelConfig at index {index}. Assign levels in the Inspector.");
            return;
        }

        LevelConfig level = levels[index];
        SetBackground(level.background);

        // A pickup nobody collected on the previous level must not carry over.
        foreach (GameObject old in GameObject.FindGameObjectsWithTag("Pickup"))
            Destroy(old);

        foreach (LevelConfig.BubbleSpawn spawn in level.startingBubbles)
        {
            for (int i = 0; i < spawn.count; i++)
                SpawnBubble(spawn.config);
        }

        // Independent roll per pickup type; ones on the same level keep apart.
        var takenX = new List<float>();
        TrySpawnPickup(lifePickupPrefab, lifePickupChance, takenX);
        TrySpawnPickup(timeFreezePickupPrefab, timeFreezePickupChance, takenX);
        TrySpawnPickup(shieldPickupPrefab, shieldPickupChance, takenX);
    }

    private void TrySpawnPickup(GameObject prefab, float chance, List<float> takenX)
    {
        if (prefab == null || Random.value >= chance) return;

        Vector3 position = GetPickupSpawnPosition(takenX);
        Instantiate(prefab, position, Quaternion.identity);
        takenX.Add(position.x);
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
        RegisterSpawnedBubble(bubbleObj);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Camera cam = Camera.main;
        float halfWidth = cam.orthographicSize * cam.aspect * spawnAreaPadding;
        float x = Random.Range(-halfWidth, halfWidth);
        float y = cam.orthographicSize * 0.5f;
        return new Vector3(x, y, 0f);
    }

    // Pickups don't move, so they sit at the player's height (random X like the
    // bubbles) instead of the bubbles' mid-air spawn height; always reachable by walking,
    // but not right on top of the player (a short walk to collect) and not on top of
    // another pickup from the same level.
    private Vector3 GetPickupSpawnPosition(List<float> takenX)
    {
        GameObject player = GameObject.FindWithTag("Player");
        float playerX = player != null ? player.transform.position.x : 0f;

        const float minDistanceFromPlayer = 2f;
        const int maxAttempts = 20;

        bool TooClose(float x)
        {
            if (Mathf.Abs(x - playerX) < minDistanceFromPlayer) return true;
            foreach (float other in takenX)
                if (Mathf.Abs(x - other) < minDistanceBetweenPickups) return true;
            return false;
        }

        Vector3 position = GetRandomSpawnPosition();
        for (int i = 0; i < maxAttempts && TooClose(position.x); i++)
            position = GetRandomSpawnPosition();

        if (player != null)
            position.y = player.transform.position.y;

        return position;
    }

    // Bubble calls this on itself (for the two children it spawns when popped) so the
    // count LevelManager tracks stays correct without LevelManager knowing about splitting.
    public void RegisterSpawnedBubble(GameObject bubbleObj)
    {
        activeBubbles.Add(bubbleObj);

        // Bubbles born during a Time Freeze (e.g. split children) start frozen too.
        if (BubblesFrozen)
            bubbleObj.GetComponent<Bubble>().SetFrozen(true);
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
