using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct IntroSystem : ISystem, ISystemStartStop
{
    private float m_shapeTime;
    private int m_shapeIdx;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<IntroPhaseTag>();
    }


    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        ecb.AppendToBuffer(mainEntity, new MusicEventBufferElement()
        {
            MusicType = AudioEvents.MusicType.Intro
        });

        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        ref var introData = ref mainComponent.ValueRO.IntroConfigBlob.Value;

        CreateDots(ref state, mainComponent, ref introData, mainEntity, ecb);
  //      CreateEnemies(ref state, mainComponent, ref introData, mainEntity, ecb);
    //    CreatePlayer(ref state, mainComponent, ref introData, mainEntity, ecb);

        ecb.Playback(state.EntityManager);

        m_shapeTime = 0;
        m_shapeIdx = 0;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        ref var introData = ref mainComponent.ValueRO.IntroConfigBlob.Value;
        UpdateShape(ref state, mainComponent, ref introData, mainEntity, ecb);
        if (Input.anyKeyDown)
        {
            GoToMainMenu(mainEntity, ecb);
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (collectible, entity) in SystemAPI.Query<MoveAnimator>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
        }
        foreach (var (collectible, entity) in SystemAPI.Query<IntroDotAnimator>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    private void GoToMainMenu(Entity mainEntity, EntityCommandBuffer ecb)
    {
        ecb.RemoveComponent<IntroPhaseTag>(mainEntity);
        ecb.AddComponent(mainEntity, new MenuPhaseTag());
    }

    private void CreateDots(ref SystemState state, RefRO<Main> mainComponent, ref IntroConfigData introData, Entity mainEntity, EntityCommandBuffer ecb)
    {
        ref var dotsPos = ref introData.ShapeData[0].DotPos;
        var dotsCount = dotsPos.Length;
        for (int i = 0; i < dotsCount; i++)
        {
            var dot = ecb.Instantiate(mainComponent.ValueRO.DotPrefab);
            ecb.SetComponent(dot,
                new LocalTransform()
                {
                    Position = new float3(dotsPos[i], 0),
                    Scale = 1.0f,
                    Rotation = quaternion.identity
                });
            ecb.RemoveComponent<Collectible>(dot);
            ecb.AddComponent(dot,
                new RotateAnimator()
                {
                    Speed = 30.0f
                });
            ecb.AddComponent(dot,
                new IntroDotAnimator()
                {
                    Idx = i,
                });
        }
    }

    private void CreatePlayer(ref SystemState state, RefRO<Main> mainComponent, ref IntroConfigData introData, Entity mainEntity, EntityCommandBuffer ecb)
    {
        var player = ecb.Instantiate(mainComponent.ValueRO.PlayerPrefab);
        ecb.SetComponent(player,
            new LocalTransform()
            {
                Position = new float3(introData.PlayerStartPos, 0),
                Scale = 1.0f,
                Rotation = quaternion.identity
            });
        ecb.AddComponent(player,
                new MoveAnimator()
                {
                    Speed = introData.PlayerSpeed,
                    Direction = new float2(1, 0)
                });
        ecb.RemoveComponent<Player>(player);
    }

    private void CreateEnemies(ref SystemState state, RefRO<Main> mainComponent, ref IntroConfigData introData, Entity mainEntity, EntityCommandBuffer ecb)
    {
        for (int i=0; i<4;i++)
        {
            var enemy = ecb.Instantiate(mainComponent.ValueRO.EnemyPrefab);
            ecb.SetComponent(enemy,
                new LocalTransform()
                {
                    Position = new float3(introData.PlayerStartPos, 0),
                    Scale = 1.0f,
                    Rotation = quaternion.identity,
                });
            ecb.AddComponent(enemy,
                    new MoveAnimator()
                    {
                        Speed = introData.EnemyFollowSpeed,
                        Direction = new float2(1, 0)
                    });
        }
    }

    private void UpdateShape(ref SystemState state, RefRO<Main> mainComponent, ref IntroConfigData introData, Entity mainEntity, EntityCommandBuffer ecb)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        m_shapeTime += deltaTime;
        if (m_shapeTime >= introData.ShapeData[m_shapeIdx].Duration)
        {
            m_shapeTime = 0;
            m_shapeIdx++;
            if (m_shapeIdx == introData.ShapeData.Length)
            {
                GoToMainMenu(mainEntity, ecb);
                return;
            }
        }

        new MoveIntroDotJob
        {
            DeltaTime = deltaTime,
            ShapeIdx = m_shapeIdx,
            BlobIntro = mainComponent.ValueRO.IntroConfigBlob
        }.ScheduleParallel();
    }


}

public partial struct MoveIntroDotJob : IJobEntity
{
    public float DeltaTime;
    public int ShapeIdx;
    public BlobAssetReference<IntroConfigData> BlobIntro;

    private void Execute(IntroDotAnimatorAspect introDot)
    {
        introDot.UpdateAnimation(DeltaTime, ShapeIdx, BlobIntro);
        
    }
}
