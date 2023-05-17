using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Player : IComponentData
{
    public float Lives;
    public int Score;
}

public struct PlayerAddScoreBufferElement : IBufferElementData
{
    public float2 MapPos;
    public int Score;
    public bool ScoreAnimation;
}

public readonly partial struct PlayerAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<LocalTransform> m_transform;
    private readonly RefRW<Movable> m_movable;
    private readonly RefRW<Player> m_player;
    private readonly DynamicBuffer<PlayerAddScoreBufferElement> m_addScoreBuffer;

    public readonly float3 GetWorldPos() => m_transform.ValueRO.Position;

    public void UpdateInput(float2 desiredDirection)
    {
        if (math.abs(desiredDirection.x) < 0.3f && math.abs(desiredDirection.y) < 0.3f)
        {
            return;
        }

        if (desiredDirection.x > 0)
        {
            m_movable.ValueRW.DesiredDir = Movable.Direction.Right;
        }
        else if (desiredDirection.y > 0)
        {
            m_movable.ValueRW.DesiredDir = Movable.Direction.Down;
        }
        else if (desiredDirection.x < 0)
        {
            m_movable.ValueRW.DesiredDir = Movable.Direction.Left;
        }
        else if (desiredDirection.y < 0)
        {
            m_movable.ValueRW.DesiredDir = Movable.Direction.Up;
        }
    }

    public void ApplyAddScore(Entity mainEntity, EntityCommandBuffer ecb)
    {
        bool scoreAdded = false;
        foreach (var score in m_addScoreBuffer)
        {
            m_player.ValueRW.Score += score.Score;

            if (score.ScoreAnimation)
            {
                // send score animation event!
            }
            scoreAdded = true;
        }

        if (scoreAdded)
        {
            var element = new SetScoreTextBufferElement()
            {
                Value = m_player.ValueRO.Score
            };
            ecb.AppendToBuffer(mainEntity, element);
        }

        m_addScoreBuffer.Clear();
    }
}
