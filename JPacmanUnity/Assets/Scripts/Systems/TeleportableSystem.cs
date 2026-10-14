using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollectibleSystem))]
public partial struct TeleportableSystem: ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<Teleportable>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var gameAspect = new GameAspect(mainEntity,
            SystemAPI.GetComponentRW<Main>(mainEntity), SystemAPI.GetComponentRW<Game>(mainEntity),
            SystemAPI.GetBuffer<AddScoreBufferElement>(mainEntity), SystemAPI.GetBuffer<DotCloneBufferElement>(mainEntity));
        if (gameAspect.IsPaused)
        {
            return;
        }
        var deltaTime = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        new TeleportableJob
        {
            DeltaTime = deltaTime,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

public partial struct TeleportableJob : IJobEntity
{
    public float DeltaTime;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(Entity entity, RefRW<LocalTransform> transform, RefRW<Teleportable> teleportable, [EntityIndexInQuery] int sortKey)
    {
        new TeleportableAspect(entity, transform, teleportable).Teleport(DeltaTime, sortKey, ECB);
    }
}

