using Unity.Burst;
using Unity.Entities;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(MovableSystem))]
[UpdateBefore(typeof(SpriteAnimationSystem))]
public partial struct RotateAnimatorSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RotateAnimator>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var timeDelta = SystemAPI.Time.DeltaTime;
        new RotateAnimatorJob
        {
            TimeDelta = timeDelta
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

public partial struct RotateAnimatorJob : IJobEntity
{
    public float TimeDelta;
    private void Execute(RotateAnimatorAspect animator)
    {
        animator.UpdateAnimation(TimeDelta);
    }
}

