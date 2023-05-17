using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MainAuthoring : MonoBehaviour
{
    public GameObject DotPrefab;
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public LevelConfig m_levelConfig;
    
    public class Baker : Baker<MainAuthoring>
    {
        public override void Bake(MainAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Main
            {
                DotPrefab = GetEntity(authoring.DotPrefab, TransformUsageFlags.None),
                EnemyPrefab = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.None),
                PlayerPrefab = GetEntity(authoring.PlayerPrefab, TransformUsageFlags.None),
                MapConfigBlob = CreateMapConfigBlob(authoring)
            });
            AddBuffer<SoundEventBufferElement>(entity);
            AddBuffer<MusicEventBufferElement>(entity);
            AddBuffer<SetLivesTextBufferElement>(entity);
            AddBuffer<SetScoreTextBufferElement>(entity);
            AddBuffer<SetLabelTextBufferElement>(entity);
            AddBuffer<StartScoreAnimationBufferElement>(entity);
        }

        private BlobAssetReference<MapConfigData> CreateMapConfigBlob(MainAuthoring authoring)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref MapConfigData mapConfig = ref builder.ConstructRoot<MapConfigData>();
            var levelConfigMap = authoring.m_levelConfig.Map;
            var mapDataSize = levelConfigMap.m_height * levelConfigMap.m_width;
            mapConfig.Width = levelConfigMap.m_width;
            mapConfig.Height = levelConfigMap.m_height;
            mapConfig.PlayerPos = levelConfigMap.m_playerPos;
            var arrayBuilder = builder.Allocate(ref mapConfig.MapData, mapDataSize);
            for (int y = 0; y < levelConfigMap.m_height; y++)
            {
                for (int x = 0; x < levelConfigMap.m_width; x++)
                {
                    arrayBuilder[y * levelConfigMap.m_width + x] = levelConfigMap.m_data[x, y];
                }
            }
            var result = builder.CreateBlobAssetReference<MapConfigData>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }

    }
}
