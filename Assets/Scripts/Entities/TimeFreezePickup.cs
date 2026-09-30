using UnityEngine;

public class TimeFreezePickup : MonoBehaviour
{
    public float duration = 3.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        LevelManager.Instance.FreezeBubbles(duration);
        AudioManager.Instance?.PlayFreezePickup();
        Destroy(gameObject);
    }
}
