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
            var gameConfig = authoring.Config;
            var livesCount = gameConfig.LivesCount;
            AddComponent(entity, new Main
            {
                DotPrefab = GetEntity(gameConfig.DotPrefab, TransformUsageFlags.None),
                EnemyPrefab = GetEntity(gameConfig.EnemyPrefab, TransformUsageFlags.None),
                PlayerPrefab = GetEntity(authoring.Config.PlayerPrefab, TransformUsageFlags.None),
                WallPrefab = GetEntity(gameConfig.WallPrefab, TransformUsageFlags.None),
                FruitPrefab = GetEntity(gameConfig.FruitPrefab, TransformUsageFlags.None),
                PowerupPrefab = GetEntity(gameConfig.PowerupPrefab, TransformUsageFlags.None),
                LevelsConfigBlob = levelsConfigBlob,
                MapsConfigBlob = mapsConfigBlob,
                IntroConfigBlob = introConfigBlob,
                RandomSeed = (uint)(System.DateTime.Now.Ticks % 1000000000),
                LivesCount = livesCount
            });
            AddBuffer<SoundEventBufferElement>(entity);
            AddBuffer<SoundStopEventBufferElement>(entity);
            AddBuffer<MusicEventBufferElement>(entity);
            AddBuffer<PauseAudioEventBufferElement>(entity);
            AddBuffer<FadeMusicEventBufferElement>(entity);
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

            var startLevel = gameConfig.DebugStartLevel;
            if (startLevel >= 0)
            {
                AddComponent(entity, new Game()
                {
                    Lives = livesCount,
                    Score = 0,
                    LevelId = startLevel
                });
            }
            else
            {
                AddComponent(entity, new IntroPhaseTag());
            }
        }
    }
}

