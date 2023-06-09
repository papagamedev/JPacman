using Unity.Entities;
using Unity.Transforms;

public struct Dot : IComponentData
{
    public int Generation;
}

public struct DotsMovingTag : IComponentData
{
}

public readonly partial struct DotCloningAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRO<Dot> m_dot;
    private readonly RefRO<Collectible> m_collectible;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<LocalTransform> m_transform;

    public void CheckClone(int remainingCollectibles, int totalCollectibles, float dotsCloneFactor, Entity mainEntity, int sortKey, EntityCommandBuffer.ParallelWriter ecb)
    {
        if (m_movable.ValueRO.JustTeleported)
        {
            return;
        }
        var limit = (int)(remainingCollectibles * totalCollectibles / dotsCloneFactor);
        var r = m_movable.ValueRW.Rand.NextInt(limit);
        if (r > 0)
        {
            return;
        }

        var generation = m_dot.ValueRO.Generation + 1;
        ecb.AppendToBuffer(sortKey, mainEntity, new DotCloneBufferElement()
        {
            WorldPos = m_transform.ValueRO.Position,
            Direction = m_movable.ValueRO.CurrentDir.Opposite(),
            Generation =  generation,
            BaseScore = m_collectible.ValueRO.Score / generation,
        });
    }
}
