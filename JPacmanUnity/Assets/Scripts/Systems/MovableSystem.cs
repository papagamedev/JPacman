using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PlayerSystem))]
public partial struct MovableSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<Movable>();
        state.RequireForUpdate<Main>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var gameAspect = new GameAspect(mainEntity,
            SystemAPI.GetComponentRW<Main>(mainEntity), SystemAPI.GetComponentRW<Game>(mainEntity),
            SystemAPI.GetBuffer<AddScoreBufferElement>(mainEntity), SystemAPI.GetBuffer<DotCloneBufferElement>(mainEntity));
        if (gameAspect.IsPaused)
        {
            return;
        }
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        var mapsBlobRef = mainComponent.ValueRO.MapsConfigBlob;
        ref var map = ref gameAspect.GetCurrentMapData();
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        new MoveEntityJob
        {
            DeltaTime = deltaTime,
            BlobMapsRef = mapsBlobRef,
            MapId = map.Idx,
            MainEntity = mainEntity,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

public partial struct MoveEntityJob : IJobEntity
{
    public float DeltaTime;
    public BlobAssetReference<MapsConfigData> BlobMapsRef;
    public Entity MainEntity;
    public int MapId;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(Entity entity, RefRW<LocalTransform> transform, RefRW<Movable> movable, [EntityIndexInQuery] int sortKey)
    {
       new MovableAspect(entity, transform, movable).Move(BlobMapsRef, MapId, DeltaTime, MainEntity, sortKey, ECB);
    }
}

