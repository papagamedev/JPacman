using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Game : IComponentData
{
    public int Lives;
    public int Score;
    public int LevelId;
    public int CollectibleCount;
    public float LiveTime;
    public float LevelTime;
    public bool FruitSpawned;
    public bool DotsMoving;
    public bool PowerupsMoving;
    public int EnemyScore;
    public bool Paused;
}

public struct LevelStartPhaseTag : IComponentData { }

public struct LevelPlayingPhaseTag : IComponentData { }

public struct LevelDeadPhaseTag : IComponentData { }

public struct LevelWinPhaseTag : IComponentData { }

public struct LevelClearPhaseTag : IComponentData { }

public struct LevelResetLivePhaseTag : IComponentData { }

public struct LevelGameOverPhaseTag : IComponentData { }


public struct AddScoreBufferElement : IBufferElementData
{
    public float3 WorldPos;
    public int Score;
    public bool ScoreAnimation;
    public bool IsCollectible;
}

public readonly partial struct GameAspect : IAspect
{
    const int kNewLiveScore = 10000;


    public readonly Entity Entity;
    private readonly RefRW<Main> m_main;
    private readonly RefRW<Game> m_game;
    private readonly RefRW<PowerupMode> m_powerupMode;
    private readonly DynamicBuffer<AddScoreBufferElement> m_addScoreBuffer;

    public bool IsPaused => m_game.ValueRO.Paused;
    public void SetPaused(bool paused, Entity mainEntity, EntityCommandBuffer ecb)
    {
        m_game.ValueRW.Paused = paused;
        ecb.AppendToBuffer(mainEntity, new PauseAudioEventBufferElement()
        {
            Paused = paused
        });
        ecb.AppendToBuffer(mainEntity, new ShowUIBufferElement()
        {
            UI = paused ? HudEvents.ShowUIType.Paused : HudEvents.ShowUIType.Ingame 
        });
    }

    public void ApplyAddScore(Entity mainEntity, EntityCommandBuffer ecb)
    {
        bool isBonus = LevelData.BonusLevel;
        int scoreToAdd = 0;
        int scoreBefore = m_game.ValueRO.Score;
        foreach (var scoreItem in m_addScoreBuffer)
        {
            var score = scoreItem.Score;
            var hasAnimation = scoreItem.ScoreAnimation;
            if (scoreItem.IsCollectible)
            {
                m_game.ValueRW.CollectibleCount--;
                if (isBonus && m_game.ValueRO.CollectibleCount == 0)
                {
                    score = LevelData.FruitScore;
                    hasAnimation = true;
                }
            }

            scoreToAdd += score;
            ecb.AppendToBuffer(mainEntity, new SetScoreTextBufferElement()
            {
                Score = scoreBefore + scoreToAdd,
                DeltaScore = score,
                WorldPos = scoreItem.WorldPos,
                HasAnimation = hasAnimation
            });
        }
        m_addScoreBuffer.Clear();

        if (scoreToAdd > 0)
        {
            var newScore = scoreBefore + scoreToAdd;
            m_game.ValueRW.Score = newScore;

            var livesToAdd = (newScore / kNewLiveScore) - (scoreBefore / kNewLiveScore);
            if (livesToAdd > 0)
            {
                m_game.ValueRW.Lives += livesToAdd;
                ecb.AppendToBuffer(mainEntity, new SetLivesTextBufferElement()
                {
                    Value = m_game.ValueRO.Lives
                });
            }
        }
    }


    public bool IsLevelCompleted() => m_game.ValueRO.CollectibleCount <= 0;

    public ref LevelConfigData LevelData => ref m_main.ValueRO.LevelsConfigBlob.Value.LevelsData[m_game.ValueRO.LevelId];

    public int Score => m_game.ValueRO.Score;
    public int Lives => m_game.ValueRO.Lives;
    public float LiveTime => m_game.ValueRO.LiveTime;
    public uint RandomSeed => m_main.ValueRW.RandomSeed++;
    public int EnemyScore => m_game.ValueRO.EnemyScore;

    public void InitLive(EntityCommandBuffer ecb, Entity mainEntity, uint randSeed)
    {
        m_game.ValueRW.LiveTime = 0;
        m_game.ValueRW.DotsMoving = false;
        m_game.ValueRW.PowerupsMoving = false;

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

    public void UpdatePlayingTime(float deltaTime)
    {
        m_game.ValueRW.LiveTime += deltaTime;
        m_game.ValueRW.LevelTime += deltaTime;
    }

    public int RemoveLive()
    {
        m_game.ValueRW.Lives--;
        return m_game.ValueRO.Lives;
    }

    public void CheckSpawnFruit(Entity mainEntity, EntityCommandBuffer ecb)
    {
        if (m_game.ValueRO.FruitSpawned || m_game.ValueRO.LevelTime < LevelData.FruitWaitTime)
        {
            return;
        }
        m_game.ValueRW.FruitSpawned = true;

        ref var mapData = ref GetCurrentMapData();
        var fruit = ecb.Instantiate(m_main.ValueRO.FruitPrefab);
        ecb.SetComponent(fruit,
            new LocalTransform()
            {
                Position = mapData.MapToWorldPos(mapData.FruitPos),
                Scale = 1.0f,
                Rotation = quaternion.identity
            });
        ecb.AddComponent(fruit,
            new Fruit()
            {
                Duration = LevelData.FruitDuration
            });
        ecb.AddComponent(fruit,
            new SpriteSetFrame()
            {
                Frame = LevelData.FruitSpriteIdx
            });
    }

    public void CheckMoveDots(Entity mainEntity, EntityCommandBuffer ecb)
    {
        if (m_game.ValueRO.DotsMoving || LevelData.DotsMoveSpeed <= 0 || m_game.ValueRO.LiveTime < LevelData.DotsMoveWaitTime)
        {
            return;
        }
        m_game.ValueRW.DotsMoving = true;
        ecb.AddComponent(mainEntity,
            new DotsMovingTag()
            {
            });
    }

    public void CheckMovePowerups(Entity mainEntity, EntityCommandBuffer ecb)
    {
        if (m_game.ValueRO.PowerupsMoving || LevelData.PowerupsMoveSpeed <= 0 || m_game.ValueRO.LiveTime < LevelData.PowerupsMoveWaitTime)
        {
            return;
        }
        m_game.ValueRW.PowerupsMoving = true;
        ecb.AddComponent(mainEntity,
            new PowerupsMovingTag()
            {
            });
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
        m_game.ValueRW.LevelTime = 0;
        m_game.ValueRW.FruitSpawned = false;
        m_game.ValueRW.PowerupsMoving = false;
        m_game.ValueRW.CollectibleCount = 0;

        ref var mapData = ref GetCurrentMapData();
        bool isBonusLevel = LevelData.BonusLevel;
        for (var y = 0; y < mapData.Height; y++)
        {
            for (var x = 0; x < mapData.Width; x++)
            {
                if (!isBonusLevel && mapData.IsDot(x, y))
                {
                    CreateDot(ecb, ref mapData, x, y);
                }
                else if (mapData.IsWall(x, y))
                {
                    CreateWall(ecb, ref mapData, x, y);
                }
            }
        }

        var labelWorldPos = mapData.MapToWorldPos(mapData.LabelMessagePos);
        ecb.AppendToBuffer(mainEntity, new SetLabelPosBufferElement()
        {
            Value = labelWorldPos
        });
        ecb.AppendToBuffer(mainEntity, new SetLevelIconBufferElement()
        {
            IconIdx = LevelData.Idx
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
                    SpeedInTunnel = LevelData.PlayerSpeed,
                    AllowChangeDirInMidCell = true,
                    Rand = new Random(randSeed),
                    CanDoTeleporting = true
                });
        ecb.AddComponent(player, new SpriteAnimatedMovableTag() { });
        ecb.AddComponent(player,
            new SpriteSetOpacity()
            {
                Opacity = 1.0f
            });
    }

    private void CreateWall(EntityCommandBuffer ecb, ref MapConfigData mapData, int x, int y)
    {
        int frame = 0;
        if (mapData.IsEnemyExit(x, y))
        {
            // last frame of this image stores the enemy exit sprite
            frame = 15;
        }
        else
        {
            if (x == mapData.Width - 1 || mapData.IsWall(x + 1, y))
            {
                frame |= 1 << Direction.Right;
            }
            if (x == 0 || mapData.IsWall(x - 1, y))
            {
                frame |= 1 << Direction.Left;
            }
            if (y == mapData.Height - 1 || mapData.IsWall(x, y + 1))
            {
                frame |= 1 << Direction.Down;
            }
            if (y == 0 || mapData.IsWall(x, y - 1))
            {
                frame |= 1 << Direction.Up;
            }
            if (frame == 15)
            {
                // means an empty sprite, this is a wall surrounded by walls, skip creating a sprite
                return;
            }
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
    }

    private void CreateDot(EntityCommandBuffer ecb, ref MapConfigData mapData, int x, int y)
    {
        var dot = ecb.Instantiate(m_main.ValueRO.DotPrefab);
        ecb.SetComponent(dot,
            new LocalTransform()
            {
                Position = mapData.MapToWorldPos(x, y),
                Scale = 1.0f,
                Rotation = quaternion.identity
            });
        ecb.AddComponent(dot, new DotTag());
        m_game.ValueRW.CollectibleCount++;
    }

    private void CreateEnemyHorizontalHome(EntityCommandBuffer ecb, ref MapConfigData mapData, int x, int y, uint randSeed)
    {
        for (var i = 0; i < 4; i++)
        {
            CreateEnemy(ecb, ref mapData, x + i * 2.5f - 3.75f, y, i < 2 ? Direction.Right : Direction.Left, i, randSeed++);
        }
    }

    private void CreateEnemyVerticalHome(EntityCommandBuffer ecb, ref MapConfigData mapData, int x, int y, uint randSeed)
    {
        for (var i = 0; i < 4; i++)
        {
            CreateEnemy(ecb, ref mapData, x, y + i * 2.5f - 3.75f, i < 2 ? Direction.Down : Direction.Up, i, randSeed++);
        }
    }

    private void CreateEnemy(EntityCommandBuffer ecb, ref MapConfigData mapData, float x, float y, Direction direction, int id, uint randSeed)
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
                SpeedInTunnel = LevelData.EnemySpeedInTunnel,
                AllowChangeDirInMidCell = false,
                CurrentDir = direction,
                DesiredDir = Direction.None,
                Rand = new Random(randSeed),
                CanDoTeleporting = true
            });
        ecb.AddComponent(enemy, new SpriteAnimatedMovableTag() { });
        ecb.AddComponent(enemy,
            new Enemy()
            {
                Id = id
            });
        ecb.AddComponent(enemy,
            new SpriteSetOpacity()
            {
                Opacity = 1.0f
            });
        ecb.AddComponent(enemy, new EnemyHomeTag() { });
    }

    private void CreatePowerups(EntityCommandBuffer ecb, ref MapConfigData mapData)
    {
        for (int i = 0; i < 4; i++)
        {
            var powerup = ecb.Instantiate(m_main.ValueRO.PowerupPrefab);
            ResetPowerupPos(i, powerup, ecb, ref mapData);
            ecb.AddComponent(powerup,
                new Powerup()
                {
                    Id = i
                });
            m_game.ValueRW.CollectibleCount++;
        }
    }

    public void ResetPowerupPos(int id, Entity powerupEntity, EntityCommandBuffer ecb, ref MapConfigData mapData)
    {
        ecb.SetComponent(powerupEntity,
                new LocalTransform()
                {
                    Position = mapData.MapToWorldPos(mapData.PowerupPos[id].x, mapData.PowerupPos[id].y),
                    Scale = 1.0f,
                    Rotation = quaternion.identity,
                });
    }

}
