using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MainAuthoring : MonoBehaviour
{
    public GameConfig Config;
    
    public class Baker : Baker<MainAuthoring>
    {
        public override void Bake(MainAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var maps = new List<MapConfig>();
            var levelsConfigBlob = CreateLevelsConfigBlob(authoring, maps);
            var mapsConfigBlob = CreateMapsConfigBlob(maps.ToArray());
            AddComponent(entity, new Main
            {
                DotPrefab = GetEntity(authoring.Config.DotPrefab, TransformUsageFlags.None),
                EnemyPrefab = GetEntity(authoring.Config.EnemyPrefab, TransformUsageFlags.None),
                PlayerPrefab = GetEntity(authoring.Config.PlayerPrefab, TransformUsageFlags.None),
                WallPrefab = GetEntity(authoring.Config.WallPrefab, TransformUsageFlags.None),
                FruitPrefab = GetEntity(authoring.Config.FruitPrefab, TransformUsageFlags.None),
                PowerupPrefab = GetEntity(authoring.Config.PowerupPrefab, TransformUsageFlags.None),
                LevelsConfigBlob = levelsConfigBlob,
                MapsConfigBlob = mapsConfigBlob,
                RandomSeed = (uint)(System.DateTime.Now.Ticks % 1000000000)
            });
            AddComponent(entity, new IntroPhaseTag());
            AddBuffer<SoundEventBufferElement>(entity);
            AddBuffer<SoundStopEventBufferElement>(entity);
            AddBuffer<MusicEventBufferElement>(entity);
            AddBuffer<SetLivesTextBufferElement>(entity);
            AddBuffer<SetScoreTextBufferElement>(entity);
            AddBuffer<SetLabelTextBufferElement>(entity);
            AddBuffer<SetLabelPosBufferElement>(entity);
            AddBuffer<SetLevelIconBufferElement>(entity);
            AddBuffer<KillAllScoreAnimationBufferElement>(entity);
            AddBuffer<FadeAnimationBufferElement>(entity);
            AddBuffer<ShowUIBufferElement>(entity);
            AddBuffer<AddScoreBufferElement>(entity);
            AddBuffer<PowerupCollectedBufferElement>(entity);
            AddBuffer<EnemyEatenBufferElement>(entity);
            AddBuffer<EnemyReturnedHomeBufferElement>(entity);
        }

        private BlobAssetReference<LevelsConfigData> CreateLevelsConfigBlob(MainAuthoring authoring, List<MapConfig> maps)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var levelsConfigData = ref builder.ConstructRoot<LevelsConfigData>();
            var levelsConfig = authoring.Config.LevelConfigs;
            var levelCount = levelsConfig.Length;
            var arrayBuilder = builder.Allocate(ref levelsConfigData.LevelsData, levelCount);
            var mapsDictionary = new Dictionary<MapConfig, int>();
            for (int i = 0; i < levelCount; i++)
            {
                arrayBuilder[i].Idx = i;
                arrayBuilder[i].LevelNumber = levelsConfig[i].LevelNumber;
                arrayBuilder[i].RoundNumber = levelsConfig[i].RoundNumber;
                arrayBuilder[i].BonusLevel = levelsConfig[i].BonusLevel;
                arrayBuilder[i].MoveDots = levelsConfig[i].MoveDots;
                arrayBuilder[i].MultiplyDots = levelsConfig[i].MultiplyDots;
                arrayBuilder[i].MovePowerups = levelsConfig[i].MovePowerups;
                arrayBuilder[i].PlayerSpeed = levelsConfig[i].PlayerSpeed;
                arrayBuilder[i].EnemySpeed = levelsConfig[i].EnemySpeed;
                arrayBuilder[i].EnemySpeedInTunnel = levelsConfig[i].EnemySpeedInTunnel;
                arrayBuilder[i].EnemySpeedScared = levelsConfig[i].EnemySpeedScared;
                arrayBuilder[i].EnemySpeedReturnHome = levelsConfig[i].EnemySpeedReturnHome;
                arrayBuilder[i].EnemyInHomeTime = levelsConfig[i].EnemyInHomeTime;
                arrayBuilder[i].EnemyScaredTime = levelsConfig[i].EnemyScaredTime;
                arrayBuilder[i].EnemyScore = levelsConfig[i].EnemyScore;
                arrayBuilder[i].EnemyCI = levelsConfig[i].EnemyCI;
                arrayBuilder[i].FruitScore = levelsConfig[i].FruitScore;
                arrayBuilder[i].FruitSpriteIdx = levelsConfig[i].FruitSpriteIdx;
                arrayBuilder[i].FruitWaitTime = levelsConfig[i].FruitWaitTime;
                arrayBuilder[i].FruitDuration = levelsConfig[i].FruitDuration;

                var map = levelsConfig[i].MapConfig;
                if (!mapsDictionary.TryGetValue(map, out var mapId))
                {
                    mapId = maps.Count;
                    mapsDictionary.Add(map, mapId);
                    maps.Add(levelsConfig[i].MapConfig);
                }
                arrayBuilder[i].MapId = mapId;
            }
            var result = builder.CreateBlobAssetReference<LevelsConfigData>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }

        private BlobAssetReference<MapsConfigData> CreateMapsConfigBlob(MapConfig[] maps)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var mapsConfigData = ref builder.ConstructRoot<MapsConfigData>();
            var mapsCount = maps.Length;
            var mapsArrayBuilder = builder.Allocate(ref mapsConfigData.MapsData, mapsCount);
            for (int i = 0;i < mapsCount; i++)
            {
                var map = maps[i].Map;
                var mapDataSize = map.m_height * map.m_width;
                mapsArrayBuilder[i].Id = i;
                mapsArrayBuilder[i].Width = map.m_width;
                mapsArrayBuilder[i].Height = map.m_height;
                var arrayBuilder = builder.Allocate(ref mapsArrayBuilder[i].MapData, mapDataSize);
                var enemyHousePos = half2.zero;
                var enemyExitPos = half2.zero;
                for (int y = 0; y < map.m_height; y++)
                {
                    for (int x = 0; x < map.m_width; x++)
                    {
                        var c = map.m_data[x, y];
                        arrayBuilder[y * map.m_width + x] = c;

                        if (c == MapConfigData.kPlayerChar)
                        {
                            mapsArrayBuilder[i].PlayerPos = new half2((half)x, (half)y);
                        }
                        else if (c == MapConfigData.kLabelsChar)
                        {
                            mapsArrayBuilder[i].LabelMessagePos = new half2((half)x, (half)y);
                        }
                        else if (c == MapConfigData.kFruitChar)
                        {
                            mapsArrayBuilder[i].FruitPos = new half2((half)x, (half)y);
                        }
                        else if (c == MapConfigData.kEnemyExitChar)
                        {
                            enemyExitPos = new half2((half)x, (half)y);
                        }
                        else if (c >= MapConfigData.kTunnelFirstChar && c<= MapConfigData.kTunnelLastChar)
                        {
                            int tunnelIdx = c - MapConfigData.kTunnelFirstChar;
                            var length = mapsArrayBuilder[i].TunnelPos.Length;
                            if (tunnelIdx >= length)
                            {
                                var addCount = tunnelIdx - length + 1;
                                mapsArrayBuilder[i].TunnelPos.AddReplicate(half2.zero, addCount);
                            }
                            mapsArrayBuilder[i].TunnelPos[tunnelIdx] = new half2((half)x, (half)y);
                        }
                        else if (c == MapConfigData.kEnemyHorizontalHomeChar ||
                            c == MapConfigData.kEnemyVerticalHomeChar)
                        {
                            enemyHousePos = new half2((half)x, (half)y);
                        }
                        else if (c == MapConfigData.kPowerupVerticalChar)
                        {
                            mapsArrayBuilder[i].PowerupPos.Add(new half2((half)x, (half)(y + 0.5f)));
                        }
                        else if (c == MapConfigData.kPowerupHorizontalChar)
                        {
                            mapsArrayBuilder[i].PowerupPos.Add(new half2((half)(x + 0.5f), (half)y));
                        }
                    }
                }

                if (enemyHousePos.x == enemyExitPos.x)
                {
                    if (enemyHousePos.y > enemyExitPos.y)
                    {
                        mapsArrayBuilder[i].EnemyExitDir = Movable.Direction.Up;
                        mapsArrayBuilder[i].EnemyExitPos = new half2(enemyExitPos.x, (half)(enemyExitPos.y - 1));
                    }
                    else
                    {
                        mapsArrayBuilder[i].EnemyExitDir = Movable.Direction.Down;
                        mapsArrayBuilder[i].EnemyExitPos = new half2(enemyExitPos.x, (half)(enemyExitPos.y + 2));
                    }
                }
                else
                {
                    if (enemyHousePos.x > enemyExitPos.x)
                    {
                        mapsArrayBuilder[i].EnemyExitDir = Movable.Direction.Left;
                        mapsArrayBuilder[i].EnemyExitPos = new half2((half)(enemyExitPos.x - 1), enemyExitPos.y);
                    }
                    else
                    {
                        mapsArrayBuilder[i].EnemyExitDir = Movable.Direction.Right;
                        mapsArrayBuilder[i].EnemyExitPos = new half2((half)(enemyExitPos.x + 2), enemyExitPos.y);
                    }
                }
                mapsArrayBuilder[i].EnemyHousePos = enemyHousePos;
            }
            var result = builder.CreateBlobAssetReference<MapsConfigData>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
    }
}
