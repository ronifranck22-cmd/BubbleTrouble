using UnityEngine;

// Sound effects. PlayOneShot, so overlapping sounds don't cut each other off.
// Per-clip volumes even out the source files' loudness (they differ by ~8 dB).
// Not affected by the Game Over freeze: AudioSource ignores Time.timeScale.
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    public AudioClip shootClip;
    public AudioClip bubblePopClip;
    public AudioClip playerHitClip;
    public AudioClip pickupClip;
    public AudioClip shieldPickupClip;
    public AudioClip freezePickupClip;
    public AudioClip levelClearClip;
    public AudioClip gameOverClip;
    public AudioClip winClip;

    [Header("Volumes (balance the clips against each other)")]
    [Range(0f, 1f)] public float shootVolume = 1f;
    [Range(0f, 1f)] public float bubblePopVolume = 1f;
    [Range(0f, 1f)] public float playerHitVolume = 1f;
    [Range(0f, 1f)] public float pickupVolume = 1f;
    [Range(0f, 1f)] public float shieldPickupVolume = 1f;
    [Range(0f, 1f)] public float freezePickupVolume = 1f;
    [Range(0f, 1f)] public float levelClearVolume = 1f;
    [Range(0f, 1f)] public float gameOverVolume = 1f;
    [Range(0f, 1f)] public float winVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    // Callers use AudioManager.Instance?.Play...(); clear the reference on destroy
    // (e.g. scene reload) so ?. sees a real null instead of a destroyed object.
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayShoot() => Play(shootClip, shootVolume);
    public void PlayBubblePop() => Play(bubblePopClip, bubblePopVolume);
    public void PlayPlayerHit() => Play(playerHitClip, playerHitVolume);
    public void PlayPickup() => Play(pickupClip, pickupVolume);
    public void PlayShieldPickup() => Play(shieldPickupClip, shieldPickupVolume);
    public void PlayFreezePickup() => Play(freezePickupClip, freezePickupVolume);
    public void PlayLevelClear() => Play(levelClearClip, levelClearVolume);
    public void PlayGameOver() => Play(gameOverClip, gameOverVolume);
    public void PlayWin() => Play(winClip, winVolume);

    private void Play(AudioClip clip, float volume)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }
}
