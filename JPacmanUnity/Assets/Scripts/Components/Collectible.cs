using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Collectible : IComponentData
{
    public int Score;
    public bool ScoreAnimation;
    public AudioEvents.SoundType SoundType; 
}

public readonly partial struct CollectibleAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRO<LocalTransform> m_transform;
    private readonly RefRO<Collectible> m_collectible;

    public void CheckPlayer(BlobAssetReference<MapsConfigData> mapsBlobRef, int mapId, float2 playerMapPos, int sortKey, Entity main, EntityCommandBuffer.ParallelWriter ecb)
    {
        ref var mapData = ref mapsBlobRef.Value.MapsData[mapId];
        var collectibleWorldPos = m_transform.ValueRO.Position;
        var collectibleMapPos = mapData.WorldToMapPos(collectibleWorldPos);
        
        if (mapData.CheckCollision(collectibleMapPos,playerMapPos))
        {
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
        }
    }
}