using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct LevelWinPhaseSystem : ISystem, ISystemStartStop
{
    enum Mode
    {
        DeathAnim,
        FadeOut
    };
    private const float kTotalWinTime = 4.0f;
    private const float kFadeOutTime = 0.5f;
    private float m_phaseTimer;
    private Mode m_mode;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelWinPhaseTag>();
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<Game>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        ecb.AppendToBuffer(mainEntity, new MusicEventBufferElement()
        {
            MusicType = AudioEvents.MusicType.LevelEnd
        });

        foreach (var (enemy, entity) in SystemAPI.Query<Enemy>().WithEntityAccess())
        {
            ecb.AddComponent(entity, new DeathAnimation()
            {
                Duration = kTotalWinTime
            });
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        m_phaseTimer = 0;
        m_mode = Mode.DeathAnim;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        m_phaseTimer += SystemAPI.Time.DeltaTime;

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        switch (m_mode)
        {
            case Mode.DeathAnim:
                if (m_phaseTimer >= kTotalWinTime - kFadeOutTime)
                {
                    ecb.AppendToBuffer(mainEntity, new FadeAnimationBufferElement()
                    {
                        Duration = kFadeOutTime,
                        IsFadeIn = false
                    });
                    m_mode = Mode.FadeOut;
                }
                break;

            case Mode.FadeOut:
                if (m_phaseTimer >= kTotalWinTime)
                {
                    var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
                    gameAspect.SetNextLevel();

                    SwitchToLevelClearPhase(mainEntity, ecb);
                }
                break;
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {

    }


    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    private void SwitchToLevelClearPhase(Entity mainEntity, EntityCommandBuffer ecb)
    {
        ecb.RemoveComponent<LevelWinPhaseTag>(mainEntity);
        ecb.AddComponent<LevelClearPhaseTag>(mainEntity);
    }

}

