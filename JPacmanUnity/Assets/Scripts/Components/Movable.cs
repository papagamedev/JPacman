using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Movable : IComponentData
{
    public float Speed;
    public float2 Direction;
    public float2 DesiredDirection;
}

public readonly partial struct MovableAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRW<Movable> m_movable;

    public void Move(ref MapConfigData map, float deltaTime)
    {
        m_movable.ValueRW.Direction = m_movable.ValueRO.DesiredDirection;

        float2 mapPos = map.WorldToMapPos(m_transform.ValueRO.Position);
        mapPos += m_movable.ValueRO.Direction * m_movable.ValueRO.Speed * deltaTime;
        m_transform.ValueRW.Position = map.MapToWorldPos(mapPos);
    }
}
