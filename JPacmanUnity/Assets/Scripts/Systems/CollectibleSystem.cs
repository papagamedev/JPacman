using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PlayerSystem))]
public partial struct CollectibleSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Collectible>();
        state.RequireForUpdate<Main>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var mainAspect = SystemAPI.GetComponentRO<Main>(mainEntity);
        ref var mapData = ref mainAspect.ValueRO.MapConfigBlob.Value;
        var player = SystemAPI.GetSingletonEntity<PlayerAspect>();
        var playerAspect = SystemAPI.GetAspect<PlayerAspect>(player);
        var playerWorldPos = playerAspect.GetWorldPos();
        var playerMapPos = mapData.WorldToMapPos(playerWorldPos);
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        new CollectJob
        {
            DeltaTime = deltaTime,
            MapData = mapData,
            PlayerMapPos = playerMapPos,
            Player = player,
            Main = mainEntity,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

public partial struct CollectJob : IJobEntity
{
    public float DeltaTime;
    public MapConfigData MapData;
    public float2 PlayerMapPos;
    public Entity Player;
    public Entity Main;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(CollectibleAspect collectible, [EntityIndexInQuery] int sortKey)
    {
        collectible.CheckPlayer(ref MapData, PlayerMapPos, sortKey, Player, Main, ECB);
    }
}

