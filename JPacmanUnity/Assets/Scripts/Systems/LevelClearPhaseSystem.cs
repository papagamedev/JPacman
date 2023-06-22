using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct LevelClearPhaseSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelClearPhase>();
        state.RequireForUpdate<Main>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var clearPhaseComponent = SystemAPI.GetComponent<LevelClearPhase>(mainEntity);
        var mainMenuType = clearPhaseComponent.MenuUIType;
        DestroyLevel(ref state, ecb);
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        if (gameAspect.Lives == 0 || gameAspect.IsPaused)
        {
            SwitchToMenu(mainEntity, ecb, mainMenuType);
        }
        else
        {
            SwitchToLevelStartPhase(mainEntity, ecb);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
       
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {

    }


    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    private void DestroyLevel(ref SystemState state, EntityCommandBuffer ecb)
    {
        foreach (var (collectible, entity) in SystemAPI.Query<Collectible>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
        }
        foreach (var (enemy, entity) in SystemAPI.Query<SpriteAnimator>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
        }
        foreach (var (wall, entity) in SystemAPI.Query<SpriteSetFrame>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
        }
        var player = SystemAPI.GetSingletonEntity<Player>();
        ecb.DestroyEntity(player);
    }

    private void SwitchToLevelStartPhase(Entity mainEntity, EntityCommandBuffer ecb)
    {
        ecb.RemoveComponent<LevelClearPhase>(mainEntity);
        ecb.AddComponent<LevelStartPhaseTag>(mainEntity);
    }

    private void SwitchToMenu(Entity mainEntity, EntityCommandBuffer ecb, UIEvents.ShowUIType uiType)
    {
        ecb.RemoveComponent<LevelClearPhase>(mainEntity);
        ecb.AddComponent(mainEntity, new MenuPhase()
        {
            UIType = uiType
        });
        ecb.RemoveComponent<Game>(mainEntity);
    }
}

