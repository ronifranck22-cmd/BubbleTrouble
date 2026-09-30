using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float shootOffsetY = 0.5f;

    [Header("Bubble contact")]
    public float invulnDuration = 1.0f;
    public float flickerInterval = 0.1f;

    [Header("Skin")]
    public PlayerSkin defaultSkin;   // Skin_Classic; used when nothing was picked on the Start screen
    private PlayerSkin activeSkin;

    private SpriteRenderer spriteRenderer;
    private GameObject activeProjectile;
    private bool isInvulnerable;
    private bool isTransitioning;

    [Header("Shield")]
    [Tooltip("Steady tint while the Shield pickup is active (distinct from the hit flicker).")]
    public Color shieldTint = new Color(0.6f, 0.85f, 1f, 1f);
    private float shieldRemaining;

    public bool IsShielded => shieldRemaining > 0f;
    public float ShieldRemaining => shieldRemaining;

    private Sprite cachedSprite;
    private float cachedMinX;
    private float cachedMaxX;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Hidden on the Start screen (the big FRONT character there is a UI Image);
        // already in its normal sprite/size/position, it fades in when the game starts.
        Color c = spriteRenderer.color;
        c.a = 0f;
        spriteRenderer.color = c;
        spriteRenderer.sprite = defaultSkin != null ? defaultSkin.back : null;
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnGameStarted += HandleGameStarted;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnGameStarted -= HandleGameStarted;
    }

    private void HandleGameStarted()
    {
        activeSkin = CharacterSelectUI.SelectedSkin != null ? CharacterSelectUI.SelectedSkin : defaultSkin;
        spriteRenderer.sprite = activeSkin.back;

        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        isTransitioning = true;
        float duration = 0.3f;
        float elapsed = 0f;
        Color c = spriteRenderer.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            spriteRenderer.color = c;
            yield return null;
        }
        c.a = 1f;
        spriteRenderer.color = c;
        isTransitioning = false;
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            (GameManager.Instance.State != GameManager.GameState.Playing || isTransitioning))
            return;

        UpdateShield();
        HandleMovement();
        HandleShooting();
    }

    // Shield pickup: no life lost from bubbles for the duration; picking another one extends it.
    public void ActivateShield(float duration)
    {
        shieldRemaining = Mathf.Max(shieldRemaining, duration);
        SetTint(shieldTint);
    }

    private void UpdateShield()
    {
        if (shieldRemaining <= 0f) return;

        shieldRemaining -= Time.deltaTime;
        if (shieldRemaining <= 0f)
        {
            shieldRemaining = 0f;
            SetTint(Color.white);
        }
    }

    // Changes RGB only; alpha belongs to the start-of-game fade-in.
    private void SetTint(Color tint)
    {
        Color c = spriteRenderer.color;
        spriteRenderer.color = new Color(tint.r, tint.g, tint.b, c.a);
    }

    private void HandleMovement()
    {
        float horizontal = 0f;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            horizontal -= 1f;

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            horizontal += 1f;

        // side.png faces left in the source art, so it's flipped only when walking right.
        if (horizontal < 0f)
        {
            spriteRenderer.sprite = activeSkin.side;
            spriteRenderer.flipX = false;
        }
        else if (horizontal > 0f)
        {
            spriteRenderer.sprite = activeSkin.side;
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.sprite = activeSkin.back;   // standing still = BACK, not FRONT
            spriteRenderer.flipX = false;
        }

        Vector3 position = transform.position;
        position.x += horizontal * movementSpeed * Time.deltaTime;

        float halfScreenWidth = Camera.main.orthographicSize * Camera.main.aspect;
        GetVisibleExtentsX(out float visibleLeft, out float visibleRight);
        float minX = -halfScreenWidth - visibleLeft;
        float maxX = halfScreenWidth - visibleRight;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        transform.position = position;
    }

    // World-space distance from the pivot to the leftmost/rightmost visible pixel.
    // The player art has transparent padding on one side only, so the renderer bounds
    // (full sprite rect) would stop the player short of one wall. With Mesh Type = Tight,
    // sprite.vertices outline just the opaque area, so no Read/Write texture access is needed.
    private void GetVisibleExtentsX(out float left, out float right)
    {
        Sprite sprite = spriteRenderer.sprite;
        if (sprite != cachedSprite)
        {
            cachedSprite = sprite;
            cachedMinX = float.MaxValue;
            cachedMaxX = float.MinValue;
            foreach (Vector2 vertex in sprite.vertices)
            {
                cachedMinX = Mathf.Min(cachedMinX, vertex.x);
                cachedMaxX = Mathf.Max(cachedMaxX, vertex.x);
            }
        }

        float scaleX = transform.lossyScale.x;
        left = (spriteRenderer.flipX ? -cachedMaxX : cachedMinX) * scaleX;
        right = (spriteRenderer.flipX ? -cachedMinX : cachedMaxX) * scaleX;
    }

    private void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (activeProjectile == null || !activeProjectile.activeSelf))
        {
            activeProjectile = ProjectilePool.Instance.GetProjectile();
            activeProjectile.transform.position = transform.position + Vector3.up * shootOffsetY;
            activeProjectile.transform.rotation = Quaternion.identity;
            activeProjectile.SetActive(true);
            AudioManager.Instance?.PlayShoot();
        }
    }

    // Requires a Collider2D (isTrigger = true) on the Player GameObject — see setup checklist.
    // Stay (not Enter) so a bubble still overlapping when invulnerability ends still costs a life.
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Bubble")) return;

        TakeHit();
    }

    private void TakeHit()
    {
        if (isInvulnerable || IsShielded) return;

        GameManager.Instance.LoseLife();

        // Final hit: the game freezes before Game Over; skip the flicker so the
        // player isn't left invisible (the flicker's first step hides the sprite).
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        StartCoroutine(InvulnerabilityRoutine());
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;

        while (elapsed < invulnDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flickerInterval);
            elapsed += flickerInterval;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }
}
