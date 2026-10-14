using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
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
        var gameAspect = new GameAspect(ref state, mainEntity);
        if (gameAspect.IsPaused)
        {
            return;
        }

        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        var player = SystemAPI.GetSingletonEntity<Player>();
        if (!SystemAPI.HasComponent<Movable>(player))
        {
            // player is teleporting!
            return;
        }
        var playerAspect = PlayerAspect.Get(ref state, player);
        var playerWorldPos = playerAspect.GetWorldPos();
        var playerCollisionRadius = playerAspect.GetCollisionRadius();
        var mapsBlobRef = mainComponent.ValueRO.MapsConfigBlob;
        ref var map = ref gameAspect.GetCurrentMapData();
        var playerMapPos = map.WorldToMapPos(playerWorldPos);
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        new CollectJob
        {
            DeltaTime = deltaTime,
            BlobMapsRef = mapsBlobRef,
            MapId = map.Idx,
            PlayerMapPos = playerMapPos,
            PlayerCollisionRadius = playerCollisionRadius,
            FruitScore = gameAspect.LevelData.FruitScore,
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
    public int FruitScore;
    public Entity Main;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(Entity entity, RefRO<LocalTransform> transform, RefRO<Collectible> collectible, RefRO<CollisionCircle> collision, [EntityIndexInQuery] int sortKey)
    {
        new CollectibleAspect(entity, transform, collectible, collision).CheckPlayer(BlobMapsRef, MapId, PlayerMapPos, PlayerCollisionRadius, FruitScore, sortKey, Main, ECB);
    }
}

