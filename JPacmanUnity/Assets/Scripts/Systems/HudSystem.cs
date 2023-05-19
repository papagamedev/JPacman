using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollectibleSystem))]
public partial class HudSystem : SystemBase
{
    public Action<HudEvents.LabelMessage, int> OnSetLabelText;
    public Action<float3> OnSetLabelPos; 
    public Action<int> OnSetLivesText;
    public Action<int> OnSetScoreText;
    public Action<int, float3> OnStartScoreAnimation;
    public Action<bool, float> OnFadeAnimation;

    private int m_levelNumber;
    private int m_roundNumber;

    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate<Game>();
           
    }

    protected override void OnUpdate()
    {
        var main = SystemAPI.GetSingletonEntity<Main>();
        var gameAspect = SystemAPI.GetAspect<GameAspect>(main);
        m_levelNumber = gameAspect.LevelData.LevelNumber;
        m_roundNumber = gameAspect.LevelData.RoundNumber;
        if (OnSetLivesText != null)
        {
            foreach (var element in gameAspect.SetLivesTextBuffer)
            {
                OnSetLivesText(element.Value);
            }
        }
        if (OnSetScoreText != null)
        {
            foreach (var element in gameAspect.SetScoreTextBuffer)
            {
                OnSetScoreText(element.Value);
            }
        }
        if (OnSetLabelText != null)
        {
            foreach (var element in gameAspect.SetLabelTextBuffer)
            {
                int value = 0;
                switch (element.Value)
                {
                    case HudEvents.LabelMessage.Round:
                        value = m_roundNumber;
                        break;

                    case HudEvents.LabelMessage.Level:
                        value = m_levelNumber;
                        break;
                }
                OnSetLabelText(element.Value, value);
            }
        }
        if (OnSetLabelPos != null)
        {
            foreach (var element in gameAspect.SetLabelPosBuffer)
            {
                OnSetLabelPos(element.Value);
            }
        }
        if (OnStartScoreAnimation != null)
        {
            foreach (var element in gameAspect.StartScoreAnimationBuffer)
            {
                OnStartScoreAnimation(element.Score, element.WorldPos);
            }
        }
        if (OnFadeAnimation != null)
        {
            foreach (var element in gameAspect.FadeAnimationBuffer)
            {
                OnFadeAnimation(element.IsFadeIn, element.Duration);
            }
        }
        gameAspect.SetLivesTextBuffer.Clear();
        gameAspect.SetScoreTextBuffer.Clear();
        gameAspect.SetLabelTextBuffer.Clear();
        gameAspect.SetLabelPosBuffer.Clear();
        gameAspect.StartScoreAnimationBuffer.Clear();
        gameAspect.FadeAnimationBuffer.Clear();
    }
}
