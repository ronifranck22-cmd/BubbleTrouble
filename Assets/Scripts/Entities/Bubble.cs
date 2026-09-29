using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Bubble : MonoBehaviour
{
    private BubbleConfig config;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isFrozen;
    private Vector2 frozenVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Called by LevelManager right after Instantiate.
    public void Initialize(BubbleConfig bubbleConfig)
    {
        config = bubbleConfig;
        transform.localScale = Vector3.one * config.scale;
        spriteRenderer.color = config.color;
        Launch();
    }

    private void Launch()
    {
        float dirX = Random.value > 0.5f ? 1f : -1f;
        rb.linearVelocity = new Vector2(config.initialSpeed * dirX, config.initialSpeed);
    }

    // Time Freeze: hold the bubble exactly where it is and resume with the same velocity.
    // Kinematic (not simulated = false) keeps its collider active, so a frozen bubble can
    // still be shot and split (its children are frozen too via LevelManager).
    public void SetFrozen(bool frozen)
    {
        if (frozen == isFrozen) return;
        isFrozen = frozen;

        if (frozen)
        {
            frozenVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = frozenVelocity;
        }
    }

    // Called by Projectile.OnTriggerEnter2D when this bubble gets hit.
    public void Pop()
    {
        GameManager.Instance.AddScore(config.score);

        if (config.nextSizeDown != null)
        {
            SpawnChild(Vector2.left);
            SpawnChild(Vector2.right);
        }

        LevelManager.Instance.NotifyBubbleRemoved(gameObject);
        Destroy(gameObject);
    }

    private void SpawnChild(Vector2 direction)
    {
        GameObject childObj = Instantiate(LevelManager.Instance.bubblePrefab, transform.position, Quaternion.identity);
        Bubble childBubble = childObj.GetComponent<Bubble>();
        childBubble.config = config.nextSizeDown;
        childBubble.spriteRenderer.color = config.nextSizeDown.color;
        childObj.transform.localScale = Vector3.one * config.nextSizeDown.scale;
        childBubble.rb.linearVelocity = new Vector2(direction.x * config.nextSizeDown.initialSpeed, config.nextSizeDown.initialSpeed);

        LevelManager.Instance.RegisterSpawnedBubble(childObj);
    }
}
