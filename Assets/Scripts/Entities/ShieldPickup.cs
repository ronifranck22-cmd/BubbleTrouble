using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Player player = other.GetComponent<Player>();
        if (player != null)
            player.ActivateShield(duration);
        AudioManager.Instance?.PlayShieldPickup();
        Destroy(gameObject);
    }
}
