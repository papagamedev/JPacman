using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct LevelDeadPhaseSystem : ISystem, ISystemStartStop
{
    private const float kDeathAnimTime = 2.0f;
    private float m_phaseTimer;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelDeadPhaseTag>();
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<Game>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        ecb.AppendToBuffer(mainEntity, new MusicEventBufferElement()
        {
            MusicType = AudioEvents.MusicType.Dead
        });

        var playerEntity = SystemAPI.GetSingletonEntity<Player>();
        ecb.AddComponent(playerEntity, new DeathAnimation()
        {
            Duration = kDeathAnimTime
        });

        ecb.Playback(state.EntityManager);

        m_phaseTimer = 0;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        m_phaseTimer += SystemAPI.Time.DeltaTime;

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        if (m_phaseTimer >= kDeathAnimTime)
        {
            var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
            var lives = gameAspect.RemoveLive();
            ecb.AppendToBuffer(mainEntity, new SetLivesTextBufferElement()
            {
                Value = lives
            });
            if (lives > 0)
            {
                SwitchToLevelResetLivePhase(mainEntity, ecb);
            }
            else
            {
                SwitchToLevelGameOverPhase(mainEntity, ecb);
            }
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {

    }


    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    private void SwitchToLevelResetLivePhase(Entity mainEntity, EntityCommandBuffer ecb)
    {
        ecb.RemoveComponent<LevelDeadPhaseTag>(mainEntity);
        ecb.AddComponent<LevelResetLivePhaseTag>(mainEntity);
    }

    private void SwitchToLevelGameOverPhase(Entity mainEntity, EntityCommandBuffer ecb)
    {
        ecb.RemoveComponent<LevelDeadPhaseTag>(mainEntity);
        ecb.AddComponent<LevelGameOverPhaseTag>(mainEntity);
    }

}

