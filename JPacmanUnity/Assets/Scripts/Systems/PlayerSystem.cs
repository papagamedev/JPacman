using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(MovableSystem))]
public partial struct PlayerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<Player>();
        state.RequireForUpdate<Main>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var gameAspect = new GameAspect(mainEntity,
            SystemAPI.GetComponentRW<Main>(mainEntity), SystemAPI.GetComponentRW<Game>(mainEntity),
            SystemAPI.GetBuffer<AddScoreBufferElement>(mainEntity), SystemAPI.GetBuffer<DotCloneBufferElement>(mainEntity));
        if (gameAspect.IsPaused)
        {
            return;
        }

        new PlayerJob
        {
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}


public partial struct PlayerJob : IJobEntity
{
    private void Execute(Entity entity, RefRW<LocalTransform> transform, RefRW<Movable> movable, RefRO<CollisionCircle> collision, RefRW<Player> player)
    {
        new PlayerAspect(entity, transform, movable, collision, player).UpdateMovement();
    }
}
