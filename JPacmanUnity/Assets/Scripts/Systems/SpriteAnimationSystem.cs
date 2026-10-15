using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(MovableSystem))]
public partial struct SpriteAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpriteFrames>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new SpriteAnimatorJob
        {
            Time = (float)SystemAPI.Time.ElapsedTime
        }.ScheduleParallel();

        // SystemAPI.Query doesn't wait for jobs this system just scheduled (Entities.ForEach.Run used to)
        state.CompleteDependency();

        foreach (var (animator, parent, entity) in SystemAPI.Query<RefRW<SpriteAnimator>, RefRO<Parent>>().WithAll<SpriteAnimCopyParentFrame>().WithEntityAccess())
        {
            new SpriteAnimCopyParentFrameAspect(entity, animator, parent).UpdateAnimFrame(state.EntityManager);
        }

        new SpriteSetFrameJob().ScheduleParallel();
        new SpriteAnimatorFrameJob().ScheduleParallel();
    }
}

[BurstCompile]
public partial struct SpriteAnimatorJob : IJobEntity
{
    public float Time;

    private void Execute(Entity entity, RefRW<SpriteAnimator> animator)
    {
        new SpriteAnimatorAspect(entity, animator).UpdateAnimation(Time);
    }
}

// Frames set by gameplay code (walls, fruits) only change when the component does
[BurstCompile]
[WithNone(typeof(SpriteAnimator))]
[WithChangeFilter(typeof(SpriteSetFrame))]
public partial struct SpriteSetFrameJob : IJobEntity
{
    private void Execute(in SpriteSetFrame set, in SpriteFrames frames, ref SpriteUVProperty uv, ref SpriteQuadProperty quad)
    {
        ref var frame = ref frames.Blob.Value.Frames[set.Frame];
        uv.Value = frame.UVRect;
        quad.Value = frame.QuadRect;
    }
}

[BurstCompile]
public partial struct SpriteAnimatorFrameJob : IJobEntity
{
    private void Execute(in SpriteAnimator animator, in SpriteFrames frames, ref SpriteUVProperty uv, ref SpriteQuadProperty quad)
    {
        ref var frame = ref frames.Blob.Value.Frames[animator.Frame];
        uv.Value = frame.UVRect;
        quad.Value = frame.QuadRect;
    }
}
