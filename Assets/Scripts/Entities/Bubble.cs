using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Bubble : MonoBehaviour
{
    private BubbleConfig config;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Called by LevelManager right after Instantiate.
    public void Initialize(BubbleConfig bubbleConfig)
    {
        config = bubbleConfig;
        transform.localScale = Vector3.one * config.scale;
        Launch();
    }

    private void Launch()
    {
        float dirX = Random.value > 0.5f ? 1f : -1f;
        rb.linearVelocity = new Vector2(config.initialSpeed * dirX, config.initialSpeed);
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
        childObj.transform.localScale = Vector3.one * config.nextSizeDown.scale;
        childBubble.rb.linearVelocity = new Vector2(direction.x * config.nextSizeDown.initialSpeed, config.nextSizeDown.initialSpeed);

        LevelManager.Instance.RegisterSpawnedBubble(childObj);
    }
}
