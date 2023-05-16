using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct LevelStartPhaseSystem : ISystem, ISystemStartStop
{
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
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<LevelStartPhaseTag>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var musicEvent = new MusicEventBufferElement()
        {
            MusicType = AudioEvents.MusicType.LevelStart
        };
        ecb.AppendToBuffer(mainEntity, musicEvent);

        var setLabel = new SetLabelTextBufferElement()
        {
            Value = LabelMode.Round
        };
        ecb.AppendToBuffer(mainEntity, setLabel);

        ecb.Playback(state.EntityManager);

        m_phaseTimer = 0;
        m_labelMode = LabelMode.Round;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        m_phaseTimer += SystemAPI.Time.DeltaTime;
        switch (m_labelMode)
        {
            case LabelMode.Round:
                if (m_phaseTimer >= kLabelSwapTime)
                {
                    SetLabelMessage(mainEntity, ecb);
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

    private void SetLabelMessage(Entity mainEntity, EntityCommandBuffer ecb)
    {
        var setLabel = new SetLabelTextBufferElement()
        {
            Value = LabelMode.Message
        };
        ecb.AppendToBuffer(mainEntity, setLabel);
        m_labelMode = LabelMode.Message;
    }

    private void SwitchToPlayingPhase(Entity mainEntity, EntityCommandBuffer ecb)
    {
        var setLabel = new SetLabelTextBufferElement()
        {
            Value = LabelMode.None
        };
        ecb.AppendToBuffer(mainEntity, setLabel);
        ecb.RemoveComponent<LevelStartPhaseTag>(mainEntity);
        ecb.AddComponent<LevelPlayingPhaseTag>(mainEntity);
    }
}

