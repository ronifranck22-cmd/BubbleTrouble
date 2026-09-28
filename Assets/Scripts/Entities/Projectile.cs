using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour
{
    public float speed = 8f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;

        float halfScreenHeight = Camera.main.orthographicSize;
        float halfSpriteHeight = spriteRenderer.bounds.extents.y;
        float topEdge = Camera.main.transform.position.y + halfScreenHeight + halfSpriteHeight;

        if (transform.position.y > topEdge)
        {
            ProjectilePool.Instance.ReturnProjectile(gameObject);
        }
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
