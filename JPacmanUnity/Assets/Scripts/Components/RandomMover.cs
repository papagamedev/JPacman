using System.Diagnostics;
using Unity.Entities;
using Unity.Mathematics;

public struct RandomMover : IComponentData
{
    public float2 RandomMapPos;
}

public readonly partial struct RandomMoverAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<RandomMover> m_randomMover;

    public void Update()
    {
        MovableAspect.UpdateMovableFollowPos(m_movable, m_randomMover.ValueRO.RandomMapPos, 1);
    }

}
