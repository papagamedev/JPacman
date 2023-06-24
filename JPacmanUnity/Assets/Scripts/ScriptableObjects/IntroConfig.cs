using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IntroConfig", menuName = "JPacman Data/Intro Config")]
public class IntroConfig : ScriptableObject
{
	[Serializable]
	public class ShapeData
	{
		public Vector2 Pos;
		public float Duration;
        public MenuDotShapeConfig Shape;
	}

	public ShapeData[] DotShapes;
    public float DotSpeed;
    public Vector2 PlayerStartPos;
    public float PlayerSpeed;
    public Vector2 EnemyStartPos;
    public float EnemySpacing;
    public float EnemyFollowDuration;
    public float EnemyFollowSpeed;
    public float EnemyScaredDuration;
    public float EnemyScaredSpeed;
}
