using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct LevelSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Main>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        ref var mapData = ref mainComponent.ValueRO.MapConfigBlob.Value;
        for (var y = 0; y < mapData.Height; y++)
        {
            for (var x = 0; x < mapData.Width; x++)
            {
                if (mapData.IsDot(x, y))
                {
                    var dot = ecb.Instantiate(mainComponent.ValueRO.DotPrefab);
                    ecb.SetComponent(dot,
                        new LocalTransform()
                        {
                            Position = mapData.MapToWorldPos(x, y),
                            Scale = 1.0f,
                            Rotation = quaternion.identity
                        });
                }
                else if (mapData.IsGhostsHorizontalHome(x, y))
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var enemy = ecb.Instantiate(mainComponent.ValueRO.EnemyPrefab);
                        ecb.SetComponent(enemy,
                            new LocalTransform()
                            {
                                Position = mapData.MapToWorldPos(x + i * 2.5f - 3.75f, y),
                                Scale = 1.0f,
                                Rotation = quaternion.identity
                            });
                    }
                }
            }
        }

        var player = ecb.Instantiate(mainComponent.ValueRO.PlayerPrefab);
        ecb.SetComponent(player, 
            new LocalTransform() 
            { 
                Position = mapData.MapToWorldPos(mapData.PlayerPos), 
                Scale = 1.0f, 
                Rotation = quaternion.identity
            });

        ecb.AddComponent<LevelStartPhaseTag>(mainEntity);

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

