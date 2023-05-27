
using System.Collections.Generic;
using System.IO.Compression;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct IntroConfigData
{
    public float DotSpeed;
    public float DotSpacing;
    public float2 PlayerStartPos;
    public float PlayerSpeed;
    public float2 EnemyStartPos;
    public float EnemySpacing;
    public float EnemyFollowDuration;
    public float EnemyFollowSpeed;
    public float EnemyScaredDuration;
    public float EnemyScaredSpeed;
    public BlobArray<IntroShapeData> ShapeData;

    public static BlobAssetReference<IntroConfigData> CreateIntroConfigBlob(MainAuthoring authoring)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var introConfigData = ref builder.ConstructRoot<IntroConfigData>();
        var introData = authoring.Config.IntroConfig;
        var dotSpacing = introData.DotSpacing;
        introConfigData.DotSpeed = introData.DotSpeed;
        introConfigData.DotSpacing = dotSpacing;
        introConfigData.PlayerStartPos = introData.PlayerStartPos;
        introConfigData.PlayerSpeed = introData.PlayerSpeed;
        introConfigData.EnemyStartPos = introData.EnemyStartPos;
        introConfigData.EnemySpacing = introData.EnemySpacing;
        introConfigData.EnemyFollowDuration = introData.EnemyFollowDuration;
        introConfigData.EnemyFollowSpeed = introData.EnemyFollowSpeed;
        introConfigData.EnemyScaredDuration = introData.EnemyScaredDuration;
        introConfigData.EnemyScaredSpeed = introData.EnemyScaredSpeed;
        var shapes = introData.Data;
        var shapesCount = shapes.Length;
        var shapesArrayBuilder = builder.Allocate(ref introConfigData.ShapeData, shapesCount);
        for (int i = 0; i < shapesCount; i++)
        {
            var shape = shapes[i];
            var shapePos = new float2(shape.Pos.x, -shape.Pos.y);
            shapesArrayBuilder[i].Duration = shape.Duration;
            var shapeLines = shape.Shape.Split("\n");
            var dotPosList = new List<float2>();
            int y = 0;
            foreach (var line in shapeLines)
            {
                int x = 0;
                foreach (var c in line)
                {
                    if (c == '#')
                    {
                        var pos = shapePos + new float2(x, -y) * dotSpacing;
                        dotPosList.Add(pos);
                    }
                    x++;
                }
                y++;
            }
            dotPosList.Sort( (a, b) => a.x < b.x ? -1 : a.x > b.x ? 1 : a.y < b.y ? -1 : a.y > b.y ? 1 : 0 );

            var dotsCount = dotPosList.Count;
            var arrayBuilder = builder.Allocate(ref shapesArrayBuilder[i].DotPos, dotsCount);
            var j = 0;
            foreach (var dotPos in dotPosList)
            {
                arrayBuilder[j++] = dotPos;
            }
        }
        var result = builder.CreateBlobAssetReference<IntroConfigData>(Allocator.Persistent);
        builder.Dispose();
        return result;
    }
}

public struct IntroShapeData
{
    public BlobArray<float2> DotPos;
    public float Duration;
}
