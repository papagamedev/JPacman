using Unity.Entities;
using Unity.Transforms;

public struct DeathAnimation : IComponentData
{
    public float Duration;
}

public readonly struct DeathAnimationAspect
{
    const float kMinScale = 0.01f;
    const float kRotationSpeed = 10.0f;

    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRO<DeathAnimation> m_death;

    public DeathAnimationAspect(Entity entity, RefRW<LocalTransform> transform, RefRO<DeathAnimation> death)
    {
        Entity = entity;
        m_transform = transform;
        m_death = death;
    }
    
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


