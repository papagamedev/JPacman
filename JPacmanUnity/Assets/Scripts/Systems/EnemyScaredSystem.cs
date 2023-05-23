using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PlayerSystem))]
public partial struct EnemyScaredSystem : ISystem, ISystemStartStop
{
    const float kBlinkTime = 3.0f;
    const float kBlinkFreq = 0.5f;
    private float m_phaseTimer;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyScaredPhaseTag>();
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<Player>();
        state.RequireForUpdate<Main>();
    }


    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        m_phaseTimer = gameAspect.LevelData.EnemyScaredTime;
        gameAspect.SetEnemyScaredBlinking(false);

        foreach (var (enemy, movable, entity) in SystemAPI.Query<EnemyFollowPlayerTag, RefRW<Movable>>().WithEntityAccess())
        {
            ecb.AddComponent(entity, new EnemyScaredTag() { });
            ecb.RemoveComponent<EnemyFollowPlayerTag>(entity);

            movable.ValueRW.CurrentDir = Movable.OppositeDir(movable.ValueRO.CurrentDir);
            movable.ValueRW.DesiredDir = Movable.Direction.None;
        }

        foreach (var (enemy, movable, entity) in SystemAPI.Query<EnemyHomeTag, RefRW<Movable>>().WithEntityAccess())
        {
            ecb.AddComponent(entity, new EnemyHomeScaredTag() { });
            ecb.RemoveComponent<EnemyHomeTag>(entity);

            movable.ValueRW.CurrentDir = Movable.OppositeDir(movable.ValueRO.CurrentDir);
            movable.ValueRW.DesiredDir = Movable.Direction.None;
        }

        ecb.AppendToBuffer(mainEntity, new SoundEventBufferElement()
        {
            SoundType = AudioEvents.SoundType.EnemyScared
        });


        ecb.Playback(state.EntityManager);

    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        m_phaseTimer -= SystemAPI.Time.DeltaTime;

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        if (m_phaseTimer <= 0)
        {
            ecb.RemoveComponent<EnemyScaredPhaseTag>(mainEntity);
        }
        else if (m_phaseTimer < kBlinkTime)
        {
            var scaredBlink = math.fmod(m_phaseTimer, kBlinkFreq) < kBlinkFreq * 0.5f;
            var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
            gameAspect.SetEnemyScaredBlinking(scaredBlink);
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    public void OnStopRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        ecb.AppendToBuffer(mainEntity, new SoundEventBufferElement()
        {
            SoundType = AudioEvents.SoundType.EnemyScared
        });
        foreach (var (enemy, entity) in SystemAPI.Query<EnemyScaredTag>().WithEntityAccess())
        {
            ecb.AddComponent(entity, new EnemyFollowPlayerTag() { });
            ecb.RemoveComponent<EnemyScaredTag>(entity);
        }

        foreach (var (enemy, entity) in SystemAPI.Query<EnemyHomeScaredTag>().WithEntityAccess())
        {
            ecb.AddComponent(entity, new EnemyHomeTag() { });
            ecb.RemoveComponent<EnemyHomeScaredTag>(entity);
        }
        ecb.Playback(state.EntityManager);
    }
}
