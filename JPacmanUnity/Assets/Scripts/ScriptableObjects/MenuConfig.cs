using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MenuConfig", menuName = "JPacman Data/Menu Config")]
public class MenuConfig : ScriptableObject
{
    [Serializable]
    public class ShapeData
    {
        public Vector2 Pos;
        public UIEvents.ShowUIType UIType;
        public MenuDotShapeConfig Shape;
    }

    public float DotSpeed;
    public float PlayerSpeed;
    public float EnemySpeed;
    public Vector2 PlayerBoundsSize;
    public Vector2 EnemiesBoundsSize;
    public ShapeData[] DotShapes;
}
