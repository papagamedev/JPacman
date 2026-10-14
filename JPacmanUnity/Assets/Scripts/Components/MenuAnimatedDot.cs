using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct MenuAnimatedDot : IComponentData
{
    public int Idx;
}

public readonly struct MenuAnimatedDotAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRO<MenuAnimatedDot> m_animator;

    public MenuAnimatedDotAspect(Entity entity, RefRW<LocalTransform> transform, RefRO<MenuAnimatedDot> animator)
    {
        Entity = entity;
        m_transform = transform;
        m_animator = animator;
    }

    public void UpdateAnimation(float deltaTime, int shapeIdx, float2 shapePos, float dotSpeed, BlobAssetReference<MenuDotShapeConfigData> shapesBlob)
    {
        ref var introData = ref shapesBlob.Value;
        ref var shapeData = ref introData.ShapesData[shapeIdx];
        var targetPos = new float3(shapeData.DotPos[m_animator.ValueRO.Idx] + shapePos, 0);
        var pos = m_transform.ValueRO.Position;
        var dir = math.normalize(targetPos - pos);
        var dist = math.distance(pos, targetPos);
        float deltaPos = dotSpeed * deltaTime;
        if (dist < deltaPos)
        {
            pos = targetPos;
        }
        else
        {
            pos += dir * deltaPos;
        }
        m_transform.ValueRW.Position = pos;
    }

}
