using Unity.Burst;
using Unity.Entities;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(MovableSystem))]
[UpdateBefore(typeof(SpriteAnimationSystem))]
public partial struct SpriteAnimatedMovableSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Movable>();
        state.RequireForUpdate<SpriteAnimator>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new SpriteAnimatedMoveEntityJob
        {
            
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

[WithAll(typeof(SpriteAnimatedMovableTag))]
public partial struct SpriteAnimatedMoveEntityJob : IJobEntity
{
    private void Execute(Entity entity, RefRO<Movable> movable, RefRW<SpriteAnimator> animator)
    {
        new SpriteAnimatedMovableAspect(entity, movable, animator).UpdateAnimation();
    }
}

