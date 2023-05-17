using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class EnemyColorDef : IComponentData
{
    public Color[] EnemyColors;
    public Color EnemyScaredColor;
}

public struct Enemy : IComponentData
{
    public int Id;
    public bool Scared;
}

public readonly partial struct EnemyAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRW<Enemy> m_enemy;
}
