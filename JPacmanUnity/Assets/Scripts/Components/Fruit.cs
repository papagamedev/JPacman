using Unity.Entities;

public struct Fruit : IComponentData
{
    public float Duration;
}

public readonly partial struct FruitAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<Fruit> m_fruit;

    public void CheckTime(float deltaTime, int sortKey, EntityCommandBuffer.ParallelWriter ecb)
    {
        m_fruit.ValueRW.Duration -= deltaTime;
        if (m_fruit.ValueRO.Duration <= 0)
        {
            ecb.DestroyEntity(sortKey, Entity);
        }
    }
}