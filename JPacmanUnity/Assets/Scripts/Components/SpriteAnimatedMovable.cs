using Unity.Entities;

public struct SpriteAnimatedMovableTag : IComponentData
{
}


public readonly partial struct SpriteAnimatedMovableAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRO<Movable> m_movable;
    private readonly RefRW<SpriteAnimator> m_animator;
    private readonly RefRO<SpriteAnimatedMovableTag> m_tag;

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
