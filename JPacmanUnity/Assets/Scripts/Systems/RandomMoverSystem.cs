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
    private void Execute(RandomMoverAspect mover, [EntityIndexInQuery] int sortKey)
    {
        mover.Update();
    }
}

