using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Player : IComponentData
{
    public float Lives;
}

public readonly partial struct PlayerAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRO<Player> m_player;

    public readonly float3 GetWorldPos() => m_transform.ValueRO.Position;

    public void UpdateInput(float2 desiredDirection)
    {
        m_movable.ValueRW.Direction = desiredDirection;
    }

    public void CheckCollectible(CollectibleAspect collectible, ref MapConfigData map, float deltaTime)
    {

    }
}
