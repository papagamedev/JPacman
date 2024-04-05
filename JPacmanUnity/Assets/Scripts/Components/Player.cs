using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Player : IComponentData
{
    public float2 MoveVector;
}

public readonly partial struct PlayerAspect : IAspect
{
    const float kInputThreshold = 0.5f;

    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<CollisionCircle> m_collision;
    private readonly RefRW<Player> m_player;

    public readonly float3 GetWorldPos() => m_transform.ValueRO.Position;
    public readonly float GetCollisionRadius() => m_collision.ValueRO.Radius;

    public void UpdateMovement()
    {
        var moveVector = m_player.ValueRO.MoveVector;
        if (math.abs(moveVector.x) > kInputThreshold)
        {
            if (moveVector.x > 0)
            {
                m_movable.ValueRW.DesiredDir = Direction.Right;
            }
            else
            {
                m_movable.ValueRW.DesiredDir = Direction.Left;
            }
        }
        else if (math.abs(moveVector.y) > kInputThreshold)
        {
            if (moveVector.y > 0)
            {
                m_movable.ValueRW.DesiredDir = Direction.Up;
            }
            else
            {
                m_movable.ValueRW.DesiredDir = Direction.Down;
            }
        }
    }

}
