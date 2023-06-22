using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct IntroDotAnimator : IComponentData
{
    public int Idx;
}

public readonly partial struct IntroDotAnimatorAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRO<IntroDotAnimator> m_animator;

    public void UpdateAnimation(float deltaTime, int shapeIdx, BlobAssetReference<IntroConfigData> introBlob)
    {
        ref var introData = ref introBlob.Value;
        ref var shapeData = ref introData.ShapeData[shapeIdx];
        var speed = introData.DotSpeed;
        var targetPos = new float3(shapeData.DotPos[m_animator.ValueRO.Idx], 0);
        var pos = m_transform.ValueRO.Position;
        var dir = math.normalize(targetPos - pos);
        var dist = math.distance(pos, targetPos);
        if (dist < speed * 0.1)
        {
            pos = targetPos;
        }
        else
        {
            pos += dir * speed * deltaTime;
        }
        m_transform.ValueRW.Position = pos;
    }

}
