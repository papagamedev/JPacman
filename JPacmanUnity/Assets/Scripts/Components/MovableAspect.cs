using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct MovableAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRO<Movable> m_movable;

    public void Move(float deltaTime)
    {
        m_transform.ValueRW.Position += (math.right() * m_movable.ValueRO.Speed * deltaTime);
    }
}

