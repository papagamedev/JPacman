using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public Color[] m_enemyColors;
    public Color m_enemyScaredColor;
    public Color EnemyScaredBlinkColor;
    public Color EnemyReturnHomeColor;
    public float m_collisionRadius;

    public class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new EnemyDef
            {
                EnemyColors = authoring.m_enemyColors,
                EnemyScaredColor = authoring.m_enemyScaredColor,
                EnemyScaredBlinkColor = authoring.EnemyScaredBlinkColor,
                EnemyReturnHomeColor = authoring.EnemyReturnHomeColor
            });
            AddComponent(entity, new CollisionCircle()
            {
                Radius = authoring.m_collisionRadius
            });
        }
    }
}


