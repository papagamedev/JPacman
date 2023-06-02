using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct LevelResetLivePhaseSystem : ISystem, ISystemStartStop
{
    private const float kWaitReadyTime = 1.0f;

    private float m_phaseTimer;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelResetLivePhaseTag>();
        state.RequireForUpdate<Main>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        ResetLevel(ref state, ecb);
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        gameAspect.InitLive(ecb, mainEntity, gameAspect.RandomSeed);
        var powerupModeAspect = SystemAPI.GetAspect<PowerupModeAspect>(mainEntity);
        powerupModeAspect.InitLive(gameAspect.LevelData);
        ref var mapData = ref gameAspect.GetCurrentMapData();
        foreach (var (powerup, entity) in SystemAPI.Query<Powerup>().WithEntityAccess())
        {
            gameAspect.ResetPowerupPos(powerup.Id, entity, ecb, ref mapData);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        m_phaseTimer = 0;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        m_phaseTimer += SystemAPI.Time.DeltaTime;

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        if (m_phaseTimer >= kWaitReadyTime)
        {
            SwitchToLevelPlayingPhase(mainEntity, ecb);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {

    }


    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    private void ResetLevel(ref SystemState state, EntityCommandBuffer ecb)
    {
        // all collectibles should stop moving
        foreach (var (collectible, entity) in SystemAPI.Query<Collectible>().WithEntityAccess())
        {
            ecb.RemoveComponent<Movable>(entity);
        }

        // delete all enemies
        foreach (var (enemy, entity) in SystemAPI.Query<Enemy>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
        }

        // delete player
        var player = SystemAPI.GetSingletonEntity<Player>();
        ecb.DestroyEntity(player);
    }

    private void SwitchToLevelPlayingPhase(Entity mainEntity, EntityCommandBuffer ecb)
    {
        ecb.RemoveComponent<LevelResetLivePhaseTag>(mainEntity);
        ecb.AddComponent<LevelPlayingPhaseTag>(mainEntity);
    }

}

