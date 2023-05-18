using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Game : IComponentData
{
    public int Lives;
    public int Score;
    public int LevelId;
    public int CollectibleCount;
}

public struct LevelStartPhaseTag : IComponentData { }

public struct LevelPlayingPhaseTag : IComponentData { }

public struct LevelDeadPhaseTag : IComponentData { }

public struct LevelWinPhaseTag : IComponentData { }

public struct LevelClearPhaseTag : IComponentData { }

public struct LevelGameOverPhaseTag : IComponentData { }

public struct SetLivesTextBufferElement : IBufferElementData
{
    public int Value;
}

public struct SetScoreTextBufferElement : IBufferElementData
{
    public int Value;
}

public struct SetLabelTextBufferElement : IBufferElementData
{
    public HudEvents.LabelMessage Value;
}

public struct StartScoreAnimationBufferElement : IBufferElementData
{
    public int Score;
    public float3 WorldPos;
}

public struct FadeAnimationBufferElement : IBufferElementData
{
    public bool IsFadeIn;
    public float Duration;
}

public struct AddScoreBufferElement : IBufferElementData
{
    public float2 MapPos;
    public int Score;
    public bool ScoreAnimation;
    public bool IsCollectible;
}

public readonly partial struct GameAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRO<Main> m_main;
    private readonly RefRW<Game> m_game;
    public readonly DynamicBuffer<SetLivesTextBufferElement> SetLivesTextBuffer;
    public readonly DynamicBuffer<SetScoreTextBufferElement> SetScoreTextBuffer;
    public readonly DynamicBuffer<SetLabelTextBufferElement> SetLabelTextBuffer;
    public readonly DynamicBuffer<StartScoreAnimationBufferElement> StartScoreAnimationBuffer;
    public readonly DynamicBuffer<FadeAnimationBufferElement> FadeAnimationBuffer;
    private readonly DynamicBuffer<AddScoreBufferElement> m_addScoreBuffer;

    public void ApplyAddScore(Entity mainEntity, EntityCommandBuffer ecb)
    {
        bool scoreAdded = false;
        foreach (var score in m_addScoreBuffer)
        {
            m_game.ValueRW.Score += score.Score;

            if (score.ScoreAnimation)
            {
                // send score animation event!
            }
            scoreAdded = true;

            if (score.IsCollectible)
            {
                m_game.ValueRW.CollectibleCount--;
            }
        }

        if (scoreAdded)
        {
            var element = new SetScoreTextBufferElement()
            {
                Value = m_game.ValueRO.Score
            };
            ecb.AppendToBuffer(mainEntity, element);
        }

        m_addScoreBuffer.Clear();
    }

    public bool IsLevelCompleted() => m_game.ValueRO.CollectibleCount <= 0;

    public ref LevelConfigData LevelData => ref m_main.ValueRO.LevelsConfigBlob.Value.LevelsData[m_game.ValueRO.LevelId];

    public int Lives => m_game.ValueRO.Lives;

    public void SetNextLevel()
    {
        var levelId = m_game.ValueRO.LevelId;
        var levelsCount = m_main.ValueRO.LevelsConfigBlob.Value.LevelsData.Length;
        if (levelId < levelsCount - 1)
        {
            m_game.ValueRW.LevelId++;
        }
    }

    public ref MapConfigData GetCurrentMapData()
    {
        ref var levelData = ref LevelData;
        var mapId = levelData.MapId;
        ref var mapData = ref m_main.ValueRO.MapsConfigBlob.Value.MapsData[mapId];
        return ref mapData;
    }

    public void CreateLevel(EntityCommandBuffer ecb)
    {
        var collectibleCount = 0;
        ref var mapData = ref GetCurrentMapData();
        for (var y = 0; y < mapData.Height; y++)
        {
            for (var x = 0; x < mapData.Width; x++)
            {
                if (mapData.IsDot(x, y))
                {
                    var dot = ecb.Instantiate(m_main.ValueRO.DotPrefab);
                    ecb.SetComponent(dot,
                        new LocalTransform()
                        {
                            Position = mapData.MapToWorldPos(x, y),
                            Scale = 1.0f,
                            Rotation = quaternion.identity
                        });
                    collectibleCount++;
                }
                else if (mapData.IsGhostsHorizontalHome(x, y))
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var enemy = ecb.Instantiate(m_main.ValueRO.EnemyPrefab);
                        ecb.SetComponent(enemy,
                            new LocalTransform()
                            {
                                Position = mapData.MapToWorldPos(x + i * 2.5f - 3.75f, y),
                                Scale = 1.0f,
                                Rotation = quaternion.identity,
                            });
                        ecb.AddComponent(enemy,
                            new Enemy()
                            {
                                Id = i,
                                Scared = false
                            });
                    }
                }
            }
        }
        m_game.ValueRW.CollectibleCount = collectibleCount;

        var player = ecb.Instantiate(m_main.ValueRO.PlayerPrefab);
        ecb.SetComponent(player,
            new LocalTransform()
            {
                Position = mapData.MapToWorldPos(mapData.PlayerPos),
                Scale = 1.0f,
                Rotation = quaternion.identity
            });
    }
}


