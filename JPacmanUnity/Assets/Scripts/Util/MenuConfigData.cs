
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct MenuConfigData
{
    public float DotSpeed;
    public float PlayerSpeed;
    public float EnemySpeed;
    public float2 PlayerBoundsSize;
    public float2 EnemiesBoundsSize;
    public BlobArray<MenuShapeData> ShapeData;

    public struct MenuShapeData
    {
        public UIEvents.ShowUIType UIType;
        public int ShapeIdx;
        public float2 ShapePos;
    }

    public static BlobAssetReference<MenuConfigData> CreateMenuConfigBlob(MainAuthoring authoring, Dictionary<MenuDotShapeConfig, int> shapeIndexMap)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var menuConfigData = ref builder.ConstructRoot<MenuConfigData>();
        var menuData = authoring.Config.MenuConfig;
        menuConfigData.PlayerSpeed = menuData.PlayerSpeed;
        menuConfigData.EnemySpeed = menuData.EnemySpeed;
        menuConfigData.PlayerBoundsSize = menuData.PlayerBoundsSize;
        menuConfigData.EnemiesBoundsSize = menuData.EnemiesBoundsSize;
        menuConfigData.DotSpeed = menuData.DotSpeed;
        var dotShapes = menuData.DotShapes;
        var shapesCount = dotShapes.Length;
        var shapesArrayBuilder = builder.Allocate(ref menuConfigData.ShapeData, shapesCount);
        int i = 0;
        foreach (var shape in dotShapes)
        {
            shapesArrayBuilder[i].UIType = shape.UIType;
            shapesArrayBuilder[i].ShapePos = new float2(shape.Pos.x, -shape.Pos.y);
            shapesArrayBuilder[i].ShapeIdx = shapeIndexMap[shape.Shape];
            i++;
        }
        var result = builder.CreateBlobAssetReference<MenuConfigData>(Allocator.Persistent);
        builder.Dispose();
        return result;
    }
}
