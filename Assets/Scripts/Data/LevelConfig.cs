using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "BubbleTrouble/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Serializable]
    public class BubbleSpawn
    {
        public BubbleConfig config;
        public int count = 1;
    }

    [Tooltip("Shown in the HUD; purely cosmetic, order in the LevelManager list is what actually drives progression.")]
    public int levelNumber = 1;

    public BubbleSpawn[] startingBubbles;

    [Tooltip("Seconds before the level auto-fails. -1 = no timer (not decided yet per GDD section 3/8.1).")]
    public float levelTimer = -1f;
}
