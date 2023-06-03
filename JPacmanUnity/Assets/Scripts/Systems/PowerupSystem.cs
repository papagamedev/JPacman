using Unity.Burst;
using Unity.Collections;
using Unity.Entities;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PlayerSystem))]
public partial struct PowerupSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PowerupMode>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Main>();
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
        powerupModeAspect.CheckEnemiesReturnedHome(mainEntity, ecb);
        if (powerupModeAspect.CheckPowerupCollected())
        {
            ecb.AddComponent(mainEntity, new PowerupModeActiveTag());
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

}
