using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct LevelStartPhaseSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<LevelStartPhaseTag>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var musicEvent = new MusicEventBufferElement()
        {
            MusicType = AudioEvents.MusicType.LevelStart
        };
        ecb.AppendToBuffer(mainEntity, musicEvent);

        ecb.Playback(state.EntityManager);

        timer = 0;
    }

    float timer;
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();

        timer += SystemAPI.Time.DeltaTime;
        if (timer > 5)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            ecb.RemoveComponent<LevelStartPhaseTag>(mainEntity);
            ecb.AddComponent<LevelPlayingPhaseTag>(mainEntity);
            ecb.Playback(state.EntityManager);
        }
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {

    }


    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

