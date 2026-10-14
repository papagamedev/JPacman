using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(MovableSystem))]
public partial class SpriteAnimationSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate<SpriteAnimator>();
        RequireForUpdate<SpriteAnimatorDef>();
    }

    protected override void OnUpdate()
    {
        var time = SystemAPI.Time.ElapsedTime;
        new SpriteAnimatorJob
        {
            Time = (float)time
        }.ScheduleParallel();

        // SystemAPI.Query doesn't wait for jobs this system just scheduled (Entities.ForEach.Run used to)
        CompleteDependency();

        foreach (var (animator, parent, entity) in SystemAPI.Query<RefRW<SpriteAnimator>, RefRO<Parent>>().WithAll<SpriteAnimCopyParentFrame>().WithEntityAccess())
        {
            new SpriteAnimCopyParentFrameAspect(entity, animator, parent).UpdateAnimFrame(EntityManager);
        }

        foreach (var (sprite, spriteDef, set) in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<SpriteRenderer>, SpriteAnimatorDef, RefRO<SpriteSetFrame>>())
        {
            sprite.Value.sprite = spriteDef.AnimationFrames[set.ValueRO.Frame];
        }

        foreach (var (sprite, spriteDef, spriteAnimator) in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<SpriteRenderer>, SpriteAnimatorDef, RefRO<SpriteAnimator>>())
        {
            sprite.Value.sprite = spriteDef.AnimationFrames[spriteAnimator.ValueRO.Frame];
        }
    }
}

public partial struct SpriteAnimatorJob : IJobEntity
{
    public float Time;

    private void Execute(Entity entity, RefRW<SpriteAnimator> animator)
    {
        new SpriteAnimatorAspect(entity, animator).UpdateAnimation(Time);
    }
}
