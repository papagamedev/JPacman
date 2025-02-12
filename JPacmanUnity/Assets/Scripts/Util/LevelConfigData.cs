using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

public struct LevelConfigData
{
    public enum ELevelType
    {
        Normal,
        Bonus,
        Final,
        Ultimate
    }

    public int Idx;
    public int RoundNumber;
    public int LevelNumber;
    public bool BonusLevel => LevelType == ELevelType.Bonus;
    public ELevelType LevelType;
    public float DotsMoveSpeed;
    public float DotsMoveWaitTime;
    public float PowerupsMoveSpeed;
    public float PowerupsMoveWaitTime;
    public float DotsCloneFactor;
    public float DotsRemainingCloneThreshold;
    public bool MovePowerups;
    public float PlayerSpeed;
    public float EnemySpeed;
    public float EnemySpeedInTunnel;
    public float EnemySpeedScared;
    public float EnemySpeedReturnHome;
    public float EnemyInHomeTime;
    public float EnemyScaredTime;
    public int EnemyScore;
    public int EnemyCI;
    public int FruitScore;
    public int FruitSpriteIdx;
    public float FruitWaitTime;
    public float FruitDuration;
    public int MapId;
}


public struct LevelsConfigData
{
    public BlobArray<LevelConfigData> LevelsData;

    public static BlobAssetReference<LevelsConfigData> CreateLevelsConfigBlob(MainAuthoring authoring, List<MapConfig> maps)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var levelsConfigData = ref builder.ConstructRoot<LevelsConfigData>();
        int levelCount = 0;
        foreach (var roundConfig in authoring.Config.RoundConfigs)
        {
            levelCount += roundConfig.LevelConfigs.Length;
        }
        var arrayBuilder = builder.Allocate(ref levelsConfigData.LevelsData, levelCount);
        var mapsDictionary = new Dictionary<MapConfig, int>();

        int levelIdx = 0;
        foreach (var roundConfig in authoring.Config.RoundConfigs)
        {
            var map = roundConfig.MapConfig;
            if (!mapsDictionary.TryGetValue(map, out var mapId))
            {
                mapId = maps.Count;
                mapsDictionary.Add(map, mapId);
                maps.Add(roundConfig.MapConfig);
            }

            foreach (var levelConfig in roundConfig.LevelConfigs)
            {
                arrayBuilder[levelIdx].Idx = levelIdx;
                arrayBuilder[levelIdx].LevelNumber = levelConfig.LevelNumber;
                arrayBuilder[levelIdx].RoundNumber = roundConfig.RoundNumber;
                arrayBuilder[levelIdx].LevelType = levelConfig.LevelType;
                arrayBuilder[levelIdx].DotsMoveSpeed = levelConfig.DotsMoveSpeed;
                arrayBuilder[levelIdx].DotsMoveWaitTime = levelConfig.DotsMoveWaitTime + roundConfig.BaseDotsMoveWaitTime;
                arrayBuilder[levelIdx].DotsCloneFactor = levelConfig.DotsCloneFactor;
                arrayBuilder[levelIdx].DotsRemainingCloneThreshold = levelConfig.DotsRemainingCloneThreshold;
                arrayBuilder[levelIdx].PowerupsMoveSpeed = levelConfig.PowerupsMoveSpeed;
                arrayBuilder[levelIdx].PowerupsMoveWaitTime = levelConfig.PowerupsMoveWaitTime + roundConfig.BasePowerupsMoveWaitTime;
                arrayBuilder[levelIdx].PlayerSpeed = levelConfig.PlayerSpeed;
                arrayBuilder[levelIdx].EnemySpeed = levelConfig.EnemySpeed;
                arrayBuilder[levelIdx].EnemySpeedInTunnel = levelConfig.EnemySpeedInTunnel;
                arrayBuilder[levelIdx].EnemySpeedScared = levelConfig.EnemySpeedScared;
                arrayBuilder[levelIdx].EnemySpeedReturnHome = levelConfig.EnemySpeedReturnHome;
                arrayBuilder[levelIdx].EnemyInHomeTime = levelConfig.EnemyInHomeTime;
                arrayBuilder[levelIdx].EnemyScaredTime = levelConfig.EnemyScaredTime;
                arrayBuilder[levelIdx].EnemyScore = levelConfig.EnemyScore;
                arrayBuilder[levelIdx].EnemyCI = levelConfig.EnemyCI + roundConfig.BaseEnemyCI;

                var fruitConfig = levelConfig.FruitConfig;
                arrayBuilder[levelIdx].FruitScore = fruitConfig.Score;
                arrayBuilder[levelIdx].FruitSpriteIdx = fruitConfig.SpriteIdx;
                arrayBuilder[levelIdx].FruitWaitTime = fruitConfig.WaitTime;
                arrayBuilder[levelIdx].FruitDuration = fruitConfig.Duration;
 
                arrayBuilder[levelIdx].MapId = mapId;

                levelIdx++;
            }
        }
        var result = builder.CreateBlobAssetReference<LevelsConfigData>(Allocator.Persistent);
        builder.Dispose();
        return result;
    }
}
