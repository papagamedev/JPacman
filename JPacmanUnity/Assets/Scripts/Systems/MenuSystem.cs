using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct MenuSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<MenuPhaseTag>();
    }


    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        ecb.AppendToBuffer(mainEntity, new ShowUIBufferElement()
        {
            UI = HudEvents.ShowUIType.Menu
        });
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (Input.anyKeyDown)
        {
            var mainEntity = SystemAPI.GetSingletonEntity<Main>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            StartGame(0, mainEntity, ecb);
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

    private void StartGame(int levelIndex, Entity mainEntity, EntityCommandBuffer ecb)
    {
        ecb.RemoveComponent<MenuPhaseTag>(mainEntity);
        ecb.AddComponent(mainEntity, new Game()
        {
            Lives = 3,
            Score = 0,
            LevelId = levelIndex
        });
    }
}
