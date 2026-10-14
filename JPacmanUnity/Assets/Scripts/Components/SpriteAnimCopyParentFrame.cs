using Unity.Entities;
using Unity.Transforms;

public struct SpriteAnimCopyParentFrame : IComponentData { }

public readonly struct SpriteAnimCopyParentFrameAspect
{
    public readonly Entity Entity;
    private readonly RefRW<SpriteAnimator> m_animator;
    private readonly RefRO<Parent> m_parent;

    public SpriteAnimCopyParentFrameAspect(Entity entity, RefRW<SpriteAnimator> animator, RefRO<Parent> parent)
    {
        Entity = entity;
        m_animator = animator;
        m_parent = parent;
    }

    public void UpdateAnimFrame(EntityManager manager)
    {
        var parentAnimator = manager.GetComponentData<SpriteAnimator>(m_parent.ValueRO.Value);
        m_animator.ValueRW.Frame = parentAnimator.Frame;
    }
}
