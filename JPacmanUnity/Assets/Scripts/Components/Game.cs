using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.Experimental.GraphView;
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
        Debug.Log("Width " + mapData.Width);
        for (var y = 0; y < mapData.Height; y++)
        {
            for (var x = 0; x < mapData.Width; x++)
            {
                if (mapData.IsDot(x, y))
                {
                    CreateDot(ecb, ref collectibleCount, ref mapData, x, y);
                }
                else if (mapData.IsEnemyHorizontalHome(x, y))
                {
                    CreateEnemyHorizontalHome(ecb, ref mapData, x, y);
                }
                else if (mapData.IsWall(x,y))
                {
                    CreateWall(ecb, ref mapData, x, y);
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

    private void CreateWall(EntityCommandBuffer ecb, ref MapConfigData mapData, int x, int y)
    {
        int frame = 0;
        if (x == mapData.Width - 1 || mapData.IsWall(x + 1, y))
        {
            frame |= 1 << (int) Movable.Direction.Right;
        }
        if (x == 0 || mapData.IsWall(x - 1, y))
        {
            frame |= 1 << (int)Movable.Direction.Left;
        }
        if (y == mapData.Height - 1 || mapData.IsWall(x, y + 1))
        {
            frame |= 1 << (int)Movable.Direction.Down;
        }
        if (y == 0 || mapData.IsWall(x, y - 1))
        {
            frame |= 1 << (int)Movable.Direction.Up;
        }
        if (frame == 15)
        {
            // means an empty sprite, this is a wall surrounded by walls
            return;
        }

        var wall = ecb.Instantiate(m_main.ValueRO.WallPrefab);
        ecb.SetComponent(wall,
            new LocalTransform()
            {
                Position = mapData.MapToWorldPos(x + 0.5f, y + 0.5f),
                Scale = 1.05f,
                Rotation = quaternion.identity
            });
        ecb.AddComponent(wall,
            new SpriteSetFrame()
            {
                Frame = frame
            });
        ecb.RemoveComponent(wall, typeof(SpriteAnimator));
    }

    private void CreateDot(EntityCommandBuffer ecb, ref int collectibleCount, ref MapConfigData mapData, int x, int y)
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

    private void CreateEnemyHorizontalHome(EntityCommandBuffer ecb, ref MapConfigData mapData, int x, int y)
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


