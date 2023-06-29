using System.Collections.Generic;
using Unity.Collections;
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
            var mapsConfigBlob = MapsConfigData.CreateMapsConfigBlob(maps);
            var shapesConfigBlob = MenuDotShapeConfigData.CreateMenuDotShapeConfigBlob(authoring, out var shapeIndexMap);
            var introConfigBlob = IntroConfigData.CreateIntroConfigBlob(authoring, shapeIndexMap);
            var menuConfigBlob = MenuConfigData.CreateMenuConfigBlob(authoring, shapeIndexMap);
            var gameConfig = authoring.Config;
            var livesCount = gameConfig.LivesCount;
            var dotCloneColors = new FixedList128Bytes<Color>();
            foreach (var color in gameConfig.DotCloneColors)
            {
                dotCloneColors.Add(color);
            }
            var tunnelColors = new FixedList512Bytes<Color>();
            foreach (var color in gameConfig.TunnelColor)
            {
                tunnelColors.Add(color);
            }
            AddComponent(entity, new Main
            {
                DotPrefab = GetEntity(gameConfig.DotPrefab, TransformUsageFlags.None),
                EnemyPrefab = GetEntity(gameConfig.EnemyPrefab, TransformUsageFlags.None),
                PlayerPrefab = GetEntity(authoring.Config.PlayerPrefab, TransformUsageFlags.None),
                WallPrefab = GetEntity(gameConfig.WallPrefab, TransformUsageFlags.None),
                TilePrefab = GetEntity(gameConfig.TilePrefab, TransformUsageFlags.None),
                FruitPrefab = GetEntity(gameConfig.FruitPrefab, TransformUsageFlags.None),
                PowerupPrefab = GetEntity(gameConfig.PowerupPrefab, TransformUsageFlags.None),
                LevelsConfigBlob = levelsConfigBlob,
                MapsConfigBlob = mapsConfigBlob,
                IntroConfigBlob = introConfigBlob,
                MenuConfigBlob = menuConfigBlob,
                MenuDotShapeConfigBlob = shapesConfigBlob,
                RandomSeed = (uint)(System.DateTime.Now.Ticks % 1000000000),
                LivesCount = livesCount,
                DotCloneColors = dotCloneColors,
                TileColor = gameConfig.TileColor,
                TunnelColors = tunnelColors
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
            AddBuffer<DotCloneBufferElement>(entity);
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

