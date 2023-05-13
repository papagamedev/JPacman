using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Collectible : IComponentData
{
    public int Score;
}

public readonly partial struct CollectibleAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRO<LocalTransform> m_transform;
    private readonly RefRO<Collectible> m_movable;

    public void CheckPlayer(ref MapConfigData mapData, float2 playerMapPos, int sortKey, EntityCommandBuffer.ParallelWriter ecb)
    {
        var collectibleWorldPos = m_transform.ValueRO.Position;
        var collectibleMapPos = mapData.WorldToMapPos(collectibleWorldPos);
        
        if (math.abs(collectibleMapPos.x - playerMapPos.x) < 0.5f &&
            math.abs(collectibleMapPos.y - playerMapPos.y) < 0.5f)
        {
            ecb.DestroyEntity(sortKey, Entity);
        }
    }
}