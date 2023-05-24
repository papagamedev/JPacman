using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class EnemyDef : IComponentData
{
    public UnityEngine.Color[] EnemyColors;
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

        var nextAvailableDirs = m_movable.ValueRO.NextCellEdgeAvailableDirections;
        if (nextAvailableDirs.Count == 1)
        {
            m_movable.ValueRW.DesiredDir = nextAvailableDirs.First;
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

    public void Update(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, float2 playerMapPos, float playerCollisionRadius, int sortKey, int enemyCI, float enemySpeed, Entity mainEntity, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];
        var enemyWorldPos = m_transform.ValueRO.Position;
        var enemyMapPos = mapData.WorldToMapPos(enemyWorldPos);

        if (CollisionCircle.CheckCollision(enemyMapPos, playerMapPos, playerCollisionRadius + m_collision.ValueRO.Radius))
        {
            ecb.AddComponent(sortKey, mainEntity, new LevelDeadPhaseTag());
            ecb.RemoveComponent<LevelPlayingPhaseTag>(sortKey, mainEntity);
            return;
        }

        m_movable.ValueRW.Speed = enemySpeed;

        var nextCellPos = m_movable.ValueRO.NextCellEdgeMapPos;
        var currentDir = m_movable.ValueRO.CurrentDir;
        var nextAvailableDirs = m_movable.ValueRO.NextCellEdgeAvailableDirections;
        if (nextAvailableDirs.Count > 2 || !nextAvailableDirs.Check(currentDir))
        {
            m_movable.ValueRW.DesiredDir = MovableAspect.ComputeFollowTargetDir(nextCellPos, currentDir, playerMapPos, nextAvailableDirs, m_enemy.ValueRO.Id + enemyCI, ref m_movable.ValueRW.Rand);
        }
    }
}

public readonly partial struct EnemyScaredAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<EnemyScaredTag> m_enemyScared;
    private readonly RefRO<CollisionCircle> m_collision;

    public void Update(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, float2 playerMapPos, float playerCollisionRadius, int sortKey, Entity mainEntity, EntityCommandBuffer.ParallelWriter ecb)
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

        m_movable.ValueRW.Speed = 4.0f;

        var nextCellPos = m_movable.ValueRO.NextCellEdgeMapPos;
        var currentDir = m_movable.ValueRO.CurrentDir;
        var nextAvailableDirs = m_movable.ValueRO.NextCellEdgeAvailableDirections;
        if (nextAvailableDirs.Count > 2 || !nextAvailableDirs.Check(currentDir))
        {
            m_movable.ValueRW.DesiredDir = MovableAspect.ComputeFollowTargetDir(nextCellPos, currentDir, mapData.EnemyHousePos, nextAvailableDirs, 5, ref m_movable.ValueRW.Rand);
        }
    }

}

public readonly partial struct EnemyHomeScaredAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<EnemyHomeScaredTag> m_enemyScared;

    public void Update(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, int sortKey, Entity mainEntity, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];

        m_movable.ValueRW.Speed = 4.0f;

        var nextAvailableDirs = m_movable.ValueRO.NextCellEdgeAvailableDirections;
        if (nextAvailableDirs.Count == 1)
        {
            m_movable.ValueRW.DesiredDir = nextAvailableDirs.First;
        }
    }

}

public readonly partial struct EnemyReturnHomeAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<EnemyReturnHomeTag> m_enemyScared;

    public void Update(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, int sortKey, Entity main, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];
        var currentDir = m_movable.ValueRO.CurrentDir;
        var enemyExitDirOpposite = Movable.OppositeDir(mapData.EnemyExitDir);
        var lastCellPos = m_movable.ValueRO.LastCellEdgeMapPos;

        if (lastCellPos.Equals(mapData.EnemyHousePos))
        {
            ecb.RemoveComponent<EnemyReturnHomeTag>(sortKey, Entity);
            ecb.AddComponent(sortKey, Entity, new EnemyHomeTag() { });
            return;
        }

        var nextCellPos = m_movable.ValueRO.NextCellEdgeMapPos;
        if (nextCellPos.Equals(mapData.EnemyExitPos))
        {
            m_movable.ValueRW.ForcedDir = true;
            m_movable.ValueRW.DesiredDir = enemyExitDirOpposite;
            return;
        }

        m_movable.ValueRW.Speed = 16.0f;

        var nextAvailableDirs = m_movable.ValueRO.NextCellEdgeAvailableDirections;
        if (nextAvailableDirs.Count > 2 || !nextAvailableDirs.Check(currentDir))
        {
            m_movable.ValueRW.DesiredDir = MovableAspect.ComputeFollowTargetDir(nextCellPos, currentDir, mapData.EnemyExitPos, nextAvailableDirs, 13, ref m_movable.ValueRW.Rand);
        }
    }

}
