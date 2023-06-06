
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct MenuConfigData
{
    public float DotSpacing;
    public float PlayerSpeed;
    public float EnemySpeed;
    public float2 PlayerBoundsSize;
    public float2 EnemiesBoundsSize;
    public BlobArray<float2> ShapeDotPos;

    public static BlobAssetReference<MenuConfigData> CreateMenuConfigBlob(MainAuthoring authoring)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var menuConfigData = ref builder.ConstructRoot<MenuConfigData>();
        var menuData = authoring.Config.MenuConfig;
        var dotSpacing = menuData.DotSpacing;
        menuConfigData.DotSpacing = dotSpacing;
        menuConfigData.PlayerSpeed = menuData.PlayerSpeed;
        menuConfigData.EnemySpeed = menuData.EnemySpeed;
        menuConfigData.PlayerBoundsSize = menuData.PlayerBoundsSize;
        menuConfigData.EnemiesBoundsSize = menuData.EnemiesBoundsSize;
        var shapePos = new float2(menuData.DotsShapePos.x, -menuData.DotsShapePos.y);
        var shapeLines = menuData.DotsShape.Split("\n");
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
        var arrayBuilder = builder.Allocate(ref menuConfigData.ShapeDotPos, dotsCount);
        var j = 0;
        foreach (var dotPos in dotPosList)
        {
            arrayBuilder[j++] = dotPos;
        }
        var result = builder.CreateBlobAssetReference<MenuConfigData>(Allocator.Persistent);
        builder.Dispose();
        return result;
    }
}
