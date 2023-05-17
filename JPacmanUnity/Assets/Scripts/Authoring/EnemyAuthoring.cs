using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public Color[] m_enemyColors;
    public Color m_enemyScaredColor;

    public class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new EnemyColorDef
            {
                EnemyColors = authoring.m_enemyColors,
                EnemyScaredColor = authoring.m_enemyScaredColor
            });
        }

    }
}


