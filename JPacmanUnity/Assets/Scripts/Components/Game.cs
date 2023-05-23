using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;

public struct Game : IComponentData
{
    public int Lives;
    public int Score;
    public int LevelId;
    public int CollectibleCount;
    public float LiveTime;
    public bool EnemyScaredBlinking;
}

public struct EnemyScaredPhaseTag : IComponentData { }

public struct LevelStartPhaseTag : IComponentData { }

public struct LevelPlayingPhaseTag : IComponentData { }

public struct LevelDeadPhaseTag : IComponentData { }

public struct LevelWinPhaseTag : IComponentData { }

public struct LevelClearPhaseTag : IComponentData { }

public struct LevelResetLivePhaseTag : IComponentData { }

public struct LevelGameOverPhaseTag : IComponentData { }

public readonly partial struct GameAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<Main> m_main;
    private readonly RefRW<Game> m_game;
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

    public int Score => m_game.ValueRO.Score;
    public int Lives => m_game.ValueRO.Lives;
    public float LiveTime => m_game.ValueRO.LiveTime;
    public uint RandomSeed => m_main.ValueRW.RandomSeed++;
    public void SetEnemyScaredBlinking(bool blinking) => m_game.ValueRW.EnemyScaredBlinking = blinking;
    public bool IsEnemyScaredBlinking => m_game.ValueRO.EnemyScaredBlinking;

    public void InitLive(EntityCommandBuffer ecb, Entity mainEntity, uint randSeed)
    {
        ref var mapData = ref GetCurrentMapData();
        int homeX = (int)mapData.EnemyHousePos.x;
        int homeY = (int)mapData.EnemyHousePos.y;
        if (mapData.IsEnemyHorizontalHome(homeX, homeY))
        {
            CreateEnemyHorizontalHome(ecb, ref mapData, homeX, homeY, randSeed++);
        }
        else if (mapData.IsEnemyVerticalHome(homeX, homeY))
        {
            CreateEnemyVerticalHome(ecb, ref mapData, homeX, homeY, randSeed++);
        }
        CreatePlayer(ecb, ref mapData, randSeed++);
    }

    public void StartLive()
    {
        m_game.ValueRW.LiveTime = 0;
    }

    public void UpdateLive(float deltaTime) => m_game.ValueRW.LiveTime += deltaTime;

    public int RemoveLive()
    {
        m_game.ValueRW.Lives--;
        return m_game.ValueRO.Lives;
    }

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

    public void CreateLevel(EntityCommandBuffer ecb, Entity mainEntity, uint randSeed)
    {
        var collectibleCount = 0;
        ref var mapData = ref GetCurrentMapData();
        bool isBonusLevel = LevelData.BonusLevel;
        for (var y = 0; y < mapData.Height; y++)
        {
            for (var x = 0; x < mapData.Width; x++)
            {
                if (!isBonusLevel && mapData.IsDot(x, y))
                {
                    CreateDot(ecb, ref collectibleCount, ref mapData, x, y);
                }
                else if (mapData.IsWall(x, y))
                {
                    CreateWall(ecb, ref mapData, x, y);
                }
            }
        }
        m_game.ValueRW.CollectibleCount = collectibleCount;

        var labelWorldPos = mapData.MapToWorldPos(mapData.LabelMessagePos);
        ecb.AppendToBuffer(mainEntity, new SetLabelPosBufferElement()
        {
            Value = labelWorldPos
        });

        CreatePowerups(ecb, ref mapData);

        InitLive(ecb, mainEntity, randSeed++);
    }

    private void CreatePlayer(EntityCommandBuffer ecb, ref MapConfigData mapData, uint randSeed)
    {
        var player = ecb.Instantiate(m_main.ValueRO.PlayerPrefab);
        ecb.SetComponent(player,
            new LocalTransform()
            {
                Position = mapData.MapToWorldPos(mapData.PlayerPos),
                Scale = 1.0f,
                Rotation = quaternion.identity
            });
        ecb.AddComponent(player,
                new Movable()
                {
                    Speed = LevelData.PlayerSpeed,
                    AllowChangeDirInMidCell = true,
                    Rand = new Random(randSeed)
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
        ecb.RemoveComponent<SpriteAnimator>(wall);

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

    private void CreateEnemyHorizontalHome(EntityCommandBuffer ecb, ref MapConfigData mapData, int x, int y, uint randSeed)
    {
        for (var i = 0; i < 4; i++)
        {
            CreateEnemy(ecb, ref mapData, x + i * 2.5f - 3.75f, y, i < 2 ? Movable.Direction.Right : Movable.Direction.Left, i, randSeed++);
        }
    }

    private void CreateEnemyVerticalHome(EntityCommandBuffer ecb, ref MapConfigData mapData, int x, int y, uint randSeed)
    {
        for (var i = 0; i < 4; i++)
        {
            CreateEnemy(ecb, ref mapData, x, y + i * 2.5f - 3.75f, i < 2 ? Movable.Direction.Down : Movable.Direction.Up, i, randSeed++);
        }
    }

    private void CreateEnemy(EntityCommandBuffer ecb, ref MapConfigData mapData, float x, float y, Movable.Direction direction, int id, uint randSeed)
    {
        var enemy = ecb.Instantiate(m_main.ValueRO.EnemyPrefab);
        ecb.SetComponent(enemy,
            new LocalTransform()
            {
                Position = mapData.MapToWorldPos(x, y),
                Scale = 1.0f,
                Rotation = quaternion.identity,
            });
        ecb.AddComponent(enemy,
            new Movable()
            {
                Speed = LevelData.EnemySpeed,
                AllowChangeDirInMidCell = false,
                CurrentDir = direction,
                DesiredDir = Movable.Direction.None,
                Rand = new Random(randSeed)
            });
        ecb.AddComponent(enemy,
            new Enemy()
            {
                Id = id,
            });
        ecb.AddComponent(enemy, new EnemyHomeTag() { });
    }

    private void CreatePowerups(EntityCommandBuffer ecb, ref MapConfigData mapData)
    {
        for (int i = 0; i < 4; i++)
        {
            var powerup = ecb.Instantiate(m_main.ValueRO.PowerupPrefab);
            ecb.SetComponent(powerup,
                new LocalTransform()
                {
                    Position = mapData.MapToWorldPos(mapData.PowerupPos[i].x, mapData.PowerupPos[i].y),
                    Scale = 1.0f,
                    Rotation = quaternion.identity,
                });
        }
    }
}
