
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct MenuDotShapeData
{
    public BlobArray<float2> DotPos;
}

public struct MenuDotShapeConfigData
{
    public BlobArray<MenuDotShapeData> ShapesData;

    public static BlobAssetReference<MenuDotShapeConfigData> CreateMenuDotShapeConfigBlob(MainAuthoring authoring, out Dictionary<MenuDotShapeConfig, int> shapeIndexMap)
    {
        shapeIndexMap = new Dictionary<MenuDotShapeConfig, int>();
        var builder = new BlobBuilder(Allocator.Temp);
        ref var menuDotShapeData = ref builder.ConstructRoot<MenuDotShapeConfigData>();
        var shapes = authoring.Config.DotShapes;
        var shapesCount = shapes.Length;
        var shapeArrayBuilder = builder.Allocate(ref menuDotShapeData.ShapesData, shapesCount);
        for (int idx = 0; idx < shapesCount; idx++)
        {
            var shape = shapes[idx];
            var dotPosList = GetDotsShapePosList(shape);
            var dotsCount = dotPosList.Count;
            var arrayBuilder = builder.Allocate(ref shapeArrayBuilder[idx].DotPos, dotsCount);
            var j = 0;
            foreach (var dotPos in dotPosList)
            {
                arrayBuilder[j++] = dotPos;
            }
            shapeIndexMap.Add(shape, idx);
        }
        var result = builder.CreateBlobAssetReference<MenuDotShapeConfigData>(Allocator.Persistent);
        builder.Dispose();
        return result;
    }

    private static List<float2> GetDotsShapePosList(MenuDotShapeConfig shape)
    {
        var dotSpacing = shape.DotSpacing;
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
                    var pos = new float2(x, -y) * dotSpacing;
                    dotPosList.Add(pos);
                }
                x++;
            }
            y++;
        }
        dotPosList.Sort((a, b) => a.x < b.x ? -1 : a.x > b.x ? 1 : a.y < b.y ? -1 : a.y > b.y ? 1 : 0);
        return dotPosList;
    }
}

