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
                MapsConfigBlob = mapsConfigBlob
            });
            AddBuffer<SoundEventBufferElement>(entity);
            AddBuffer<SoundStopEventBufferElement>(entity);
            AddBuffer<MusicEventBufferElement>(entity);
            AddBuffer<SetLivesTextBufferElement>(entity);
            AddBuffer<SetScoreTextBufferElement>(entity);
            AddBuffer<SetLabelTextBufferElement>(entity);
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
                arrayBuilder[i].PlayerSpeed = levelsConfig[i].PlayerSpeed;
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
                mapsArrayBuilder[i].PlayerPos = map.m_playerPos;
                var arrayBuilder = builder.Allocate(ref mapsArrayBuilder[i].MapData, mapDataSize);
                for (int y = 0; y < map.m_height; y++)
                {
                    for (int x = 0; x < map.m_width; x++)
                    {
                        arrayBuilder[y * map.m_width + x] = map.m_data[x, y];
                    }
                }
            }
            var result = builder.CreateBlobAssetReference<MapsConfigData>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
    }
}
