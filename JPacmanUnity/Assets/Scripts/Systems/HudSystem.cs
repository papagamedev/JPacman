using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollectibleSystem))]
public partial class HudSystem : SystemBase
{
    public Action<LevelStartPhaseSystem.LabelMode> OnSetLabelText;
    public Action<int> OnSetLivesText;
    public Action<int> OnSetScoreText;
    public Action<int, float3> OnStartScoreAnimation;

    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate<Main>();
           
    }

    protected override void OnUpdate()
    {
        var main = SystemAPI.GetSingletonEntity<Main>();
        var mainAspect = SystemAPI.GetAspect<MainAspect>(main);
        foreach (var element in mainAspect.SetLivesTextBuffer)
        {
            OnSetLivesText(element.Value);
        }
        foreach (var element in mainAspect.SetScoreTextBuffer)
        {
            OnSetScoreText(element.Value);
        }
        foreach (var element in mainAspect.SetLabelTextBuffer)
        {
            OnSetLabelText(element.Value);
        }
        foreach (var element in mainAspect.StartScoreAnimationBuffer)
        {
            OnStartScoreAnimation(element.Score, element.WorldPos);
        }
        mainAspect.SetLivesTextBuffer.Clear();
        mainAspect.SetScoreTextBuffer.Clear();
        mainAspect.SetLabelTextBuffer.Clear();
        mainAspect.StartScoreAnimationBuffer.Clear();
    }
}
