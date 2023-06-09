using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Teleportable : IComponentData
{
    public float Duration;
    public float Time;
    public float3 StartWorldPos;
    public float3 TargetWorldPos;
    public float2 TargetMapPos;
    public float MovableSpeed;
    public float MovableSpeedInTunnel;
    public bool MovableAllowChangeDirInMidCell;
    public Direction MovableDirection;
    public Random Rand;
}

public readonly partial struct TeleportableAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRW<Teleportable> m_teleportable;

    public void Teleport(float deltaTime, int sortKey, EntityCommandBuffer.ParallelWriter ecb)
    {
        m_teleportable.ValueRW.Time += deltaTime;
        var time = m_teleportable.ValueRO.Time;
        var duration = m_teleportable.ValueRO.Duration;
        if (time < duration)
        {
            var startPos = m_teleportable.ValueRO.StartWorldPos;
            var targetPos = m_teleportable.ValueRO.TargetWorldPos;
            var factor = time / duration;                   // [0 , 1]
            factor = factor * factor * (3.0f - 2.0f * factor);      // [0 , 1]
            m_transform.ValueRW.Position = targetPos * factor + startPos * (1.0f - factor);

            var scaleFactor = math.abs(factor - 0.5f); // [0.0 , 0.5]
            scaleFactor *= scaleFactor * scaleFactor;             // [0.0 , 0.125]
            var scale = 0.125f + scaleFactor * 7.0f;  // [0.125 , 1.0]
            m_transform.ValueRW.Scale = scale;
            return;
        }

        m_transform.ValueRW.Position = m_teleportable.ValueRO.TargetWorldPos;
        m_transform.ValueRW.Scale = 1.0f;

        ecb.RemoveComponent<Teleportable>(sortKey, Entity);
        ecb.AddComponent(sortKey, Entity, new Movable()
        {
            Init = true,
            AllowChangeDirInMidCell = m_teleportable.ValueRO.MovableAllowChangeDirInMidCell,
            CurrentDir = m_teleportable.ValueRO.MovableDirection,
            ForcedDir = true,
            DesiredDir = m_teleportable.ValueRO.MovableDirection,
            Speed = m_teleportable.ValueRO.MovableSpeed,
            SpeedInTunnel = m_teleportable.ValueRO.MovableSpeedInTunnel,
            LastCellEdgeMapPos = m_teleportable.ValueRO.TargetMapPos,
            NextCellEdgeMapPos = m_teleportable.ValueRO.TargetMapPos,
            IsInTunnel = true,
            CanDoTeleporting = true,
            JustTeleported = true,
            Rand = new Random(m_teleportable.ValueRO.Rand.state)
        });
    }
}