using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct RotateAnimator: IComponentData
{
    public float Speed;
}

public readonly struct RotateAnimatorAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRO<RotateAnimator> m_animator;

    public RotateAnimatorAspect(Entity entity, RefRW<LocalTransform> transform, RefRO<RotateAnimator> animator)
    {
        Entity = entity;
        m_transform = transform;
        m_animator = animator;
    }

    public void UpdateAnimation(float timeDelta)
    {
        var speed = m_animator.ValueRO.Speed;
        var rotation = m_transform.ValueRO.Rotation;
        var rotDelta = quaternion.EulerXYZ(0, 0, timeDelta * speed);
        rotation = math.normalize(math.mul(rotDelta, rotation));
        m_transform.ValueRW.Rotation = rotation;
    }
}
