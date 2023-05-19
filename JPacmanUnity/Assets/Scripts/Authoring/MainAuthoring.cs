using System.Collections;
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
                LevelsConfigBlob = levelsConfigBlob,
                MapsConfigBlob = mapsConfigBlob,
                RandomSeed = (uint)(System.DateTime.Now.Ticks % 1000000000)
            });
            AddBuffer<SoundEventBufferElement>(entity);
            AddBuffer<SoundStopEventBufferElement>(entity);
            AddBuffer<MusicEventBufferElement>(entity);
            AddBuffer<SetLivesTextBufferElement>(entity);
            AddBuffer<SetScoreTextBufferElement>(entity);
            AddBuffer<SetLabelTextBufferElement>(entity);
            AddBuffer<SetLabelPosBufferElement>(entity);
            AddBuffer<StartScoreAnimationBufferElement>(entity);
            AddBuffer<FadeAnimationBufferElement>(entity);
            AddBuffer<AddScoreBufferElement>(entity);
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
                arrayBuilder[i].LevelNumber = levelsConfig[i].LevelNumber;
                arrayBuilder[i].RoundNumber = levelsConfig[i].RoundNumber;
                arrayBuilder[i].BonusLevel = levelsConfig[i].BonusLevel;
                arrayBuilder[i].MoveDots = levelsConfig[i].MoveDots;
                arrayBuilder[i].MultiplyDots = levelsConfig[i].MultiplyDots;
                arrayBuilder[i].MovePowerups = levelsConfig[i].MovePowerups;
                arrayBuilder[i].PlayerSpeed = levelsConfig[i].PlayerSpeed;
                arrayBuilder[i].EnemySpeed = levelsConfig[i].EnemySpeed;
                arrayBuilder[i].EnemyInHomeTime = levelsConfig[i].EnemyInHomeTime;
                arrayBuilder[i].EnemyScaredTime = levelsConfig[i].EnemyScaredTime;
                arrayBuilder[i].EnemyCI = levelsConfig[i].EnemyCI;
                arrayBuilder[i].FruitScore = levelsConfig[i].FruitScore;

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
                float2 enemyHousePos = new float2(0, 0);
                float2 enemyExitPos = new float2(0, 0);
                for (int y = 0; y < map.m_height; y++)
                {
                    for (int x = 0; x < map.m_width; x++)
                    {
                        var c = map.m_data[x, y];
                        arrayBuilder[y * map.m_width + x] = c;

                        if (c == MapConfigData.kPlayerChar)
                        {
                            mapsArrayBuilder[i].PlayerPos = new float2(x, y);
                        }
                        else if (c == MapConfigData.kLabelsChar)
                        {
                            mapsArrayBuilder[i].LabelMessagePos = new float2(x, y);
                        }
                        else if (c == MapConfigData.kEnemyExitChar)
                        {
                            enemyExitPos = new float2(x, y);
                        }
                        else if (c == MapConfigData.kEnemyHorizontalHomeChar ||
                            c == MapConfigData.kEnemyVerticalHomeChar)
                        {
                            enemyHousePos = new float2(x, y);
                        }
                    }
                }

                if (enemyHousePos.x == enemyExitPos.x)
                {
                    if (enemyHousePos.y > enemyExitPos.y)
                    {
                        mapsArrayBuilder[i].EnemyExitDir = Movable.Direction.Up;
                        mapsArrayBuilder[i].EnemyExitPos = new float2(enemyExitPos.x, enemyExitPos.y - 1);
                    }
                    else
                    {
                        mapsArrayBuilder[i].EnemyExitDir = Movable.Direction.Down;
                        mapsArrayBuilder[i].EnemyExitPos = new float2(enemyExitPos.x, enemyExitPos.y + 2);
                    }
                }
                else
                {
                    if (enemyHousePos.x > enemyExitPos.x)
                    {
                        mapsArrayBuilder[i].EnemyExitDir = Movable.Direction.Left;
                        mapsArrayBuilder[i].EnemyExitPos = new float2(enemyExitPos.x - 1, enemyExitPos.y);
                    }
                    else
                    {
                        mapsArrayBuilder[i].EnemyExitDir = Movable.Direction.Right;
                        mapsArrayBuilder[i].EnemyExitPos = new float2(enemyExitPos.x + 2, enemyExitPos.y);
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
