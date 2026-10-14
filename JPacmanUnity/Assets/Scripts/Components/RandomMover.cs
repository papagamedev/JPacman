using System.Diagnostics;
using Unity.Entities;
using Unity.Mathematics;

public struct RandomMover : IComponentData
{
    public float2 RandomMapPos;
}

public readonly struct RandomMoverAspect
{
    public readonly Entity Entity;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<RandomMover> m_randomMover;

    public RandomMoverAspect(Entity entity, RefRW<Movable> movable, RefRO<RandomMover> randomMover)
    {
        Entity = entity;
        m_movable = movable;
        m_randomMover = randomMover;
    }

    public void Update()
    {
        MovableAspect.UpdateMovableFollowPos(m_movable, m_randomMover.ValueRO.RandomMapPos, 1);
    }

}
