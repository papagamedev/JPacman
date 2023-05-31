using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
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
        ecb.AppendToBuffer(mainEntity, new MusicEventBufferElement()
        {
            MusicType = AudioEvents.MusicType.Menu
        });
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (Input.anyKeyDown)
        {
            var mainEntity = SystemAPI.GetSingletonEntity<Main>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            StartGame(ref state, 0, mainEntity, ecb);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
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

    private void StartGame(ref SystemState state, int levelIndex, Entity mainEntity, EntityCommandBuffer ecb)
    {
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);

        var lives = mainComponent.ValueRO.LivesCount;
        ecb.RemoveComponent<MenuPhaseTag>(mainEntity);
        ecb.AddComponent(mainEntity, new Game()
        {
            Lives = lives,
            Score = 0,
            LevelId = levelIndex
        });
    }
}
