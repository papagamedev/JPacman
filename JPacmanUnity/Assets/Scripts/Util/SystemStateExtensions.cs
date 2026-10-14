using Unity.Entities;

// Main-thread component access for helper structs that can't use SystemAPI (it only works inside system code).
// Mirrors what SystemAPI.GetComponentRW/RO does: complete pending jobs on the type, then return a ref via a lookup.
public static class SystemStateExtensions
{
    public static RefRW<T> GetComponentRW<T>(this ref SystemState state, Entity entity) where T : unmanaged, IComponentData
    {
        state.EntityManager.CompleteDependencyBeforeRW<T>();
        return state.GetComponentLookup<T>(false).GetRefRW(entity);
    }

    public static RefRO<T> GetComponentRO<T>(this ref SystemState state, Entity entity) where T : unmanaged, IComponentData
    {
        state.EntityManager.CompleteDependencyBeforeRO<T>();
        return state.GetComponentLookup<T>(true).GetRefRO(entity);
    }
}
