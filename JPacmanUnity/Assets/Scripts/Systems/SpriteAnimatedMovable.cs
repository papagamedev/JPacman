using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(MovableSystem))]
[UpdateBefore(typeof(SpriteAnimationSystem))]
public partial struct SpriteAnimatedMovableSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Movable>();
        state.RequireForUpdate<SpriteAnimator>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new SpriteAnimatedMoveEntityJob
        {
            
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

public partial struct SpriteAnimatedMoveEntityJob : IJobEntity
{
    private void Execute(SpriteAnimatedMovableAspect movable)
    {
        movable.UpdateAnimation();
    }
}

