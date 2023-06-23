using UnityEngine;

[CreateAssetMenu(fileName = "MenuConfig", menuName = "JPacman Data/Menu Config")]
public class MenuConfig : ScriptableObject
{
    public float PlayerSpeed;
    public float EnemySpeed;
    public Vector2 PlayerBoundsSize;
    public Vector2 EnemiesBoundsSize;
}
