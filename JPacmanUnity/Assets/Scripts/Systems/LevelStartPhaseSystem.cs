using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct LevelStartPhaseSystem : ISystem, ISystemStartStop
{
    private const float kFadeInTime = 0.5f;
    private const float kLabelSwapTime = 2.0f;
    private const float kTotalIntroTime = 4.0f;
        
    public enum LabelMode
    {
        Round,
        Message,
        None
    }

    private LabelMode m_labelMode;
    private float m_phaseTimer;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelStartPhaseTag>();
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<Game>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        ecb.AppendToBuffer(mainEntity, new MusicEventBufferElement()
        {
            MusicType = AudioEvents.MusicType.LevelStart
        });
        ecb.AppendToBuffer(mainEntity, new SetLabelTextBufferElement()
        {
            Value = HudEvents.LabelMessage.Round
        });
        ecb.AppendToBuffer(mainEntity, new SetLivesTextBufferElement()
        {
            Value = gameAspect.Lives
        });
        ecb.AppendToBuffer(mainEntity, new SetScoreTextBufferElement()
        {
            HasAnimation = false,
            Score = gameAspect.Score
        });
        ecb.AppendToBuffer(mainEntity, new FadeAnimationBufferElement()
        {
            Duration = kFadeInTime,
            IsFadeIn = true
        });

        uint randSeed = gameAspect.RandomSeed;
        gameAspect.CreateLevel(ecb, mainEntity, randSeed);
        var powerupModeAspect = SystemAPI.GetAspect<PowerupModeAspect>(mainEntity);
        powerupModeAspect.InitLive(gameAspect.LevelData);

        ecb.Playback(state.EntityManager);

        m_phaseTimer = 0;
        m_labelMode = LabelMode.Round;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        m_phaseTimer += SystemAPI.Time.DeltaTime;
        switch (m_labelMode)
        {
            case LabelMode.Round:
                if (m_phaseTimer >= kLabelSwapTime)
                {
                    SetLabelMessage(mainEntity, gameAspect, ecb);
                }
                break;

            case LabelMode.Message:
                if (m_phaseTimer >= kTotalIntroTime)
                {
                    SwitchToPlayingPhase(mainEntity, ecb);
                }
                break;
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {

    }


    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    private void SetLabelMessage(Entity mainEntity, GameAspect gameAspect, EntityCommandBuffer ecb)
    {
        var setLabel = new SetLabelTextBufferElement()
        {
            Value = gameAspect.LevelData.BonusLevel ? HudEvents.LabelMessage.Bonus : HudEvents.LabelMessage.Level
        };
        ecb.AppendToBuffer(mainEntity, setLabel);
        m_labelMode = LabelMode.Message;
    }

    private void SwitchToPlayingPhase(Entity mainEntity, EntityCommandBuffer ecb)
    {
        var setLabel = new SetLabelTextBufferElement()
        {
            Value = HudEvents.LabelMessage.None
        };
        ecb.AppendToBuffer(mainEntity, setLabel);
        ecb.RemoveComponent<LevelStartPhaseTag>(mainEntity);
        ecb.AddComponent<LevelPlayingPhaseTag>(mainEntity);
    }

    

}

