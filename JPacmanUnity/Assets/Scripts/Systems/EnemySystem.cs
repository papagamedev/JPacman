using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PlayerSystem))]
public partial struct EnemySystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<Player>();
        state.RequireForUpdate<Main>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        if (gameAspect.IsPaused)
        {
            return;
        }
        var mapsBlobRef = mainComponent.ValueRO.MapsConfigBlob;
        ref var map = ref gameAspect.GetCurrentMapData();
        var player = SystemAPI.GetSingletonEntity<Player>();
        bool playerIsTeleporting;
        float playerCollisionRadius = 0;
        float2 playerMapPos = float2.zero;
        if (!SystemAPI.HasComponent<Movable>(player))
        {
            // player is teleporting!
            playerIsTeleporting = true;
        }
        else
        {
            playerIsTeleporting = false;
            var playerAspect = SystemAPI.GetAspect<PlayerAspect>(player);
            var playerWorldPos = playerAspect.GetWorldPos();
            playerCollisionRadius = playerAspect.GetCollisionRadius();
            playerMapPos = map.WorldToMapPos(playerWorldPos);
        }
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        new EnemyFollowPlayerJob
        {
            DeltaTime = deltaTime,
            BlobMapsRef = mapsBlobRef,
            MapId = map.Id,
            PlayerIsTeleporting = playerIsTeleporting,
            PlayerMapPos = playerMapPos,
            PlayerCollisionRadius = playerCollisionRadius,
            EnemyCI = gameAspect.LevelData.EnemyCI,
            EnemySpeed = gameAspect.LevelData.EnemySpeed,
            EnemySpeedInTunnel = gameAspect.LevelData.EnemySpeedInTunnel,
            IsBonus = gameAspect.LevelData.BonusLevel,
            MainEntity = mainEntity,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
        new EnemyScaredJob
        {
            DeltaTime = deltaTime,
            BlobMapsRef = mapsBlobRef,
            MapId = map.Id,
            PlayerIsTeleporting = playerIsTeleporting,
            PlayerMapPos = playerMapPos,
            PlayerCollisionRadius = playerCollisionRadius,
            EnemySpeedScared = gameAspect.LevelData.EnemySpeedScared,
            Main = mainEntity,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
        new EnemyHomeScaredJob
        {
            DeltaTime = deltaTime,
            BlobMapsRef = mapsBlobRef,
            MapId = map.Id,
            EnemySpeedScared = gameAspect.LevelData.EnemySpeedScared,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
        new EnemyHomeJob
        {
            DeltaTime = deltaTime,
            BlobMapsRef = mapsBlobRef,
            MapId = map.Id,
            LiveTime = gameAspect.LiveTime,
            ExitHomeTime = gameAspect.LevelData.EnemyInHomeTime,
            EnemySpeed = gameAspect.LevelData.EnemySpeed,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
        new EnemyReturnHomeJob
        {
            DeltaTime = deltaTime,
            BlobMapsRef = mapsBlobRef,
            MapId = map.Id,
            EnemySpeedReturnHome = gameAspect.LevelData.EnemySpeedReturnHome,
            Main = mainEntity,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

public partial struct EnemyHomeJob : IJobEntity
{
    public float DeltaTime;
    public BlobAssetReference<MapsConfigData> BlobMapsRef;
    public int MapId;
    public float LiveTime;
    public float ExitHomeTime;
    public float EnemySpeed;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(EnemyHomeAspect enemy, [EntityIndexInQuery] int sortKey)
    {
        enemy.Update(BlobMapsRef, MapId, sortKey, LiveTime, ExitHomeTime, EnemySpeed, ECB);
    }
}

public partial struct EnemyFollowPlayerJob : IJobEntity
{
    public float DeltaTime;
    public BlobAssetReference<MapsConfigData> BlobMapsRef;
    public int MapId;
    public bool PlayerIsTeleporting;
    public float2 PlayerMapPos;
    public float PlayerCollisionRadius;
    public int EnemyCI;
    public float EnemySpeed;
    public float EnemySpeedInTunnel;
    public bool IsBonus;
    public Entity MainEntity;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(EnemyFollowPlayerAspect enemy, [EntityIndexInQuery] int sortKey)
    {
        enemy.Update(BlobMapsRef, MapId, PlayerIsTeleporting, PlayerMapPos, PlayerCollisionRadius, EnemyCI, EnemySpeed, EnemySpeedInTunnel, IsBonus, sortKey, MainEntity, ECB);
    }
}

public partial struct EnemyScaredJob : IJobEntity
{
    public float DeltaTime;
    public BlobAssetReference<MapsConfigData> BlobMapsRef;
    public int MapId;
    public bool PlayerIsTeleporting;
    public float2 PlayerMapPos;
    public float PlayerCollisionRadius;
    public float EnemySpeedScared;
    public Entity Main;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(EnemyScaredAspect enemy, [EntityIndexInQuery] int sortKey)
    {
        enemy.Update(BlobMapsRef, MapId, PlayerIsTeleporting, PlayerMapPos, PlayerCollisionRadius, EnemySpeedScared, sortKey, Main, ECB);
    }
}


public partial struct EnemyHomeScaredJob : IJobEntity
{
    public float DeltaTime;
    public BlobAssetReference<MapsConfigData> BlobMapsRef;
    public int MapId;
    public float EnemySpeedScared;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(EnemyHomeScaredAspect enemy, [EntityIndexInQuery] int sortKey)
    {
        enemy.Update(BlobMapsRef, MapId, EnemySpeedScared, ECB);
    }
}

public partial struct EnemyReturnHomeJob : IJobEntity
{
    public float DeltaTime;
    public BlobAssetReference<MapsConfigData> BlobMapsRef;
    public int MapId;
    public float EnemySpeedReturnHome;
    public Entity Main;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(EnemyReturnHomeAspect enemy, [EntityIndexInQuery] int sortKey)
    {
        enemy.Update(BlobMapsRef, MapId, EnemySpeedReturnHome, sortKey, Main, ECB);
    }
}
