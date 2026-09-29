using UnityEngine;
using UnityEngine.UI;

// HUD line under the level number showing active power-ups and their time left.
public class PowerUpStatusUI : MonoBehaviour
{
    public Text freezeText;
    public Text shieldText;
    public Player player;

    private void Update()
    {
        LevelManager levels = LevelManager.Instance;
        bool frozen = levels != null && levels.BubblesFrozen;
        bool shielded = player != null && player.IsShielded;

        if (freezeText)
        {
            freezeText.gameObject.SetActive(frozen);
            if (frozen) freezeText.text = $"Freeze {levels.FreezeRemaining:0.0}s";
        }

        if (shieldText)
        {
            shieldText.gameObject.SetActive(shielded);
            if (shielded) shieldText.text = $"Shield {player.ShieldRemaining:0.0}s";
        }
    }
}
