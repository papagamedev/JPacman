using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct LevelPlayingPhaseSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<Game>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        ecb.AppendToBuffer(mainEntity, new MusicEventBufferElement()
        {
            MusicType = gameAspect.LevelData.BonusLevel ? AudioEvents.MusicType.LevelBonus : AudioEvents.MusicType.Level
        });
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

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        gameAspect.UpdatePlayingTime(SystemAPI.Time.DeltaTime);
        gameAspect.CheckSpawnFruit(mainEntity, ecb);
        gameAspect.CheckMoveDots(mainEntity, ecb);
        gameAspect.CheckMovePowerups(mainEntity, ecb);

        if (gameAspect.IsLevelCompleted() || Input.GetKeyDown(KeyCode.W))
        {
            SwitchToWinPhase(mainEntity, ecb);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameAspect.SetPaused(true, mainEntity, ecb);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<Main>(out var mainEntity))
        {
            return;
        }
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        ecb.AppendToBuffer(mainEntity, new SoundStopEventBufferElement()
        {
            SoundType = AudioEvents.SoundType.EnemyReturnHome
        });
        ecb.RemoveComponent<PowerupModeActiveTag>(mainEntity);
        ecb.RemoveComponent<DotsMovingTag>(mainEntity);
        ecb.RemoveComponent<PowerupsMovingTag>(mainEntity);
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }


    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    private void SwitchToWinPhase(Entity mainEntity, EntityCommandBuffer ecb)
    {
        ecb.RemoveComponent<LevelPlayingPhaseTag>(mainEntity);
        ecb.AddComponent<LevelWinPhaseTag>(mainEntity);
    }
}

