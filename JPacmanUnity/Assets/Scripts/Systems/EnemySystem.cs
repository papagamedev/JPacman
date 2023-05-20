using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PlayerSystem))]
public partial struct EnemySystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<Player>();
        state.RequireForUpdate<Main>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        var player = SystemAPI.GetSingletonEntity<PlayerAspect>();
        var playerAspect = SystemAPI.GetAspect<PlayerAspect>(player);
        var playerWorldPos = playerAspect.GetWorldPos();
        var playerCollisionRadius = playerAspect.GetCollisionRadius();
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        var mapsBlobRef = mainComponent.ValueRO.MapsConfigBlob;
        ref var map = ref gameAspect.GetCurrentMapData();
        var playerMapPos = map.WorldToMapPos(playerWorldPos);
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        new EnemyFollowPlayerJob
        {
            DeltaTime = deltaTime,
            BlobMapsRef = mapsBlobRef,
            MapId = map.Id,
            PlayerMapPos = playerMapPos,
            PlayerCollisionRadius = playerCollisionRadius,
            EnemyCI = gameAspect.LevelData.EnemyCI,
            MainEntity = mainEntity,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
        new EnemyScaredJob
        {
            DeltaTime = deltaTime,
            BlobMapsRef = mapsBlobRef,
            MapId = map.Id,
            PlayerMapPos = playerMapPos,
            PlayerCollisionRadius = playerCollisionRadius,
            Main = mainEntity,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
        new EnemyHomeJob
        {
            DeltaTime = deltaTime,
            BlobMapsRef = mapsBlobRef,
            MapId = map.Id,
            LiveTime = gameAspect.LiveTime,
            ExitHomeTime = gameAspect.LevelData.EnemyInHomeTime,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

public partial struct EnemyHomeJob : IJobEntity
{
    public float DeltaTime;
    public BlobAssetReference<MapsConfigData> BlobMapsRef;
    public int MapId;
    public float LiveTime;
    public float ExitHomeTime;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(EnemyHomeAspect enemy, [EntityIndexInQuery] int sortKey)
    {
        enemy.Update(BlobMapsRef, MapId, sortKey, LiveTime, ExitHomeTime, ECB);
    }
}

public partial struct EnemyFollowPlayerJob : IJobEntity
{
    public float DeltaTime;
    public BlobAssetReference<MapsConfigData> BlobMapsRef;
    public int MapId;
    public float2 PlayerMapPos;
    public float PlayerCollisionRadius;
    public int EnemyCI;
    public Entity MainEntity;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(EnemyFollowPlayerAspect enemy, [EntityIndexInQuery] int sortKey)
    {
        enemy.Update(BlobMapsRef, MapId, PlayerMapPos, PlayerCollisionRadius, sortKey, EnemyCI, MainEntity, ECB);
    }
}

public partial struct EnemyScaredJob : IJobEntity
{
    public float DeltaTime;
    public BlobAssetReference<MapsConfigData> BlobMapsRef;
    public int MapId;
    public float2 PlayerMapPos;
    public float PlayerCollisionRadius;
    public Entity Main;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(EnemyScaredAspect enemy, [EntityIndexInQuery] int sortKey)
    {
        enemy.Update(BlobMapsRef, MapId, PlayerMapPos, PlayerCollisionRadius, sortKey, Main, ECB);
    }
}
