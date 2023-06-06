using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct MoveAnimator: IComponentData
{
    public float Speed;
    public Direction Direction;
}

public readonly partial struct MoveAnimatorAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRO<MoveAnimator> m_animator;

    public void UpdateAnimation(float timeDelta)
    {
        var direction = new float3(m_animator.ValueRO.Direction.Vector(), 0);
        var speed = m_animator.ValueRO.Speed;
        var position = m_transform.ValueRO.Position;
        var posDelta = direction * timeDelta * speed;
        position = position + posDelta;
        m_transform.ValueRW.Position = position;
    }
}
