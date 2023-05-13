using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
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
        state.RequireForUpdate<Player>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        float2 desiredDirection = new float2(math.sign(inputX), -math.sign(inputY));

        var entity = SystemAPI.GetSingletonEntity<PlayerAspect>();
        var playerAspect = SystemAPI.GetAspect<PlayerAspect>(entity);
        playerAspect.UpdateInput(desiredDirection);
        playerAspect.ApplyAddScore();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
