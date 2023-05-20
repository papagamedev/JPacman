using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class EnemyDef : IComponentData
{
    public UnityEngine.Color[] EnemyColors;
    public UnityEngine.Color EnemyScaredColor;
}

public struct Enemy : IComponentData
{
    public int Id;
}

public struct EnemyHome : IComponentData
{
}

public struct EnemyFollowPlayer : IComponentData
{
}

public struct EnemyScared : IComponentData
{
}


public readonly partial struct EnemyHomeAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<Enemy> m_enemy;
    private readonly RefRO<EnemyHome> m_enemyHome;

    public void Update(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, int sortKey, float liveTime, float exitHomeTime, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];

        if (liveTime > exitHomeTime * m_enemy.ValueRO.Id 
            && m_movable.ValueRO.NextCellEdgeMapPos.Equals(mapData.EnemyHousePos))
        {
            m_movable.ValueRW.ForcedDir = true;
            m_movable.ValueRW.DesiredDir = mapData.EnemyExitDir;
            ecb.RemoveComponent<EnemyHome>(sortKey, Entity);
            ecb.AddComponent(sortKey, Entity, new EnemyFollowPlayer() { });
            return;
        }

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
    private readonly RefRO<EnemyFollowPlayer> m_enemyFollowPLayer;
    private readonly RefRO<CollisionCircle> m_collision;

    public void Update(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, float2 playerMapPos, float playerCollisionRadius, int sortKey, int enemyCI, Entity mainEntity, EntityCommandBuffer.ParallelWriter ecb)
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
    private readonly RefRO<Enemy> m_enemy;
    private readonly RefRO<EnemyScared> m_enemyScared;
    private readonly RefRO<CollisionCircle> m_collision;

    public void Update(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, float2 playerMapPos, float playerCollisionRadius, int sortKey, Entity main, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];
        var enemyWorldPos = m_transform.ValueRO.Position;
        var enemyMapPos = mapData.WorldToMapPos(enemyWorldPos);

        if (CollisionCircle.CheckCollision(enemyMapPos, playerMapPos, playerCollisionRadius + m_collision.ValueRO.Radius))
        {
            // enemy captured by player!!

            return;
        }

        /*
                DirsCanMove(Goblins[i],dirs);
				if ((dirs[0]+dirs[1]+dirs[2]+dirs[3]>2) || (!dirs[Goblins[i]->dir]))
					FollowPos(Goblins[i],HouseX,HouseY,dirs,5);
				MakeMove(Goblins[i],dirs);
        */
    }

}


