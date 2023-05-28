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
    enum EnemyPhase
    {
        Start,
        Follow,
        Scared
    }
    private EnemyPhase m_enemyPhase;
    private float m_enemyTime;
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
        CreateEnemies(ref state, mainComponent, ref introData, mainEntity, ecb);
        CreatePlayer(ref state, mainComponent, ref introData, mainEntity, ecb);

        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        m_enemyTime = 0;
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
        UpdateDotsShape(ref state, mainComponent, ref introData, mainEntity, ecb);
        UpdatePlayerEnemies(ref state, mainComponent, ref introData, mainEntity, ecb);
        if (Input.anyKeyDown)
        {
            GoToMainMenu(mainEntity, ecb);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
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
        ecb.Dispose();
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
    }

    private void CreateEnemies(ref SystemState state, RefRO<Main> mainComponent, ref IntroConfigData introData, Entity mainEntity, EntityCommandBuffer ecb)
    {
        for (int i = 0; i < 4; i++)
        {
            var enemy = ecb.Instantiate(mainComponent.ValueRO.EnemyPrefab);
            ecb.SetComponent(enemy,
                new LocalTransform()
                {
                    Position = new float3(introData.EnemyStartPos.x - introData.EnemySpacing * i, introData.EnemyStartPos.y, 0),
                    Scale = 1.0f,
                    Rotation = quaternion.identity,
                });
            ecb.AddComponent(enemy,
                new MoveAnimator()
                {
                    Speed = introData.EnemyFollowSpeed,
                    Direction = new float2(1, 0)
                });
            ecb.AddComponent(enemy,
                new Enemy()
                {
                    Id = i
                });
            ecb.AddComponent(enemy,
                new SpriteSetColor()
                {
                    Color = Color.white
                });
        }
    }

    private void UpdateDotsShape(ref SystemState state, RefRO<Main> mainComponent, ref IntroConfigData introData, Entity mainEntity, EntityCommandBuffer ecb)
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

    private void UpdatePlayerEnemies(ref SystemState state, RefRO<Main> mainComponent, ref IntroConfigData introData, Entity mainEntity, EntityCommandBuffer ecb)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        m_enemyTime += deltaTime;

        switch (m_enemyPhase)
        {
            case EnemyPhase.Start:
                RunPlayerEnemiesJob(ref state, Direction.Right, introData.EnemyFollowSpeed, false);
                m_enemyPhase = EnemyPhase.Follow;
                m_enemyTime = 0;
                break;

            case EnemyPhase.Follow:
                if (m_enemyTime >= introData.EnemyFollowDuration)
                {
                    RunPlayerEnemiesJob(ref state, Direction.Left, introData.EnemyScaredSpeed, true);
                    m_enemyPhase = EnemyPhase.Scared;
                    m_enemyTime = 0;
                }
                break;
        }
    }

    private void RunPlayerEnemiesJob(ref SystemState state, Direction dir, float enemySpeed, bool enemyScared)
    {
        new UpdateIntroEnemyJob
        {
            NewDir = dir,
            NewSpeed = enemySpeed,
            Scared = enemyScared
        }.ScheduleParallel();
        new UpdateIntroPlayerJob
        {
            NewDir = dir,
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

public partial struct UpdateIntroEnemyJob : IJobEntity
{
    public Direction NewDir;
    public bool Scared;
    public float NewSpeed;

    private void Execute(RefRO<Enemy> enemy, RefRW<MoveAnimator> animator, RefRW<SpriteAnimator> spriteAnim, RefRW<SpriteSetColor> spriteColor, RefRO<EnemyDef> enemyDef)
    {
        animator.ValueRW.Speed = NewSpeed;
        animator.ValueRW.Direction = NewDir.Vector();

        int framesPerDirAnim = spriteAnim.ValueRO.FramesCount / 4;
        spriteAnim.ValueRW.StartFrame = framesPerDirAnim * (int)NewDir;
        spriteAnim.ValueRW.LastFrame = framesPerDirAnim * (int)NewDir + framesPerDirAnim - 1;

        spriteColor.ValueRW.Color = Scared ? enemyDef.ValueRO.EnemyScaredColor : enemyDef.ValueRO.EnemyColors[enemy.ValueRO.Id];
    }
}


public partial struct UpdateIntroPlayerJob : IJobEntity
{
    public Direction NewDir;

    private void Execute(RefRO<Player> player, RefRW<MoveAnimator> animator, RefRW<SpriteAnimator> spriteAnim)
    {
        animator.ValueRW.Direction = NewDir.Vector();

        int framesPerDirAnim = spriteAnim.ValueRO.FramesCount / 4;
        spriteAnim.ValueRW.StartFrame = framesPerDirAnim * (int)NewDir;
        spriteAnim.ValueRW.LastFrame = framesPerDirAnim * (int)NewDir + framesPerDirAnim - 1;
    }
}

