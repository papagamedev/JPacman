using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollectibleSystem))]
public partial struct DotsMovingSystem: ISystem , ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<DotsMovingTag>();
        state.RequireForUpdate<Game>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {


        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        if (gameAspect.IsPaused)
        {
            return;
        }

        var dotsRemainingCloneThreshold = gameAspect.LevelData.DotsRemainingCloneThreshold;
        var dotsCloneFactor = gameAspect.LevelData.DotsCloneFactor;
        if (dotsCloneFactor > 0 && dotsRemainingCloneThreshold > 0)
        {
            var remainingCollectibles = gameAspect.RemainingCollectibles;
            var totalCollectibles = gameAspect.TotalCollectibles;

            if (remainingCollectibles < totalCollectibles * dotsRemainingCloneThreshold)
            {
                var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
                new DotCloneJob()
                {
                    RemainingCollectibles = remainingCollectibles,
                    TotalCollectibles = totalCollectibles,
                    DotsCloneFactor = dotsCloneFactor,
                    MainEntity = mainEntity,
                    ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
                }.ScheduleParallel();
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        ref var map = ref gameAspect.GetCurrentMapData();
        foreach (var (_, entity) in SystemAPI.Query<Dot>().WithEntityAccess())
        {
            gameAspect.SetDotMovable(entity, Direction.None, ref map, ecb);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (_, entity) in SystemAPI.Query<Dot>().WithEntityAccess())
        {
            ecb.RemoveComponent<Movable>(entity);
            ecb.RemoveComponent<RotateAnimator>(entity);
            ecb.RemoveComponent<RandomMover>(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public partial struct DotCloneJob : IJobEntity
{
    public int RemainingCollectibles;
    public int TotalCollectibles;
    public float DotsCloneFactor;
    public Entity MainEntity;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(DotCloningAspect dot, [EntityIndexInQuery] int sortKey)
    {
        dot.CheckClone(RemainingCollectibles, TotalCollectibles, DotsCloneFactor, MainEntity, sortKey, ECB);
    }
}