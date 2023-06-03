using Unity.Burst;
using Unity.Entities;


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
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        if (gameAspect.IsPaused)
        {
            return;
        }
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        var mapsBlobRef = mainComponent.ValueRO.MapsConfigBlob;
        ref var map = ref gameAspect.GetCurrentMapData();
        new MoveEntityJob
        {
            DeltaTime = deltaTime,
            BlobMapsRef = mapsBlobRef,
            MapId = map.Id,
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
    public int MapId;

    private void Execute(MovableAspect movable)
    {
       movable.Move(BlobMapsRef, MapId, DeltaTime);
    }
}

