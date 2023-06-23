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
    const float kFadeTime = 0.5f;

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
    private float m_fadeTime;
    private bool m_inFade;

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
        m_fadeTime = 0;
        m_shapeIdx = 0;
        m_inFade = false;
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
        UpdateFade(ref state, mainEntity, ecb);
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (_, entity) in SystemAPI.Query<IntroSpriteTag>().WithEntityAccess())
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
        ecb.AddComponent(mainEntity, new MenuPhase()
        {
            UIType = UIEvents.ShowUIType.Menu
        });
    }

    private void CreateDots(ref SystemState state, RefRO<Main> mainComponent, ref IntroConfigData introData, Entity mainEntity, EntityCommandBuffer ecb)
    {
        ecb.AddComponent(mainEntity, new MenuDotShape()
        {
            ShapeIdx = 0,
            ShapePos = introData.ShapeData[0].ShapePos
        });
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
                    Direction = Direction.Right
                });
        ecb.AddComponent(player, new IntroSpriteTag());
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
                    Direction = Direction.Right
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
            ecb.AddComponent(enemy, new IntroSpriteTag());
        }
    }

    private void UpdateDotsShape(ref SystemState state, RefRO<Main> mainComponent, ref IntroConfigData introData, Entity mainEntity, EntityCommandBuffer ecb)
    {
        if (m_shapeIdx >= introData.ShapeData.Length)
        {
            return;
        }

        var deltaTime = SystemAPI.Time.DeltaTime;
        m_shapeTime += deltaTime;
        if (m_shapeTime >= introData.ShapeData[m_shapeIdx].Duration)
        {
            m_shapeTime = 0;
            m_shapeIdx++;

            if (m_shapeIdx < introData.ShapeData.Length)
            {
                ecb.SetComponent(mainEntity, new MenuDotShape()
                {
                    ShapeIdx = m_shapeIdx,
                    ShapePos = introData.ShapeData[m_shapeIdx].ShapePos
                });
            }
            else
            {
                StartFade(mainEntity, ecb);
            }
        }
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

    private void StartFade(Entity mainEntity, EntityCommandBuffer ecb)
    {
        m_inFade = true;
        ecb.AppendToBuffer(mainEntity, new FadeAnimationBufferElement()
        {
            Duration = kFadeTime,
            IsFadeIn = false
        });
        ecb.AppendToBuffer(mainEntity, new FadeMusicEventBufferElement()
        {
            IsFadeIn = false,
            Duration = kFadeTime
        });
    }

    private void UpdateFade(ref SystemState state, Entity mainEntity, EntityCommandBuffer ecb)
    {
        if (!m_inFade)
        {
            if (Input.anyKeyDown)
            {
                StartFade(mainEntity, ecb);
            }
            return;
        }

        var deltaTime = SystemAPI.Time.DeltaTime;
        m_fadeTime += deltaTime;
        if (m_fadeTime >= kFadeTime)
        {
            GoToMainMenu(mainEntity, ecb);
        }
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
        animator.ValueRW.Direction = NewDir;

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
        animator.ValueRW.Direction = NewDir;

        int framesPerDirAnim = spriteAnim.ValueRO.FramesCount / 4;
        spriteAnim.ValueRW.StartFrame = framesPerDirAnim * (int)NewDir;
        spriteAnim.ValueRW.LastFrame = framesPerDirAnim * (int)NewDir + framesPerDirAnim - 1;
    }
}

