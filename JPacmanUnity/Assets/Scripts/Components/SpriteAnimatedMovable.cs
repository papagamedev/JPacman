using Unity.Entities;

public struct SpriteAnimatedMovableTag : IComponentData
{
}


public readonly struct SpriteAnimatedMovableAspect
{
    public readonly Entity Entity;
    private readonly RefRO<Movable> m_movable;
    private readonly RefRW<SpriteAnimator> m_animator;

    public SpriteAnimatedMovableAspect(Entity entity, RefRO<Movable> movable, RefRW<SpriteAnimator> animator)
    {
        Entity = entity;
        m_movable = movable;
        m_animator = animator;
    }

    public void UpdateAnimation()
    {
        var currentDir = m_movable.ValueRO.CurrentDir;
        if (currentDir == Direction.None)
        {
            return;
        }
        int framesPerDirAnim = m_animator.ValueRO.FramesCount / 4;
        m_animator.ValueRW.StartFrame = framesPerDirAnim * (int)currentDir;
        m_animator.ValueRW.LastFrame = framesPerDirAnim * (int)currentDir + framesPerDirAnim - 1;
    }
}
