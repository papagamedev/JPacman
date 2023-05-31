using UnityEngine;

[CreateAssetMenu(fileName="FruitConfig", menuName = "JPacman Data/Fruit Config")]
public class FruitConfig : ScriptableObject
{
    public Sprite Sprite;
    public int SpriteIdx;
    public int Score;
    public float WaitTime;
    public float Duration;
}
