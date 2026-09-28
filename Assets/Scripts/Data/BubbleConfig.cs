using UnityEngine;

[CreateAssetMenu(fileName = "BubbleConfig", menuName = "BubbleTrouble/Bubble Config")]
public class BubbleConfig : ScriptableObject
{
    [Tooltip("Uniform scale applied to the shared bubble prefab for this size tier.")]
    public float scale = 1f;

    [Tooltip("Points awarded when a bubble of this size is popped.")]
    public int score = 10;

    [Tooltip("Speed magnitude used for both the horizontal and vertical starting velocity.")]
    public float initialSpeed = 3f;

    [Tooltip("Config used for the two bubbles spawned when this one is popped. Leave empty for the smallest tier (pops and clears, nothing spawned).")]
    public BubbleConfig nextSizeDown;

    [Tooltip("Tint applied to the shared bubble sprite for this size tier.")]
    public Color color = Color.white;
}
