using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct DeathAnimation : IComponentData
{
    public float Duration;
}

public readonly partial struct DeathAnimationAspect : IAspect
{
    const float kMinScale = 0.01f;
    const float kRotationSpeed = 10.0f;

    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRO<DeathAnimation> m_death;
    
    public void UpdateAnimation(float deltaTime)
    {
        m_transform.ValueRW = m_transform.ValueRO.RotateZ(deltaTime * kRotationSpeed);

        var newScale = m_transform.ValueRO.Scale - deltaTime / m_death.ValueRO.Duration;
        if (newScale < kMinScale)
        {
            newScale = kMinScale;
        }
        m_transform.ValueRW.Scale = newScale;
    }
}


