using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour
{
    public float speed = 8f;
    public float trailLength = 1.2f;

    private SpriteRenderer spriteRenderer;
    private LineRenderer trailRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<LineRenderer>();
    }

    // A pooled arrow keeps its old trail positions until the next Update; refresh on
    // activation so the first frame never draws the previous shot's trail.
    private void OnEnable()
    {
        UpdateTrail();
    }

    private void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
        UpdateTrail();

        float halfScreenHeight = Camera.main.orthographicSize;
        float halfSpriteHeight = spriteRenderer.bounds.extents.y;
        float topEdge = Camera.main.transform.position.y + halfScreenHeight + halfSpriteHeight;

        if (transform.position.y > topEdge)
        {
            ProjectilePool.Instance.ReturnProjectile(gameObject);
        }
    }

    private void UpdateTrail()
    {
        if (trailRenderer == null) return;
        // Anchor at the bottom of the arrow sprite, not its centre (pivot), so the
        // sprite doesn't hide the trail.
        float halfHeight = spriteRenderer.bounds.extents.y;
        Vector3 arrowBottom = transform.position - Vector3.up * halfHeight;
        trailRenderer.SetPosition(0, arrowBottom - Vector3.up * trailLength);
        trailRenderer.SetPosition(1, arrowBottom);
    }

    // Projectile's collider is a trigger (see prefab); Bubble's Rigidbody2D is what
    // makes the 2D physics system fire this event.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Bubble")) return;

        Bubble bubble = other.GetComponent<Bubble>();
        bubble?.Pop();

        ProjectilePool.Instance.ReturnProjectile(gameObject);
    }
}
