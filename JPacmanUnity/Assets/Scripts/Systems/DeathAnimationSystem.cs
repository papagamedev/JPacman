using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(MovableSystem))]
public partial struct DeathAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DeathAnimation>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        new DeathAnimationJob
        {
            DeltaTime = deltaTime
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

public partial struct DeathAnimationJob : IJobEntity
{
    public float DeltaTime;

    private void Execute(Entity entity, RefRW<LocalTransform> transform, RefRO<DeathAnimation> death)
    {
        new DeathAnimationAspect(entity, transform, death).UpdateAnimation(DeltaTime);
    }
}

