using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PlayerSystem))]
public partial struct CollectibleSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<Collectible>();
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
        new CollectJob
        {
            DeltaTime = deltaTime,
            BlobMapsRef = mapsBlobRef,
            MapId = map.Id,
            PlayerMapPos = playerMapPos,
            PlayerCollisionRadius = playerCollisionRadius,
            Main = mainEntity,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

public partial struct CollectJob : IJobEntity
{
    public float DeltaTime;
    public BlobAssetReference<MapsConfigData> BlobMapsRef;
    public int MapId;
    public float2 PlayerMapPos;
    public float PlayerCollisionRadius;
    public Entity Main;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(CollectibleAspect collectible, [EntityIndexInQuery] int sortKey)
    {
        collectible.CheckPlayer(BlobMapsRef, MapId, PlayerMapPos, PlayerCollisionRadius, sortKey, Main, ECB);
    }
}

