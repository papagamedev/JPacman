using Unity.Burst;
using Unity.Entities;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(RotateAnimatorSystem))]
[UpdateBefore(typeof(SpriteAnimationSystem))]
public partial struct MoveAnimatorSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MoveAnimator>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var timeDelta = SystemAPI.Time.DeltaTime;
        new MoveAnimatorJob
        {
            TimeDelta = timeDelta
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

public partial struct MoveAnimatorJob : IJobEntity
{
    public float TimeDelta;
    private void Execute(MoveAnimatorAspect animator)
    {
        animator.UpdateAnimation(TimeDelta);
    }
}

