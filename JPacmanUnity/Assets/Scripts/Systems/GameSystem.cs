using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(MenuSystem))]
public partial struct GameSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<Game>();
    }


    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        StartLevel(mainEntity, ecb);
        ecb.AppendToBuffer(mainEntity, new ShowUIBufferElement()
        {
            UI = UIEvents.ShowUIType.Ingame
        });
        ecb.AddComponent(mainEntity, new PowerupMode()
        {
        });
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        gameAspect.ApplyAddScore(mainEntity, ecb);
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
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        ecb.AppendToBuffer(mainEntity, new KillAllScoreAnimationBufferElement() { Dummy = 0 });
        ecb.RemoveComponent<PowerupMode>(mainEntity);
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    private void StartLevel(Entity mainEntity, EntityCommandBuffer ecb)
    {        
        ecb.AddComponent(mainEntity, new LevelStartPhaseTag());
    }

}
