using System.Collections.Generic;
using Unity.Entities;
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
            var levelsConfigBlob = LevelsConfigData.CreateLevelsConfigBlob(authoring, maps);
            var mapsConfigBlob = MapsConfigData.CreateMapsConfigBlob(maps.ToArray());
            var introConfigBlob = IntroConfigData.CreateIntroConfigBlob(authoring);
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
                IntroConfigBlob = introConfigBlob,
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
    }
}

