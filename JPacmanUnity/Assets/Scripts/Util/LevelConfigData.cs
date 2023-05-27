using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

public struct LevelConfigData
{
    public int Idx;
    public int RoundNumber;
    public int LevelNumber;
    public bool BonusLevel;
    public bool MoveDots;
    public int MultiplyDots;
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
}
