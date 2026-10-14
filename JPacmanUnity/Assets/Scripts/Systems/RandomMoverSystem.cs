using Unity.Burst;
using Unity.Entities;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollectibleSystem))]
public partial struct RandomMoverSystem: ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<RandomMover>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new RandomMoverJob
        {
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

public partial struct RandomMoverJob : IJobEntity
{
    private void Execute(Entity entity, RefRW<Movable> movable, RefRO<RandomMover> randomMover, [EntityIndexInQuery] int sortKey)
    {
        new RandomMoverAspect(entity, movable, randomMover).Update();
    }
}

