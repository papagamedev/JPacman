
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct IntroConfigData
{
    public float DotSpeed;
    public float2 PlayerStartPos;
    public float PlayerSpeed;
    public float2 EnemyStartPos;
    public float EnemySpacing;
    public float EnemyFollowDuration;
    public float EnemyFollowSpeed;
    public float EnemyScaredDuration;
    public float EnemyScaredSpeed;
    public BlobArray<IntroShapeData> ShapeData;

    public struct IntroShapeData
    {
        public int ShapeIdx;
        public float2 ShapePos;
        public float Duration;
    }

    public static BlobAssetReference<IntroConfigData> CreateIntroConfigBlob(MainAuthoring authoring, Dictionary<MenuDotShapeConfig, int> shapeIndexMap)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var introConfigData = ref builder.ConstructRoot<IntroConfigData>();
        var introData = authoring.Config.IntroConfig;
        introConfigData.DotSpeed = introData.DotSpeed;
        introConfigData.PlayerStartPos = introData.PlayerStartPos;
        introConfigData.PlayerSpeed = introData.PlayerSpeed;
        introConfigData.EnemyStartPos = introData.EnemyStartPos;
        introConfigData.EnemySpacing = introData.EnemySpacing;
        introConfigData.EnemyFollowDuration = introData.EnemyFollowDuration;
        introConfigData.EnemyFollowSpeed = introData.EnemyFollowSpeed;
        introConfigData.EnemyScaredDuration = introData.EnemyScaredDuration;
        introConfigData.EnemyScaredSpeed = introData.EnemyScaredSpeed;
        var shapesCount = introData.DotShapes.Length;
        var shapesArrayBuilder = builder.Allocate(ref introConfigData.ShapeData, shapesCount);
        int i = 0;
        foreach (var shape in introData.DotShapes)
        {
            shapesArrayBuilder[i].Duration = shape.Duration;
            shapesArrayBuilder[i].ShapePos = new float2(shape.Pos.x, -shape.Pos.y);
            shapesArrayBuilder[i].ShapeIdx = shapeIndexMap[shape.Shape];
            i++;
        }
        var result = builder.CreateBlobAssetReference<IntroConfigData>(Allocator.Persistent);
        builder.Dispose();
        return result;
    }

}