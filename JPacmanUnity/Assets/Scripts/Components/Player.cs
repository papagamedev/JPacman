using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Player : IComponentData
{
}

public readonly partial struct PlayerAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<CollisionCircle> m_collision;
#pragma warning disable IDE0052 // Remove unread private members
    private readonly RefRW<Player> _;
#pragma warning restore IDE0052 // Remove unread private members

    public readonly float3 GetWorldPos() => m_transform.ValueRO.Position;
    public readonly float GetCollisionRadius() => m_collision.ValueRO.Radius;

    public void UpdateInput(float2 desiredDirection)
    {
        if (math.abs(desiredDirection.x) < 0.3f && math.abs(desiredDirection.y) < 0.3f)
        {
            return;
        }

        if (desiredDirection.x > 0)
        {
            m_movable.ValueRW.DesiredDir = Direction.Right;
        }
        else if (desiredDirection.y > 0)
        {
            m_movable.ValueRW.DesiredDir = Direction.Down;
        }
        else if (desiredDirection.x < 0)
        {
            m_movable.ValueRW.DesiredDir = Direction.Left;
        }
        else if (desiredDirection.y < 0)
        {
            m_movable.ValueRW.DesiredDir = Direction.Up;
        }
    }

}
