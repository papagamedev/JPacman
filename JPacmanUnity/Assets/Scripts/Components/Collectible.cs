using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Collectible : IComponentData
{
    public enum EType
    {
        Dot,
        Powerup,
        Fruit
    };

    public int Score;
    public bool ScoreAnimation;
    public AudioEvents.SoundType SoundType;
    public EType Type;
}

public readonly partial struct CollectibleAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRO<LocalTransform> m_transform;
    private readonly RefRO<Collectible> m_collectible;
    private readonly RefRO<CollisionCircle> m_collision;

    public void CheckPlayer(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, float2 playerMapPos, float playerCollisionRadius, int fruitScore, int sortKey, Entity main, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];
        var collectibleWorldPos = m_transform.ValueRO.Position;
        var collectibleMapPos = mapData.WorldToMapPos(collectibleWorldPos);

        if (!CollisionCircle.CheckCollision(collectibleMapPos, playerMapPos, playerCollisionRadius + m_collision.ValueRO.Radius))
        {
            return;
        }

        ecb.DestroyEntity(sortKey, Entity);

        var score = m_collectible.ValueRO.Score;
        var registerAsCollectible = true;

        switch (m_collectible.ValueRO.Type)
        {
            case Collectible.EType.Powerup:
                ecb.AppendToBuffer(sortKey, main, new PowerupCollectedBufferElement()
                {
                    CollectedPos = collectibleMapPos
                });
                break;
            case Collectible.EType.Fruit:
                score = fruitScore;
                registerAsCollectible = false;
                break;
        }

        ecb.AppendToBuffer(sortKey, main, new AddScoreBufferElement()
        {
            WorldPos = collectibleWorldPos,
            Score = score,
            ScoreAnimation = m_collectible.ValueRO.ScoreAnimation,
            IsCollectible = registerAsCollectible
        });

        ecb.AppendToBuffer(sortKey, main, new SoundEventBufferElement()
        {
            SoundType = m_collectible.ValueRO.SoundType
        });

    }
}