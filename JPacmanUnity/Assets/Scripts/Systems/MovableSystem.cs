using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PlayerSystem))]
public partial struct MovableSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<Movable>();
        state.RequireForUpdate<Main>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        new MoveEntityJob
        {
            DeltaTime = deltaTime,
            BlobMapRef = mainComponent.ValueRO.MapConfigBlob
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

public partial struct MoveEntityJob : IJobEntity
{
    public float DeltaTime;
    public BlobAssetReference<MapConfigData> BlobMapRef;

    private void Execute(MovableAspect movable)
    {
       movable.Move(BlobMapRef, DeltaTime);
    }
}

