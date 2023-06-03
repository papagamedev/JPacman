using Unity.Burst;
using Unity.Collections;
using Unity.Entities;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PlayerSystem))]
public partial struct PowerupModeSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PowerupModeActiveTag>();
        state.RequireForUpdate<PowerupMode>();
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<Player>();
        state.RequireForUpdate<Main>();
    }


    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var powerupModeAspect = SystemAPI.GetAspect<PowerupModeAspect>(mainEntity);

        ecb.AppendToBuffer(mainEntity, new SoundEventBufferElement()
        {
            SoundType = AudioEvents.SoundType.EnemyScared
        });

        SetEnemyScaredState(ref state, powerupModeAspect, mainEntity, ecb);

        ecb.Playback(state.EntityManager);
        ecb.Dispose();

    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        if (gameAspect.IsPaused)
        {
            return;
        }
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var powerupModeAspect = SystemAPI.GetAspect<PowerupModeAspect>(mainEntity);
        if (powerupModeAspect.ProcessPowerupCollected(mainEntity, ecb))
        {
            SetEnemyScaredState(ref state, powerupModeAspect, mainEntity, ecb);
        }
        powerupModeAspect.UpdateActive(SystemAPI.Time.DeltaTime, mainEntity, ecb);
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    public void OnStopRunning(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<Main>(out var mainEntity))
        {
            return;
        }
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        ecb.AppendToBuffer(mainEntity, new SoundStopEventBufferElement()
        {
            SoundType = AudioEvents.SoundType.EnemyScared
        });
        SetEnemyNormalState(ref state, mainEntity, ecb);
        ecb.RemoveComponent<PowerupModeActiveTag>(mainEntity);
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void SetEnemyScaredState(ref SystemState state, PowerupModeAspect powerupModeAspect, Entity mainEntity, EntityCommandBuffer ecb)
    {
        int count = 0;
        foreach (var (enemy, movable, entity) in SystemAPI.Query<EnemyFollowPlayerTag, RefRW<Movable>>().WithEntityAccess())
        {
            ecb.AddComponent(entity, new EnemyScaredTag() { });
            ecb.RemoveComponent<EnemyFollowPlayerTag>(entity);

            movable.ValueRW.CurrentDir = movable.ValueRO.CurrentDir.Opposite();
            movable.ValueRW.DesiredDir = Direction.None;
            count++;
        }

        foreach (var (enemy, movable, entity) in SystemAPI.Query<EnemyHomeTag, RefRW<Movable>>().WithEntityAccess())
        {
            ecb.AddComponent(entity, new EnemyHomeScaredTag() { });
            ecb.RemoveComponent<EnemyHomeTag>(entity);

            movable.ValueRW.CurrentDir = movable.ValueRO.CurrentDir.Opposite();
            movable.ValueRW.DesiredDir = Direction.None;
            count++;
        }

        powerupModeAspect.AddEnemyScaredCount(count);
 
    }

    private void SetEnemyNormalState(ref SystemState state, Entity mainEntity, EntityCommandBuffer ecb)
    {
        foreach (var (enemy, entity) in SystemAPI.Query<EnemyScaredTag>().WithEntityAccess())
        {
            ecb.AddComponent(entity, new EnemyFollowPlayerTag() { });
            ecb.RemoveComponent<EnemyScaredTag>(entity);
        }

        foreach (var (enemy, entity) in SystemAPI.Query<EnemyHomeScaredTag>().WithEntityAccess())
        {
            ecb.AddComponent(entity, new EnemyHomeTag() { });
            ecb.RemoveComponent<EnemyHomeScaredTag>(entity);
        }
    }
}
