using UnityEngine;

public class LifePickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.GainLife();
        Destroy(gameObject);
    }
}
