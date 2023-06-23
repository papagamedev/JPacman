
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct MenuConfigData
{
    public float PlayerSpeed;
    public float EnemySpeed;
    public float2 PlayerBoundsSize;
    public float2 EnemiesBoundsSize;

    public static BlobAssetReference<MenuConfigData> CreateMenuConfigBlob(MainAuthoring authoring)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var menuConfigData = ref builder.ConstructRoot<MenuConfigData>();
        var menuData = authoring.Config.MenuConfig;
        menuConfigData.PlayerSpeed = menuData.PlayerSpeed;
        menuConfigData.EnemySpeed = menuData.EnemySpeed;
        menuConfigData.PlayerBoundsSize = menuData.PlayerBoundsSize;
        menuConfigData.EnemiesBoundsSize = menuData.EnemiesBoundsSize;
        var result = builder.CreateBlobAssetReference<MenuConfigData>(Allocator.Persistent);
        builder.Dispose();
        return result;
    }
}
