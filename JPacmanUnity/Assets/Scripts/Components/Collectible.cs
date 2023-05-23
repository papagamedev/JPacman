using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Collectible : IComponentData
{
    public int Score;
    public bool ScoreAnimation;
    public AudioEvents.SoundType SoundType;
    public bool IsPowerup;
}

public readonly partial struct CollectibleAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRO<LocalTransform> m_transform;
    private readonly RefRO<Collectible> m_collectible;
    private readonly RefRO<CollisionCircle> m_collision;

    public void CheckPlayer(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, float2 playerMapPos, float playerCollisionRadius, int sortKey, Entity main, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];
        var collectibleWorldPos = m_transform.ValueRO.Position;
        var collectibleMapPos = mapData.WorldToMapPos(collectibleWorldPos);

        if (!CollisionCircle.CheckCollision(collectibleMapPos, playerMapPos, playerCollisionRadius + m_collision.ValueRO.Radius))
        {
            return;
        }

        ecb.DestroyEntity(sortKey, Entity);

        var scoreBufferElement = new AddScoreBufferElement()
        {
            MapPos = collectibleMapPos,
            Score = m_collectible.ValueRO.Score,
            ScoreAnimation = m_collectible.ValueRO.ScoreAnimation,
            IsCollectible = true
        };
        ecb.AppendToBuffer(sortKey, main, scoreBufferElement);

        var soundEventBufferElement = new SoundEventBufferElement()
        {
            SoundType = m_collectible.ValueRO.SoundType
        };
        ecb.AppendToBuffer(sortKey, main, soundEventBufferElement);

        if (m_collectible.ValueRO.IsPowerup)
        {
            ecb.AddComponent(sortKey, main, new EnemyScaredPhaseTag());
        }
    }
}