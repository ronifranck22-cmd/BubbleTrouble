using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkin", menuName = "BubbleTrouble/Player Skin")]
public class PlayerSkin : ScriptableObject
{
    public string skinName = "Classic";
    public Sprite front;
    public Sprite back;
    public Sprite side;
}
