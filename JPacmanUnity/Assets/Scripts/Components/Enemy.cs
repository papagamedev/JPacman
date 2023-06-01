using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct EnemyDef : IComponentData
{
    public FixedList128Bytes<UnityEngine.Color> EnemyColors;
    public UnityEngine.Color EnemyScaredColor;
    public UnityEngine.Color EnemyScaredBlinkColor;
    public UnityEngine.Color EnemyReturnHomeColor;
}

public struct Enemy : IComponentData
{
    public int Id;
}

public struct EnemyHomeTag : IComponentData { }

public struct EnemyFollowPlayerTag : IComponentData { }

public struct EnemyScaredTag : IComponentData { }

public struct EnemyHomeScaredTag : IComponentData { }

public struct EnemyReturnHomeTag : IComponentData { }


public readonly partial struct EnemyHomeAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<Enemy> m_enemy;
    private readonly RefRO<EnemyHomeTag> m_enemyHome;

    public void Update(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, int sortKey, float liveTime, float exitHomeTime, float enemySpeed, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];

        if (liveTime > exitHomeTime * m_enemy.ValueRO.Id 
            && m_movable.ValueRO.NextCellEdgeMapPos.Equals(mapData.EnemyHousePos))
        {
            m_movable.ValueRW.ForcedDir = true;
            m_movable.ValueRW.DesiredDir = mapData.EnemyExitDir;
            ecb.RemoveComponent<EnemyHomeTag>(sortKey, Entity);
            ecb.AddComponent(sortKey, Entity, new EnemyFollowPlayerTag() { });
            return;
        }

        m_movable.ValueRW.Speed = enemySpeed;

        UpdateMovableAtHome(m_movable);
    }

    public static void UpdateMovableAtHome(RefRW<Movable> movable)
    {
        var nextAvailableDirs = movable.ValueRO.NextCellEdgeAvailableDirections;
        if (nextAvailableDirs.Count == 1)
        {
            movable.ValueRW.DesiredDir = nextAvailableDirs.First;
        }
    }

}

public readonly partial struct EnemyFollowPlayerAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<Enemy> m_enemy;
    private readonly RefRO<EnemyFollowPlayerTag> m_enemyFollowPLayer;
    private readonly RefRO<CollisionCircle> m_collision;

    public void Update(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, float2 playerMapPos, float playerCollisionRadius, int enemyCI, float enemySpeed, float enemySpeedInTunnel, bool isBonus, int sortKey, Entity mainEntity, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];
        var enemyWorldPos = m_transform.ValueRO.Position;
        var enemyMapPos = mapData.WorldToMapPos(enemyWorldPos);

        // if for some reason the enemy is in follow player state and INSIDE the home, just get out!
        if (m_movable.ValueRO.NextCellEdgeMapPos.Equals(mapData.EnemyHousePos))
        {
            m_movable.ValueRW.ForcedDir = true;
            m_movable.ValueRW.DesiredDir = mapData.EnemyExitDir;
            return;
        }

        // check if player is captured
        if (CollisionCircle.CheckCollision(enemyMapPos, playerMapPos, playerCollisionRadius + m_collision.ValueRO.Radius))
        {
            ecb.RemoveComponent<LevelPlayingPhaseTag>(sortKey, mainEntity);
            if (isBonus)
            {
                ecb.AddComponent(sortKey, mainEntity, new LevelWinPhaseTag());
            }
            else
            {
                ecb.AddComponent(sortKey, mainEntity, new LevelDeadPhaseTag());
            }
            return;
        }

        // set normal speed in this state
        m_movable.ValueRW.Speed = enemySpeed;
        m_movable.ValueRW.SpeedInTunnel = enemySpeedInTunnel;

        MovableAspect.UpdateMovableFollowPos(m_movable, playerMapPos, m_enemy.ValueRO.Id + enemyCI);
    }
}

public readonly partial struct EnemyScaredAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<EnemyScaredTag> m_enemyScared;
    private readonly RefRO<CollisionCircle> m_collision;

    public void Update(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, float2 playerMapPos, float playerCollisionRadius, float enemySpeedScared, int sortKey, Entity mainEntity, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];
        var enemyWorldPos = m_transform.ValueRO.Position;
        var enemyMapPos = mapData.WorldToMapPos(enemyWorldPos);

        if (CollisionCircle.CheckCollision(enemyMapPos, playerMapPos, playerCollisionRadius + m_collision.ValueRO.Radius))
        {
            ecb.RemoveComponent<EnemyScaredTag>(sortKey, Entity);
            ecb.AddComponent(sortKey, Entity, new EnemyReturnHomeTag() { });

            ecb.AppendToBuffer(sortKey, mainEntity, new SoundEventBufferElement()
            {
                SoundType = AudioEvents.SoundType.PlayerEatEnemy
            });
            ecb.AppendToBuffer(sortKey, mainEntity, new EnemyEatenBufferElement()
            {
                WorldPos = enemyWorldPos
            });
            return;
        }

        m_movable.ValueRW.Speed = enemySpeedScared;
        m_movable.ValueRW.SpeedInTunnel = enemySpeedScared;

        MovableAspect.UpdateMovableFollowPos(m_movable, mapData.EnemyHousePos, 5);
    }
}

public readonly partial struct EnemyHomeScaredAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<EnemyHomeScaredTag> m_enemyScared;

    public void Update(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, float enemySpeedScared, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];

        m_movable.ValueRW.Speed = enemySpeedScared;

        EnemyHomeAspect.UpdateMovableAtHome(m_movable);
    }

}

public readonly partial struct EnemyReturnHomeAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<EnemyReturnHomeTag> m_enemyScared;

    public void Update(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, float enemySpeedRerturnHome, int sortKey, Entity mainEntity, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];
        var currentDir = m_movable.ValueRO.CurrentDir;
        var enemyExitDirOpposite = mapData.EnemyExitDir.Opposite();
        var lastCellPos = m_movable.ValueRO.LastCellEdgeMapPos;

        if (lastCellPos.Equals(mapData.EnemyHousePos))
        {
            ecb.RemoveComponent<EnemyReturnHomeTag>(sortKey, Entity);
            ecb.AddComponent(sortKey, Entity, new EnemyHomeTag() { });
            ecb.AppendToBuffer(sortKey, mainEntity, new EnemyReturnedHomeBufferElement()
            {
                Dummy = 0
            });
            return;
        }

        var nextCellPos = m_movable.ValueRO.NextCellEdgeMapPos;
        if (nextCellPos.Equals(mapData.EnemyExitPos))
        {
            m_movable.ValueRW.ForcedDir = true;
            m_movable.ValueRW.DesiredDir = enemyExitDirOpposite;
            return;
        }

        m_movable.ValueRW.Speed = enemySpeedRerturnHome;
        m_movable.ValueRW.SpeedInTunnel = enemySpeedRerturnHome;

        MovableAspect.UpdateMovableFollowPos(m_movable, mapData.EnemyExitPos, 13);
    }

}
