using UnityEngine;

[CreateAssetMenu(fileName = "MenuConfig", menuName = "JPacman Data/Menu Config")]
public class MenuConfig : ScriptableObject
{
    public float DotSpacing;
    public float PlayerSpeed;
    public float EnemySpeed;
    public Vector2 PlayerBoundsSize;
    public Vector2 EnemiesBoundsSize;
    public Vector2 DotsShapePos;
    [Multiline(10)]
    public string DotsShape;
}
