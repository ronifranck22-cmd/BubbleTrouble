using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float shootOffsetY = 0.5f;

    private SpriteRenderer spriteRenderer;
    private GameObject activeProjectile;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        float horizontal = 0f;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            horizontal -= 1f;

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            horizontal += 1f;

        Vector3 position = transform.position;
        position.x += horizontal * movementSpeed * Time.deltaTime;

        float halfScreenWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float halfSpriteWidth = spriteRenderer.bounds.extents.x;
        float maxX = halfScreenWidth - halfSpriteWidth;
        float minX = -maxX;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        transform.position = position;

        if (Input.GetKeyDown(KeyCode.Space) && (activeProjectile == null || !activeProjectile.activeSelf))
        {
            activeProjectile = ProjectilePool.Instance.GetProjectile();
            activeProjectile.transform.position = transform.position + Vector3.up * shootOffsetY;
            activeProjectile.transform.rotation = Quaternion.identity;
            activeProjectile.SetActive(true);
        }
    }
}
