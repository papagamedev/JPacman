using UnityEngine;

[CreateAssetMenu(fileName = "MenuDotShapeConfig", menuName = "JPacman Data/Menu Dot Shape Config")]
public class MenuDotShapeConfig : ScriptableObject
{
    public float DotSpacing;
    [Multiline(20)]
    public string Shape;
}
